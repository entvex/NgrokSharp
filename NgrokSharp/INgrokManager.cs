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
        void StartNgrok();
        void StopNgrok();
        Task<HttpResponseMessage> StartTunnel(StartTunnelDTO startTunnelDto);
        Task<int> StopTunnel(string name);
        Task<int> ListTunnels();
    }
}