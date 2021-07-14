using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NgrokSharp
{
    public interface INgrokManager
    {
        event EventHandler DownloadAndUnZipDone;
        void DownloadNgrok();
        void RegisterAuthToken(string authtoken);
        void StartNgrok(NgrokManager.Region region = NgrokManager.Region.UnitedStates);
        void StopNgrok();
        Task<HttpResponseMessage> StartTunnel(StartTunnelDTO startTunnelDto);
        Task<HttpResponseMessage> StopTunnel(string name);
        Task<HttpResponseMessage> ListTunnels();
    }
}