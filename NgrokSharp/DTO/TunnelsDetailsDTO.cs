using System;
using System.Text.Json.Serialization;

namespace NgrokSharp.DTO
{
    public partial class TunnelsDetailsDTO
    {
        [JsonPropertyName("tunnels")]
        public Tunnel[] Tunnels { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }

    public partial class Tunnel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("public_url")]
        public Uri PublicUrl { get; set; }

        [JsonPropertyName("proto")]
        public string Proto { get; set; }

        [JsonPropertyName("config")]
        public Config Config { get; set; }

        [JsonPropertyName("metrics")]
        public Metrics Metrics { get; set; }
    }
}