using System;
using Newtonsoft.Json;

namespace NgrokSharp
{
    public partial class TunnelDetail
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri { get; set; }

        [JsonProperty("public_url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri PublicUrl { get; set; }

        [JsonProperty("proto", NullValueHandling = NullValueHandling.Ignore)]
        public string Proto { get; set; }

        [JsonProperty("config", NullValueHandling = NullValueHandling.Ignore)]
        public Config Config { get; set; }

        [JsonProperty("metrics", NullValueHandling = NullValueHandling.Ignore)]
        public Metrics Metrics { get; set; }
    }

    public partial class Config
    {
        [JsonProperty("addr", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Addr { get; set; }

        [JsonProperty("inspect", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Inspect { get; set; }
    }

    public partial class Metrics
    {
        [JsonProperty("conns", NullValueHandling = NullValueHandling.Ignore)]
        public Conns Conns { get; set; }

        [JsonProperty("http", NullValueHandling = NullValueHandling.Ignore)]
        public Conns Http { get; set; }
    }

    public partial class Conns
    {
        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public long? Count { get; set; }

        [JsonProperty("gauge", NullValueHandling = NullValueHandling.Ignore)]
        public long? Gauge { get; set; }

        [JsonProperty("rate1", NullValueHandling = NullValueHandling.Ignore)]
        public long? Rate1 { get; set; }

        [JsonProperty("rate5", NullValueHandling = NullValueHandling.Ignore)]
        public long? Rate5 { get; set; }

        [JsonProperty("rate15", NullValueHandling = NullValueHandling.Ignore)]
        public long? Rate15 { get; set; }

        [JsonProperty("p50", NullValueHandling = NullValueHandling.Ignore)]
        public long? P50 { get; set; }

        [JsonProperty("p90", NullValueHandling = NullValueHandling.Ignore)]
        public long? P90 { get; set; }

        [JsonProperty("p95", NullValueHandling = NullValueHandling.Ignore)]
        public long? P95 { get; set; }

        [JsonProperty("p99", NullValueHandling = NullValueHandling.Ignore)]
        public long? P99 { get; set; }
    }
}