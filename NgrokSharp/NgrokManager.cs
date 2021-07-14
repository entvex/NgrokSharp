using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Mono.Unix;
using Newtonsoft.Json;
using NgrokSharp.PlatformSpecific;
using NgrokSharp.PlatformSpecific.Linux;
using NgrokSharp.PlatformSpecific.Windows;

namespace NgrokSharp
{
    public class NgrokManager : INgrokManager
    {
        /// <summary>
        ///     Configure one of the supported regions to be used. https://ngrok.com/docs#global-locations
        /// </summary>
        public enum Region
        {
            UnitedStates,
            Europe,
            AsiaPacific,
            Australia,
            SouthAmerica,
            Japan,
            India
        }

        private readonly HttpClient _httpClient;

        private readonly Uri _ngrokDownloadUrl;
        private readonly Uri _ngrokLocalUrl = new("http://localhost:4040/api");

        private readonly PlatformCode _platformCode;

        private readonly WebClient _webClient;
        
        /// <summary>
        /// Constructor for NgrokManager
        /// </summary>
        public NgrokManager()
        {
            _httpClient = new HttpClient();
            _webClient = new WebClient();
            _webClient.DownloadFileCompleted += WebClientDownloadFileCompleted;

            //Detect OS and set Platform and Url
            if (OperatingSystem.IsWindows())
            {
                _platformCode = new PlatformCode(new PlatformWindows());
                _ngrokDownloadUrl = new Uri("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-amd64.zip");
            }

            if (OperatingSystem.IsLinux())
            {
                _platformCode = new PlatformCode(new PlatformLinux());
                _ngrokDownloadUrl = new Uri("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-linux-amd64.zip");
            }
        }

        /// <summary>
        /// Event fires when Ngrok have been downloaded and unzipped.
        /// </summary>
        public event EventHandler DownloadAndUnZipDone;

        /// <summary>
        /// Downloads Ngrok.
        /// </summary>
        public void DownloadNgrok()
        {
            _webClient.DownloadFileAsync(_ngrokDownloadUrl, "ngrok-stable-amd64.zip");
        }

        /// <summary>
        /// Registers your authtoken, if empty your sessions will be restricted to 2 hours.
        /// </summary>
        /// <param name="authtoken">The token</param>
        public void RegisterAuthToken(string authtoken)
        {
            _platformCode.RegisterAuthToken(authtoken);
        }
        
        /// <summary>
        /// Starts Ngrok
        /// </summary>
        /// <param name="region">DataCenter region</param>
        public void StartNgrok(Region region = Region.UnitedStates)
        {
            var regions = new Dictionary<Region, string>
            {
                {Region.UnitedStates, "us"},
                {Region.Europe, "eu"},
                {Region.AsiaPacific, "ap"},
                {Region.Australia, "au"},
                {Region.SouthAmerica, "sa"},
                {Region.Japan, "jp"},
                {Region.India, "in"}
            };

            var selectedRegion = regions.First(x => x.Key == region).Value;

            _platformCode.StartNgrok(selectedRegion);
        }
        
        /// <summary>
        /// Starts a Ngrok tunnel
        /// </summary>
        /// <param name="startTunnelDto"></param>
        /// <returns>A httpResponseMessage that can be parse into TunnelDetailDTO</returns>
        /// <exception cref="ArgumentNullException">The input, can't be null</exception>
        /// <exception cref="ArgumentException">Missing values in input</exception>
        public async Task<HttpResponseMessage> StartTunnel(StartTunnelDTO startTunnelDto)
        {
            if (startTunnelDto == null) throw new ArgumentNullException(nameof(startTunnelDto));

            if (string.IsNullOrWhiteSpace(startTunnelDto.addr))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.addr));
            if (string.IsNullOrWhiteSpace(startTunnelDto.name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.name));
            if (string.IsNullOrWhiteSpace(startTunnelDto.proto))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.proto));

            return await _httpClient.PostAsync($"{_ngrokLocalUrl}/tunnels",
                new StringContent(JsonConvert.SerializeObject(startTunnelDto), Encoding.UTF8, "application/json"));
        }

        /// <summary>
        ///     Stops a ngrok tunnel
        /// </summary>
        /// <param name="name">Name of the tunnel to stop</param>
        /// <returns>A httpResponseMessage that will contain 204 status code, if successful</returns>
        public async Task<HttpResponseMessage> StopTunnel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            var httpResponseMessage = await _httpClient.DeleteAsync($"{_ngrokLocalUrl}/tunnels/{name}");

            return httpResponseMessage;
        }
        
        /// <summary>
        /// Gets a list of the tunnels
        /// </summary>
        /// <returns>A httpResponseMessage, that can be parse into TunnelsDetailsDTO </returns>
        public async Task<HttpResponseMessage> ListTunnels()
        {
            var httpResponseMessage = await _httpClient.GetAsync($"{_ngrokLocalUrl}/tunnels");

            return httpResponseMessage;
        }
        
        protected virtual void OnDownloadAndUnZipDone(EventArgs e)
        {
            var handler = DownloadAndUnZipDone;
            handler?.Invoke(this, e);
        }

        private void WebClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                throw new TaskCanceledException("File download cancelled.");

            if (e.Error != null)
                throw new Exception(e.Error.ToString());

            UnzipNgrok();
        }

        private void UnzipNgrok()
        {
            var fastZip = new FastZip();

            // Will always overwrite if target filenames already exist
            fastZip.ExtractZip("ngrok-stable-amd64.zip", Directory.GetCurrentDirectory(), null);

            if (OperatingSystem.IsLinux())
            {
                UnixFileSystemInfo.GetFileSystemEntry("ngrok").FileAccessPermissions =
                    FileAccessPermissions.UserReadWriteExecute;
                UnixFileSystemInfo.GetFileSystemEntry("ngrok-stable-amd64.zip").FileAccessPermissions =
                    FileAccessPermissions.UserReadWriteExecute;
            }

            if (File.Exists("ngrok-stable-amd64.zip"))
                File.Delete("ngrok-stable-amd64.zip");

            OnDownloadAndUnZipDone(EventArgs.Empty);
        }
        
        /// <summary>
        /// Stops Ngrok
        /// </summary>
        public void StopNgrok()
        {
            _platformCode.StopNgrok();
        }
    }
}