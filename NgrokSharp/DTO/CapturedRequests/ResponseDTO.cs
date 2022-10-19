#pragma warning disable CS8618
#pragma warning disable CS1591
namespace NgrokSharp.DTO.CapturedRequests;

public class Response
{
    public string status { get; set; }
    public int status_code { get; set; }
    public string proto { get; set; }
    public Headers1 headers { get; set; }
    public string raw { get; set; }
}