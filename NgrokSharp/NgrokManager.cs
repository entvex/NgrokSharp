using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mono.Unix;
using Newtonsoft.Json;
using NgrokSharp.DTO;
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
        private readonly Uri _ngrokLocalUrl;
        private readonly ILogger _logger;
        private readonly PlatformStrategy _platformCode;

        private readonly string _downloadFolder;

        /// <summary>
        ///     Constructor for NgrokManager
        /// </summary>
        public NgrokManager()
        {
            _httpClient = new HttpClient();
            _ngrokLocalUrl = new Uri("http://localhost:4040/api");
            _downloadFolder =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar}NgrokSharp{Path.DirectorySeparatorChar}";

            //Detect OS and set Platform and Url
            if (OperatingSystem.IsWindows())
            {
                _platformCode = new PlatformWindows();
                _ngrokDownloadUrl = new Uri("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-amd64.zip");
            }

            if (OperatingSystem.IsLinux())
            {
                _platformCode = new PlatformLinux();
                _ngrokDownloadUrl = new Uri("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-linux-amd64.zip");
            }

            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
        }
        
        /// <summary>
        ///     Constructor for NgrokManager. Only use this if you need logging.
        /// </summary>
        public NgrokManager(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();
            _ngrokLocalUrl = new Uri("http://localhost:4040/api");
            _downloadFolder =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar}NgrokSharp{Path.DirectorySeparatorChar}";

            //Detect OS and set Platform and Url
            if (OperatingSystem.IsWindows())
            {
                _platformCode = new PlatformWindows(_logger);
                _ngrokDownloadUrl = new Uri("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-amd64.zip");
            }

            if (OperatingSystem.IsLinux())
            {
                _platformCode = new PlatformLinux();
                _ngrokDownloadUrl = new Uri("https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-linux-amd64.zip");
            }

            if (!Directory.Exists(_downloadFolder))
            {
                Directory.CreateDirectory(_downloadFolder);
            }
        }

        /// <summary>
        ///     Downloads Ngrok.
        /// </summary>
        public async Task DownloadAndUnzipNgrokAsync(CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await _httpClient.GetAsync(_ngrokDownloadUrl, cancellationToken);
            httpResponseMessage.EnsureSuccessStatusCode();

            var readAsStreamAsync = await httpResponseMessage.Content.ReadAsStreamAsync();
            await using (var writer = File.Create($"{_downloadFolder}ngrok-stable-amd64.zip"))
            {
                await readAsStreamAsync.CopyToAsync(writer, cancellationToken);
            }

            await Task.Run(() => ZipFile.ExtractToDirectory($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, true));

            if (OperatingSystem.IsLinux())
            {
                UnixFileSystemInfo.GetFileSystemEntry($"{_downloadFolder}ngrok").FileAccessPermissions = FileAccessPermissions.UserReadWriteExecute;
                UnixFileSystemInfo.GetFileSystemEntry($"{_downloadFolder}ngrok-stable-amd64.zip").FileAccessPermissions = FileAccessPermissions.AllPermissions;
            }
            
            if (File.Exists($"{_downloadFolder}ngrok-stable-amd64.zip"))
            {
                File.Delete($"{_downloadFolder}ngrok-stable-amd64.zip");
            }
        }

        /// <summary>
        ///     Registers your authtoken, if empty your sessions will be restricted to 2 hours.
        /// </summary>
        /// <param name="authtoken">The token</param>
        public async Task RegisterAuthTokenAsync(string authtoken) => await _platformCode.RegisterAuthTokenAsync(authtoken);

        /// <summary>
        ///     Starts Ngrok
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
        /// Only use this if you passed ILogger into the constructor
        /// </summary>
        /// <param name="region"></param>
        public void StartNgrokWithLogging(Region region = Region.UnitedStates)
        {
            if (_logger == null)
            {
                throw new ArgumentNullException(nameof(_logger));
            }
            
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
            _platformCode.StartNgrokWithLogging(selectedRegion);
        }

        /// <summary>
        ///     Starts a Ngrok tunnel
        /// </summary>
        /// <param name="startTunnelDto"></param>
        /// <returns>A httpResponseMessage that can be parse into TunnelDetailDTO</returns>
        /// <exception cref="ArgumentNullException">The input, can't be null</exception>
        /// <exception cref="ArgumentException">Missing values in input</exception>
        public async Task<HttpResponseMessage> StartTunnelAsync(StartTunnelDTO startTunnelDto, CancellationToken cancellationToken = default)
        {
            if (startTunnelDto == null)
            {
                throw new ArgumentNullException(nameof(startTunnelDto));
            }
            if (string.IsNullOrWhiteSpace(startTunnelDto.addr))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.addr));
            }
            if (string.IsNullOrWhiteSpace(startTunnelDto.name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.name));
            }   
            if (string.IsNullOrWhiteSpace(startTunnelDto.proto))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.proto));
            }
            return await _httpClient.PostAsync($"{_ngrokLocalUrl}/tunnels", new StringContent(JsonConvert.SerializeObject(startTunnelDto), Encoding.UTF8, "application/json"),cancellationToken);
        }

        /// <summary>
        ///     Stops a ngrok tunnel
        /// </summary>
        /// <param name="name">Name of the tunnel to stop</param>
        /// <returns>A httpResponseMessage that will contain 204 status code, if successful</returns>
        public async Task<HttpResponseMessage> StopTunnelAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }
            return await _httpClient.DeleteAsync($"{_ngrokLocalUrl}/tunnels/{name}",cancellationToken);
        }

        /// <summary>
        ///     Gets a list of the tunnels
        /// </summary>
        /// <returns>A httpResponseMessage, that can be parse into TunnelsDetailsDTO </returns>
        public async Task<HttpResponseMessage> ListTunnelsAsync(CancellationToken cancellationToken = default) => await _httpClient.GetAsync($"{_ngrokLocalUrl}/tunnels",cancellationToken);

        /// <summary>
        ///     Stops Ngrok
        /// </summary>
        public void StopNgrok() => _platformCode.StopNgrok();

        public void Dispose() => _platformCode.Dispose();
    }
}