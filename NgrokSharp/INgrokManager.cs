using System;
using System.Net.Http;
using System.Threading.Tasks;
using NgrokSharp.DTO;

namespace NgrokSharp
{
    public interface INgrokManager : IDisposable
    {
        Task DownloadAndUnzipNgrokAsync();
        Task RegisterAuthTokenAsync(string authtoken);
        void StartNgrok(NgrokManager.Region region = NgrokManager.Region.UnitedStates);
        void StopNgrok();
        Task<HttpResponseMessage> StartTunnelAsync(StartTunnelDTO startTunnelDto);
        Task<HttpResponseMessage> StopTunnelAsync(string name);
        Task<HttpResponseMessage> ListTunnelsAsync();
    }
}