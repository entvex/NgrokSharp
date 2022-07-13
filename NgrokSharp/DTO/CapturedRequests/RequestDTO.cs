#pragma warning disable CS1591
using System;

namespace NgrokSharp.DTO;

public class RequestDTO
{
    public class Request
    {
        public string uri { get; set; }
        public string id { get; set; }
        public string tunnel_name { get; set; }
        public string remote_addr { get; set; }
        public DateTime start { get; set; }
        public int duration { get; set; }
        public Request1 request { get; set; }
        public Response response { get; set; }
    }
}