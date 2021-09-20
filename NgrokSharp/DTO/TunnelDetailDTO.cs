using System;
using System.Text.Json.Serialization;
using NgrokSharp.JsonConverter;

namespace NgrokSharp.DTO
{
    public partial class TunnelDetailDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("public_url"), JsonConverter(typeof(JsonConverterUri))]
        public Uri PublicUrl { get; set; }

        [JsonPropertyName("proto")]
        public string Proto { get; set; }

        [JsonPropertyName("config")]
        public Config Config { get; set; }

        [JsonPropertyName("metrics")]
        public Metrics Metrics { get; set; }
    }

    public partial class Config
    {
        [JsonPropertyName("addr")]
        public Uri Addr { get; set; }

        [JsonPropertyName("inspect")]
        public bool? Inspect { get; set; }
    }

    public partial class Metrics
    {
        [JsonPropertyName("conns")]
        public Conns Conns { get; set; }

        [JsonPropertyName("http")]
        public Conns Http { get; set; }
    }

    public partial class Conns
    {
        [JsonPropertyName("count")]
        public long? Count { get; set; }

        [JsonPropertyName("gauge")]
        public long? Gauge { get; set; }

        [JsonPropertyName("rate1")]
        public long? Rate1 { get; set; }

        [JsonPropertyName("rate5")]
        public long? Rate5 { get; set; }

        [JsonPropertyName("rate15")]
        public long? Rate15 { get; set; }

        [JsonPropertyName("p50")]
        public long? P50 { get; set; }

        [JsonPropertyName("p90")]
        public long? P90 { get; set; }

        [JsonPropertyName("p95")]
        public long? P95 { get; set; }

        [JsonPropertyName("p99")]
        public long? P99 { get; set; }
    }
}