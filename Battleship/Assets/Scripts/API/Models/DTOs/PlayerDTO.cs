using System;

namespace Assets.Scripts.API.Models.DTOs
{
    public class PlayerDTO
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Guid ServerId { get; set; }
        public string ServerAddress { get; set; }
        public ushort ServerPort { get; set; }
    }
}