#pragma warning disable CS1591
using System;
using System.Text.Json.Serialization;
#pragma warning disable CS8618

namespace NgrokSharp.DTO;

public class TunnelsDetailsDTO
{
    [JsonPropertyName("tunnels")] public Tunnel[] Tunnels { get; set; }

    [JsonPropertyName("uri")] public string Uri { get; set; }
}

public class Tunnel
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("uri")] public string Uri { get; set; }

    [JsonPropertyName("public_url")] public Uri PublicUrl { get; set; }

    [JsonPropertyName("proto")] public string Proto { get; set; }

    [JsonPropertyName("config")] public Config Config { get; set; }

    [JsonPropertyName("metrics")] public Metrics Metrics { get; set; }
}