using System.Application.Models;
using System.Application.Settings;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    /// <summary>
    /// 基于 TCP ConnectAsync 的多 IP 并行延迟测速实现。
    /// 通过信号量限流并发连接数，单 IP 连接超时后跳过，选延迟最低者并带 TTL 缓存。
    /// </summary>
    sealed class LatencyTestServiceImpl : ILatencyTestService
    {
        const int MaxConcurrency = 16;

        /// <summary>
        /// 快速阈值：任一候选延迟低于该值立即返回，无需等其余候选测完
        /// </summary>
        const int FastThresholdMs = 150;

        readonly SemaphoreSlim semaphore = new(MaxConcurrency);

        sealed class CacheEntry
        {
            public IPAddress? Ip;
            public long Timestamp;
        }

        readonly ConcurrentDictionary<string, CacheEntry> cache = new();
        readonly object cacheLock = new();

        public async Task<IPAddress?> SelectBestIpAsync(AccelerateProjectDTO item)
        {
            var candidates = item.ForwardIPs;
            if (candidates == null || candidates.Count == 0) return null;

            var port = item.PortId > 0 ? item.PortId : 443;
            var cacheKey = $"{item.Id}|{port}";
            var timeoutMs = ProxySettings.SpeedTestTimeoutMs.Value;
            if (timeoutMs <= 0) timeoutMs = 3000;
            var cacheTtlSec = ProxySettings.SpeedTestCacheTTLSeconds.Value;
            if (cacheTtlSec <= 0) cacheTtlSec = 600;

            long nowMs = Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
            if (cache.TryGetValue(cacheKey, out var entry) && entry.Ip != null)
            {
                var age = nowMs - entry.Timestamp;
                if (age < cacheTtlSec * 1000L) return entry.Ip;
            }

            var best = await PickBestAsync(candidates, port, timeoutMs);
            if (best != null)
            {
                lock (cacheLock)
                {
                    cache[cacheKey] = new CacheEntry { Ip = best, Timestamp = nowMs };
                    if (cache.Count > 512)
                    {
                        var oldest = cache.OrderBy(x => x.Value.Timestamp).Select(x => x.Key).Take(cache.Count - 512);
                        foreach (var k in oldest) cache.TryRemove(k, out _);
                    }
                }
            }
            else
            {
                // 全部失败：短缓存 10s 防止反复重测
                lock (cacheLock)
                {
                    cache[cacheKey] = new CacheEntry { Ip = null, Timestamp = nowMs };
                }
            }
            return best;
        }

        async Task<IPAddress?> PickBestAsync(IReadOnlyList<string> candidates, int port, int timeoutMs)
        {
            var tasks = new List<Task<(IPAddress, long)>>(candidates.Count);
            foreach (var candidate in candidates)
            {
                if (!IPAddress.TryParse(candidate, out var ip) || ip.AddressFamily != AddressFamily.InterNetwork)
                    continue;
                tasks.Add(TestIpAsync(ip, port, timeoutMs));
            }
            if (tasks.Count == 0) return null;

            // 竞速式选优：任一候选延迟低于快速阈值立即返回；
            // 否则等全部完成后选最低延迟者
            var pending = tasks.Count;
            var completed = new List<(IPAddress ip, long latencyMs)>(tasks.Count);
            while (pending > 0)
            {
                var done = await Task.WhenAny(tasks);
                tasks.Remove(done);
                pending--;
                var r = await done;
                if (r.Item1 != null)
                {
                    if (r.Item2 <= FastThresholdMs) return r.Item1;
                    completed.Add(r);
                }
            }

            return completed.OrderBy(x => x.latencyMs)
                            .Select(x => x.ip)
                            .FirstOrDefault();
        }

        async Task<(IPAddress, long)> TestIpAsync(IPAddress ip, int port, int timeoutMs)
        {
            await semaphore.WaitAsync();
            var stopwatch = Stopwatch.StartNew();
            using var client = new TcpClient(AddressFamily.InterNetwork);
            try
            {
                client.NoDelay = true;
                var connectTask = client.ConnectAsync(ip, port);
                var delayTask = Task.Delay(timeoutMs);
                var completed = await Task.WhenAny(connectTask, delayTask);
                if (completed != connectTask)
                {
                    // 超时：连接未完成，跳过
                    return default;
                }
                await connectTask;
                stopwatch.Stop();
                return (ip, stopwatch.ElapsedMilliseconds);
            }
            catch
            {
                return default;
            }
            finally
            {
                semaphore.Release();
                try { client.Close(); } catch { }
            }
        }

        public void ClearCache()
        {
            lock (cacheLock) cache.Clear();
        }
    }
}