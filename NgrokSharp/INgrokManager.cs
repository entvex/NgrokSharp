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
        /// <summary>
        ///     Registers your authtoken, if empty your sessions will be restricted to 2 hours.
        /// </summary>
        /// <param name="authtoken">The token</param>
        Task RegisterAuthTokenAsync(string authtoken);
        /// <summary>
        ///     Starts Ngrok
        /// </summary>
        /// <param name="region">DataCenter region</param>
        void StartNgrok(NgrokManager.Region region = NgrokManager.Region.UnitedStates);
        /// <summary>
        /// Only use this if you passed ILogger into the constructor
        /// </summary>
        /// <param name="region">DataCenter region</param>
        void StartNgrokWithLogging(NgrokManager.Region region = NgrokManager.Region.UnitedStates);
        /// <summary>
        ///     Stops Ngrok
        /// </summary>
        void StopNgrok();
        /// <summary>
        ///     Starts a Ngrok tunnel
        /// </summary>
        /// <param name="startTunnelDto"></param>
        /// <returns>A httpResponseMessage that can be parse into TunnelDetailDTO</returns>
        /// <exception cref="ArgumentNullException">The input, can't be null</exception>
        /// <exception cref="ArgumentException">Missing values in input</exception>
        Task<HttpResponseMessage> StartTunnelAsync(StartTunnelDTO startTunnelDto, CancellationToken cancellationToken = default);
        /// <summary>
        ///     Stops a ngrok tunnel
        /// </summary>
        /// <param name="name">Name of the tunnel to stop</param>
        /// <returns>A httpResponseMessage that will contain 204 status code, if successful</returns>
        Task<HttpResponseMessage> StopTunnelAsync(string name, CancellationToken cancellationToken = default);
        /// <summary>
        ///     Gets a list of the tunnels
        /// </summary>
        /// <returns>A httpResponseMessage, that can be parse into TunnelsDetailsDTO </returns>
        Task<HttpResponseMessage> ListTunnelsAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Returns a list of all HTTP requests captured for inspection. This will only return requests that are still in memory (ngrok evicts captured requests when their memory usage exceeds inspect_db_size) 
        /// </summary>
        /// <param name="limit">maximum number of requests to return</param>
        /// <param name="cancellationToken"></param>
        /// <returns> A HttpResponseMessage that can be parsed into a CapturedRequestRootDTO</returns>
        Task<HttpResponseMessage> ListCapturedRequests(uint limit = 50, CancellationToken cancellationToken = default);
        /// <summary>
        /// Returns a list of all HTTP requests captured for inspection. This will only return requests that are still in memory (ngrok evicts captured requests when their memory usage exceeds inspect_db_size) 
        /// </summary>
        /// <param name="name">filter requests only for the given tunnel name</param>
        /// <param name="limit">maximum number of requests to return</param>
        /// <param name="cancellationToken"></param>
        /// <returns> A HttpResponseMessage that can be parsed into a CapturedRequestRootDTO</returns>
        Task<HttpResponseMessage> ListCapturedRequests(string name, uint limit = 50, CancellationToken cancellationToken = default);
        /// <summary>
        /// Returns metadata and raw bytes of a captured request. The raw data is base64-encoded in the JSON response.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">RequestId but not be null or whitespace</exception>
        Task<HttpResponseMessage> CapturedRequestDetail(string requestId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deletes all captured requests
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>204 status code with no response body</returns>
        Task<HttpResponseMessage> DeleteCapturedRequests(CancellationToken cancellationToken = default);
    }
}