#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NgrokSharp.DTO;
using NgrokSharp.PlatformSpecific;
using NgrokSharp.PlatformSpecific.Linux;
using NgrokSharp.PlatformSpecific.Windows;

namespace NgrokSharp;

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

    private readonly Dictionary<Region, string> _regions = new()
    {
        { Region.UnitedStates, "us" },
        { Region.Europe, "eu" },
        { Region.AsiaPacific, "ap" },
        { Region.Australia, "au" },
        { Region.SouthAmerica, "sa" },
        { Region.Japan, "jp" },
        { Region.India, "in" }
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private const string NgrokDownloadUrlWindows = "https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-windows-amd64.zip";
    private const string NgrokDownloadUrlLinux = "https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-linux-amd64.zip";
    private readonly Uri _ngrokDownloadUrl;
    private readonly Uri _ngrokLocalUrl;

    private string _downloadFolder;
    private PlatformStrategy _platformCode;
    
    /// <summary>
    /// Constructor for NgrokManager. Only use this if you need logging.
    /// </summary>
    /// <param name="logger">The logger is optional</param>
    public NgrokManager(ILogger? logger = null)
    {
        _httpClient = new HttpClient();
        _ngrokLocalUrl = new Uri("http://localhost:4040/api");
        _downloadFolder =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar}NgrokSharp{Path.DirectorySeparatorChar}";

        //Detect OS and set Platform and Url
        if (OperatingSystem.IsWindows())
        {
            _platformCode = new PlatformWindows(_logger);
            _ngrokDownloadUrl = new Uri(NgrokDownloadUrlWindows);
        }

        if (OperatingSystem.IsLinux())
        {
            _platformCode = new PlatformLinux(_logger);
            _ngrokDownloadUrl = new Uri(NgrokDownloadUrlLinux);
        }

        if (!Directory.Exists(_downloadFolder)) Directory.CreateDirectory(_downloadFolder);
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

        await Task.Run(() =>
            ZipFile.ExtractToDirectory($"{_downloadFolder}ngrok-stable-amd64.zip", _downloadFolder, true));

        if (File.Exists($"{_downloadFolder}ngrok-stable-amd64.zip"))
            File.Delete($"{_downloadFolder}ngrok-stable-amd64.zip");
    }

    /// <summary>
    ///     Sets the path to a directory that contains the Ngrok executable. Only use this method you don't wish NgrokSharp to
    ///     manage the ngrok executable. <see cref="DownloadAndUnzipNgrokAsync" />
    /// </summary>
    /// <param name="pathToExecutable">Path to a directory that contains the Ngrok executable</param>
    public void SetNgrokDirectory(string pathToExecutable)
    {
        //Detect OS and set Platform
        if (OperatingSystem.IsWindows()) _platformCode = new PlatformWindows();

        if (OperatingSystem.IsLinux()) _platformCode = new PlatformLinux();

        _downloadFolder = pathToExecutable;
    }

    /// <summary>
    ///     Sets the path to a directory that contains the Ngrok executable. Only use this method you don't wish NgrokSharp to
    ///     manage the ngrok executable. <see cref="DownloadAndUnzipNgrokAsync" />
    /// </summary>
    /// <param name="pathToExecutable">Path to a directory that contains the Ngrok executable</param>
    /// <param name="logger"></param>
    public void SetNgrokDirectory(string pathToExecutable, ILogger logger)
    {
        //Detect OS and set Platform and Url
        if (OperatingSystem.IsWindows()) _platformCode = new PlatformWindows(_logger);

        if (OperatingSystem.IsLinux()) _platformCode = new PlatformLinux(_logger);

        _downloadFolder = pathToExecutable;
    }

    /// <summary>
    ///     Registers your authtoken, if empty your sessions will be restricted to 2 hours.
    /// </summary>
    /// <param name="authtoken">The token</param>
    public async Task RegisterAuthTokenAsync(string authtoken)
    {
        await _platformCode.RegisterAuthTokenAsync(authtoken);
    }

    /// <summary>
    ///     Starts Ngrok
    /// </summary>
    /// <param name="region">DataCenter region</param>
    public void StartNgrok(Region region = Region.UnitedStates)
    {
        var selectedRegion = _regions.First(x => x.Key == region).Value;
        _platformCode.StartNgrok(selectedRegion);
    }

    /// <summary>
    ///     Only use this if you passed ILogger into the constructor
    /// </summary>
    /// <param name="region"></param>
    public void StartNgrokWithLogging(Region region = Region.UnitedStates)
    {
        if (_logger == null) throw new ArgumentNullException(nameof(_logger));
        
        var selectedRegion = _regions.First(x => x.Key == region).Value;
        _platformCode.StartNgrokWithLogging(selectedRegion);
    }

    /// <summary>
    ///     Starts a Ngrok tunnel
    /// </summary>
    /// <param name="startTunnelDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A httpResponseMessage that can be parse into TunnelDetailDTO</returns>
    /// <exception cref="ArgumentNullException">The input, can't be null</exception>
    /// <exception cref="ArgumentException">Missing values in input</exception>
    public async Task<HttpResponseMessage> StartTunnelAsync(StartTunnelDTO startTunnelDto,
        CancellationToken cancellationToken = default)
    {
        if (startTunnelDto == null) throw new ArgumentNullException(nameof(startTunnelDto));
        if (string.IsNullOrWhiteSpace(startTunnelDto.addr))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.addr));
        if (string.IsNullOrWhiteSpace(startTunnelDto.name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.name));
        if (string.IsNullOrWhiteSpace(startTunnelDto.proto))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(startTunnelDto.proto));
        return await _httpClient.PostAsync($"{_ngrokLocalUrl}/tunnels",
            new StringContent(JsonSerializer.Serialize(startTunnelDto), Encoding.UTF8, "application/json"),
            cancellationToken);
    }

    /// <summary>
    ///     Stops a ngrok tunnel
    /// </summary>
    /// <param name="name">Name of the tunnel to stop</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A httpResponseMessage that will contain 204 status code, if successful</returns>
    public async Task<HttpResponseMessage> StopTunnelAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        return await _httpClient.DeleteAsync($"{_ngrokLocalUrl}/tunnels/{name}", cancellationToken);
    }

    /// <summary>
    ///     Returns a list of all HTTP requests captured for inspection. This will only return requests that are still in
    ///     memory (ngrok evicts captured requests when their memory usage exceeds inspect_db_size)
    /// </summary>
    /// <param name="limit">maximum number of requests to return</param>
    /// <param name="cancellationToken"></param>
    /// <returns> A HttpResponseMessage that can be parsed into a CapturedRequestRootDTO</returns>
    public async Task<HttpResponseMessage> ListCapturedRequests(uint limit = 50,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync($"{_ngrokLocalUrl}/requests/http?limit={limit}", cancellationToken);
    }

    /// <summary>
    ///     Returns a list of all HTTP requests captured for inspection. This will only return requests that are still in
    ///     memory (ngrok evicts captured requests when their memory usage exceeds inspect_db_size)
    /// </summary>
    /// <param name="name">filter requests only for the given tunnel name</param>
    /// <param name="limit">maximum number of requests to return</param>
    /// <param name="cancellationToken"></param>
    /// <returns> A HttpResponseMessage that can be parsed into a CapturedRequestRootDTO</returns>
    public async Task<HttpResponseMessage> ListCapturedRequests(string name, uint limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        return await _httpClient.GetAsync($"{_ngrokLocalUrl}/requests/http?tunnel_name={name}&limit={limit}",
            cancellationToken);
    }

    /// <summary>
    ///     Gets a list of the tunnels
    /// </summary>
    /// <returns>A httpResponseMessage, that can be parse into TunnelsDetailsDTO </returns>
    public async Task<HttpResponseMessage> ListTunnelsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync($"{_ngrokLocalUrl}/tunnels", cancellationToken);
    }

    /// <summary>
    ///     Returns metadata and raw bytes of a captured request. The raw data is base64-encoded in the JSON response.
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">RequestId but not be null or whitespace</exception>
    public async Task<HttpResponseMessage> CapturedRequestDetail(string requestId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(requestId));

        return await _httpClient.GetAsync($"{_ngrokLocalUrl}/requests/http/{requestId}", cancellationToken);
    }

    /// <summary>
    ///     Deletes all captured requests
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>204 status code with no response body</returns>
    public async Task<HttpResponseMessage> DeleteCapturedRequests(CancellationToken cancellationToken = default)
    {
        return await _httpClient.DeleteAsync($"{_ngrokLocalUrl}/requests/http", cancellationToken);
    }

    /// <summary>
    ///     Stops Ngrok
    /// </summary>
    public void StopNgrok()
    {
        _platformCode.StopNgrok();
    }

    public void Dispose()
    {
        _platformCode.Dispose();
    }
}