#pragma warning disable CS1591
namespace NgrokSharp.DTO;

public class CapturedRequestRootDTO
{
    public string uri { get; set; }
    public RequestDTO.Request[] requests { get; set; }
}