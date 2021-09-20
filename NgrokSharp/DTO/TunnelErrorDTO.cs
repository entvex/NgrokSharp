using System.Text.Json.Serialization;

namespace NgrokSharp.DTO
{
    public partial class TunnelErrorDTO
    {
        [JsonPropertyName("error_code")]
        public long? ErrorCode { get; set; }

        [JsonPropertyName("status_code")]
        public long? StatusCode { get; set; }

        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("details")]
        public Details Details { get; set; }
    }

    public partial class Details
    {
        [JsonPropertyName("err")]
        public string Err { get; set; }
    }
}