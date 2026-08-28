using System.Application.Models;
using System.Net;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// 多 IP 并行延迟测速服务：对候选 IP 并行 TCP 建连测速，选出延迟最低者，并带 TTL 结果缓存。
    /// </summary>
    public interface ILatencyTestService
    {
        static ILatencyTestService Instance => DI.Get<ILatencyTestService>();

        /// <summary>
        /// 对加速条目的全部候选 IP 并行测速，返回最优 IP（有 TTL 缓存）。
        /// </summary>
        /// <param name="item">加速条目（读取 <see cref="AccelerateProjectDTO.ForwardIPs"/>/<see cref="AccelerateProjectDTO.PortId"/>）</param>
        /// <returns>最优 IP；无法连接任何候选时返回 <see langword="null"/></returns>
        Task<IPAddress?> SelectBestIpAsync(AccelerateProjectDTO item);

        /// <summary>
        /// 清空测速缓存
        /// </summary>
        void ClearCache();
    }
}