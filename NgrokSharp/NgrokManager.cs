using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace NgrokSharp
{
    public class NgrokManager : INgrokManager
    {
        private Process _process;
        private WebClient _webClient;
        private HttpClient _httpClient;
        private readonly Uri _ngrokDownloadUrl = new Uri("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-amd64.zip");
        private readonly Uri _ngrokLocalUrl = new Uri("http://localhost:4040/api");

        public event EventHandler DownloadAndUnZipDone;

        protected virtual void OnDownloadAndUnZipDone(EventArgs e)
        {
            EventHandler handler = DownloadAndUnZipDone;
            handler?.Invoke(this, e);
        }

        public NgrokManager()
        {
            _httpClient = new HttpClient();

            _webClient = new WebClient();
            _webClient.DownloadFileCompleted += WebClientDownloadFileCompleted;
        }

        private void WebClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                Console.WriteLine("File download cancelled.");

            if (e.Error != null)
                Console.WriteLine(e.Error.ToString());
            
            UnzipNgrok();
        }

        private void UnzipNgrok()
        {
            FastZip fastZip = new FastZip();
            
            // Will always overwrite if target filenames already exist
            fastZip.ExtractZip("ngrok-stable-windows-amd64.zip", Directory.GetCurrentDirectory(), null);

            if (File.Exists("ngrok-stable-windows-amd64.zip"))
                File.Delete("ngrok-stable-windows-amd64.zip");

            OnDownloadAndUnZipDone(EventArgs.Empty);
        }

        public void DownloadNgrok()
        {
            _webClient.DownloadFileAsync(_ngrokDownloadUrl, "ngrok-stable-windows-amd64.zip");
        }

        public void RegisterAuthToken(string authtoken)
        {
            if (_process != null)
            {
                _process.Refresh();
                if (!_process.HasExited)
                    throw new Exception("The Ngrok process is already running. Please use StopNgrok() and then register the AuthToken again.");
            }
            
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "ngrok.exe";
            startInfo.Arguments = $"authtoken {authtoken}";
            process.StartInfo = startInfo;
            process.Start();

        }

        public void StartNgrok()
        {
            _process = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "ngrok.exe",
                    Arguments = "start --none"
                }
            };

            _process.Start();
        }

        public void StopNgrok()
        {
            if (_process != null)
            {
                _process.Refresh();
                if (!_process.HasExited)
                {
                    _process.Kill();
                }
            }
        }

        public async Task<HttpResponseMessage> StartTunnel(StartTunnelDTO startTunnelDto)
        {
            if (startTunnelDto == null) throw new ArgumentNullException(nameof(startTunnelDto));

            if (string.IsNullOrWhiteSpace(startTunnelDto.addr))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.addr));
            if (string.IsNullOrWhiteSpace(startTunnelDto.name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.name));
            if (string.IsNullOrWhiteSpace(startTunnelDto.proto))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.proto));

            return await _httpClient.PostAsync($"{_ngrokLocalUrl}/tunnels", new StringContent(JsonConvert.SerializeObject(startTunnelDto), Encoding.UTF8, "application/json"));
        }

        /// <summary>
        /// Stops a ngrok tunnel
        /// </summary>
        /// <param name="name">Name of the tunnel to stop</param>
        /// <returns>204 status code with an empty body</returns>
        public async Task<int> StopTunnel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            var httpResponseMessage = await _httpClient.DeleteAsync($"{_ngrokLocalUrl}/tunnels/{name}");

            return (int)httpResponseMessage.StatusCode;
        }

        public async Task<int> ListTunnels()
        {
            var httpResponseMessage = await _httpClient.GetAsync($"{_ngrokLocalUrl}/tunnels");

            return (int)httpResponseMessage.StatusCode;
        }
    }
}