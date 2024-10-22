namespace API.Models.Requests
{
    public class ServerRegistrationRequest
    {
        public string Address { get; set; }
        public ulong Port { get; set; }
        public string ListenAddress { get; set; }
    }
}