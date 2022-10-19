#pragma warning disable CS8618
#pragma warning disable CS1591
namespace NgrokSharp.DTO.CapturedRequests;

public class CapturedRequestRootDTO
{
    public string uri { get; set; }
    public RequestDTO.Request[] requests { get; set; }
}