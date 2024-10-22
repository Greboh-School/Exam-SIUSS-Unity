using System;

namespace Assets.Scripts.API.Models.DTOs
{
    public class ServerDTO
    {
        public Guid Id { get; set; }
        public string Address { get; set; }
        public ulong Port { get; set; }
        public ServerProperties Properties { get; set; }
    }

    public class ServerProperties
    {
        public int PlayerCount { get; set; }
        public int MaxPlayerCount { get; set; }
    }
}