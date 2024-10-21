using System;

namespace Assets.Scripts.API.Models.Requests
{
    public class PlayerConnectionRequest
    {
        public string UserName { get; set; }
        public Guid UserId { get; set; }
    }
}