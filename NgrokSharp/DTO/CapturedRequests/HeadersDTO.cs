#pragma warning disable CS1591
namespace NgrokSharp.DTO;

public class Headers
{
    public string[] Accept { get; set; }
    public string[] AcceptEncoding { get; set; }
    public string[] AcceptLanguage { get; set; }
    public string[] Cookie { get; set; }
    public string[] Dnt { get; set; }
    public string[] Host { get; set; }
    public string[] Referer { get; set; }
    public string[] SecFetchDest { get; set; }
    public string[] SecFetchMode { get; set; }
    public string[] SecFetchSite { get; set; }
    public string[] SecGpc { get; set; }
    public string[] Te { get; set; }
    public string[] UserAgent { get; set; }
    public string[] XForwardedFor { get; set; }
    public string[] XForwardedProto { get; set; }
    public string[] CacheControl { get; set; }
    public string[] Connection { get; set; }
    public string[] Origin { get; set; }
    public string[] Pragma { get; set; }
    public string[] SecWebsocketExtensions { get; set; }
    public string[] SecWebsocketKey { get; set; }
    public string[] SecWebsocketVersion { get; set; }
    public string[] Upgrade { get; set; }
    public string[] SecFetchUser { get; set; }
    public string[] UpgradeInsecureRequests { get; set; }
}