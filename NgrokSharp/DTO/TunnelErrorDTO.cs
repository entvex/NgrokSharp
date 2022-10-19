#pragma warning disable CS1591
using System.Text.Json.Serialization;
#pragma warning disable CS8618

namespace NgrokSharp.DTO;

public class TunnelErrorDTO
{
    [JsonPropertyName("error_code")] public long? ErrorCode { get; set; }

    [JsonPropertyName("status_code")] public long? StatusCode { get; set; }

    [JsonPropertyName("msg")] public string Msg { get; set; }

    [JsonPropertyName("details")] public Details Details { get; set; }
}

public class Details
{
    [JsonPropertyName("err")] public string Err { get; set; }
}