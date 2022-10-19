#pragma warning disable CS8618
#pragma warning disable CS1591
namespace NgrokSharp.DTO.CapturedRequests;

public class Request1
{
    public string method { get; set; }
    public string proto { get; set; }
    public Headers headers { get; set; }
    public string uri { get; set; }
    public string raw { get; set; }
}