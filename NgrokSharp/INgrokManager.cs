using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NgrokSharp.DTO;

namespace NgrokSharp
{
    public interface INgrokManager : IDisposable
    {
        Task DownloadAndUnzipNgrokAsync(CancellationToken cancellationToken = default);
        Task RegisterAuthTokenAsync(string authtoken);
        void StartNgrok(NgrokManager.Region region = NgrokManager.Region.UnitedStates);
        void StartNgrokWithLogging(NgrokManager.Region region = NgrokManager.Region.UnitedStates);
        void StopNgrok();
        Task<HttpResponseMessage> StartTunnelAsync(StartTunnelDTO startTunnelDto, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> StopTunnelAsync(string name, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> ListTunnelsAsync(CancellationToken cancellationToken = default);
    }
}