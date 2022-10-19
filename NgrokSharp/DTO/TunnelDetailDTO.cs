#pragma warning disable CS1591
using System;
using System.Text.Json.Serialization;
using NgrokSharp.JsonConverter;
#pragma warning disable CS8618

namespace NgrokSharp.DTO;

public class TunnelDetailDTO
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("uri")] public string Uri { get; set; }

    [JsonPropertyName("public_url")]
    [JsonConverter(typeof(JsonConverterUri))]
    public Uri PublicUrl { get; set; }

    [JsonPropertyName("proto")] public string Proto { get; set; }

    [JsonPropertyName("config")] public Config Config { get; set; }

    [JsonPropertyName("metrics")] public Metrics Metrics { get; set; }
}

public class Config
{
    [JsonPropertyName("addr")] public Uri Addr { get; set; }

    [JsonPropertyName("inspect")] public bool? Inspect { get; set; }
}

public class Metrics
{
    [JsonPropertyName("conns")] public Conns Conns { get; set; }

    [JsonPropertyName("http")] public Conns Http { get; set; }
}

public class Conns
{
    [JsonPropertyName("count")] public long? Count { get; set; }

    [JsonPropertyName("gauge")] public long? Gauge { get; set; }

    [JsonPropertyName("rate1")] public long? Rate1 { get; set; }

    [JsonPropertyName("rate5")] public long? Rate5 { get; set; }

    [JsonPropertyName("rate15")] public long? Rate15 { get; set; }

    [JsonPropertyName("p50")] public long? P50 { get; set; }

    [JsonPropertyName("p90")] public long? P90 { get; set; }

    [JsonPropertyName("p95")] public long? P95 { get; set; }

    [JsonPropertyName("p99")] public long? P99 { get; set; }
}