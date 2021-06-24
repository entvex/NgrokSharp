using System;
using Newtonsoft.Json;

namespace NgrokSharp
{
    public partial class TunnelsDetailsDTO
    {
        [JsonProperty("tunnels", NullValueHandling = NullValueHandling.Ignore)]
        public Tunnel[] Tunnels { get; set; }

        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri { get; set; }
    }

    public partial class Tunnel
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
}