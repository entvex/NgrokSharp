namespace NgrokSharp.DTO
{
    public class StartTunnelDTO
    {
        public string addr { get; set; }
        public string proto { get; set; }
        public string name { get; set; }
        public string bind_tls { get; set; }
        public string subdomain { get; set; }
        public string hostname { get; set; }
    }
}