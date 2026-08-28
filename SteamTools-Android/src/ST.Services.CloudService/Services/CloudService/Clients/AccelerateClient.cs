using Newtonsoft.Json;
using System.Application.Models;
using System.Application.Services;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class AccelerateClient : ApiClient, IAccelerateClient
    {
        /// <summary>
        /// Fast Proxy 规则数据源地址（第三方 JSON，完全替换官方 api/Accelerate/All）
        /// </summary>
        public const string FastProxyRulesUri = "https://abhuang.dpdns.org/rules";

        public AccelerateClient(IApiConnection conn) : base(conn)
        {
        }

        public Task<IApiResponse<List<ScriptDTO>>> Scripts()
            => conn.SendAsync<List<ScriptDTO>>(
                isPolly: true,
                isAnonymous: true,
                isSecurity: false,
                method: HttpMethod.Get,
                requestUri: "api/Accelerate/Scripts",
                cancellationToken: default);

        public Task<IApiResponse<List<AccelerateProjectGroupDTO>>> All()
            => conn.SendAsync<List<AccelerateProjectGroupDTO>>(
                isPolly: true,
                isAnonymous: false,
                isSecurity: false,
                method: HttpMethod.Get,
                requestUri: "api/Accelerate/All",
                cancellationToken: default);

        public async Task<IApiResponse<List<AccelerateProjectGroupDTO>>> All(EReverseProxyEngine reverseProxyEngine)
        {
            try
            {
                var json = await IHttpService.Instance.GetAsync<string>(
                    FastProxyRulesUri,
                    accept: MediaTypeNames.JSON,
                    cancellationToken: CancellationToken.None);

                if (string.IsNullOrWhiteSpace(json))
                    return ApiResponse.Fail<List<AccelerateProjectGroupDTO>>("Fast Proxy /rules 返回空内容");

                var rules = JsonConvert.DeserializeObject<FastProxyRules>(json);
                if (rules?.Groups == null || rules.Groups.Count == 0)
                    return ApiResponse.Fail<List<AccelerateProjectGroupDTO>>("Fast Proxy /rules 解析为空");

                var groups = ApproveFastProxyRules(rules);
                if (groups.Count == 0)
                    return ApiResponse.Fail<List<AccelerateProjectGroupDTO>>("Fast Proxy /rules 无可用的加速条目");

                return ApiResponse.Ok(groups);
            }
            catch (Exception ex)
            {
                return ApiResponse.Exception<List<AccelerateProjectGroupDTO>>(ex);
            }
        }

        /// <summary>
        /// 将 Fast Proxy /rules 的第三方 JSON 映射为客户端加速数据。
        /// 纯正 hosts 策略：跳过占位符条目、跳过无 IP 条目、跳过空分组。
        /// </summary>
        static List<AccelerateProjectGroupDTO> ApproveFastProxyRules(FastProxyRules rules)
        {
            var groups = new List<AccelerateProjectGroupDTO>();
            var order = 0;
            foreach (var g in rules.Groups)
            {
                if (g.Entries == null || g.Entries.Count == 0) continue;

                var items = new List<AccelerateProjectDTO>();
                foreach (var e in g.Entries)
                {
                    if (e.IsPlaceholder) continue; // 占位符(如 {Cloudflare})不需要
                    var ips = e.Ips?.Where(IPAddress2.IsValidIPv4)?.ToList() ?? new();
                    if (ips.Count == 0) continue;   // 无 IP 的条目无意义

                    var name = string.IsNullOrWhiteSpace(e.NameZh) ? e.Name : e.NameZh;
                    var domainNames = e.Domains?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                    if (domainNames == null || domainNames.Length == 0) continue;

                    items.Add(new AccelerateProjectDTO
                    {
                        Name = name ?? string.Empty,
                        PortId = ParsePort(e.Port),
                        DomainNames = string.Join(Constants.GeneralSeparator, domainNames),
                        ForwardDomainIP = ips[0],          // 默认取第一个 IP
                        ForwardIPs = ips,                  // 全部候选 IP（测速选优用）
                        IconUrl = BuildIconUrl(e),         // 第三方图标
                        ServerName = string.Empty,          // 无需 ServerName，直连 IP
                        ProxyType = ProxyType.Local,
                        Id = CreateStableId(e),            // 确定性 Guid，勾选状态跨版本稳定
                        Order = order++,
                    });
                }
                if (items.Count == 0) continue; // 该分组全是占位符/无效条目则跳过

                groups.Add(new AccelerateProjectGroupDTO
                {
                    Name = string.IsNullOrWhiteSpace(g.GroupZh) ? g.Group : g.GroupZh,
                    Items = items,
                    ImageId = CreateStableId(g.Group ?? g.GroupZh ?? string.Empty),
                    IconUrl = BuildGroupIconUrl(g),
                    Order = groups.Count,
                });
            }
            return groups;
        }

        static ushort ParsePort(string? port)
        {
            if (ushort.TryParse(port, out var p) && p > 0) return p;
            return 443;
        }

        /// <summary>
        /// 构建分组图标 URL（优先分组自带 URL，否则用 /icon/&lt;group&gt;）
        /// </summary>
        static string? BuildGroupIconUrl(FastProxyGroup g)
        {
            if (!string.IsNullOrWhiteSpace(g.IconUrl)) return g.IconUrl;
            if (string.IsNullOrWhiteSpace(g.Group)) return null;
            return $"https://abhuang.dpdns.org/icon/{Uri.EscapeDataString(g.Group)}";
        }

        /// <summary>
        /// 构建加速条目图标 URL（优先条目自带 URL，否则用 /icon/entry/&lt;id&gt;）
        /// </summary>
        static string? BuildIconUrl(FastProxyEntry e)
        {
            if (!string.IsNullOrWhiteSpace(e.IconUrl)) return e.IconUrl;
            if (string.IsNullOrWhiteSpace(e.Id)) return null;
            return $"https://abhuang.dpdns.org/icon/entry/{e.Id}";
        }

        /// <summary>
        /// 使用确定性的字符串哈希生成稳定 Guid，保证 SupportProxyServicesStatus 勾选状态跨版本不丢。
        /// </summary>
        static Guid CreateStableId(object key)
        {
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(key?.ToString() ?? string.Empty));
            return new Guid(bytes);
        }
    }

    /// <summary>
    /// Fast Proxy /rules 第三方 JSON 结构
    /// </summary>
    sealed class FastProxyRules
    {
        [JsonProperty("meta")]
        public FastProxyMeta? Meta { get; set; }

        [JsonProperty("groups")]
        public List<FastProxyGroup>? Groups { get; set; }

        [JsonProperty("filtered")]
        public List<object>? Filtered { get; set; }
    }

    sealed class FastProxyMeta
    {
        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("update_time")]
        public string? UpdateTime { get; set; }
    }

    sealed class FastProxyGroup
    {
        [JsonProperty("group")]
        public string? Group { get; set; }

        [JsonProperty("groupZh")]
        public string? GroupZh { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("iconSlug")]
        public string? IconSlug { get; set; }

        [JsonProperty("iconUrl")]
        public string? IconUrl { get; set; }

        [JsonProperty("entries")]
        public List<FastProxyEntry>? Entries { get; set; }
    }

    sealed class FastProxyEntry
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("nameZh")]
        public string? NameZh { get; set; }

        [JsonProperty("ips")]
        public List<string>? Ips { get; set; }

        [JsonProperty("domains")]
        public List<string>? Domains { get; set; }

        [JsonProperty("port")]
        public string? Port { get; set; }

        [JsonProperty("cert")]
        public string? Cert { get; set; }

        [JsonProperty("isPlaceholder")]
        public bool IsPlaceholder { get; set; }

        [JsonProperty("iconSlug")]
        public string? IconSlug { get; set; }

        [JsonProperty("iconUrl")]
        public string? IconUrl { get; set; }
    }

    static class IPAddress2
    {
        public static bool IsValidIPv4(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (!System.Net.IPAddress.TryParse(s, out var ip)) return false;
            // 只保留 IPv4，过滤 IPv6 与本地/保留地址
            if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;
            if (System.Net.IPAddress.IsLoopback(ip)) return false;
            return !ip.Equals(System.Net.IPAddress.Any);
        }
    }
}