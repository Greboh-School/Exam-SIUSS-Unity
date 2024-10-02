using System.Collections;
using UnityEngine;

namespace Assets.Scripts.API.Models.Requests
{
    public class RegistrationRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CreatedBy { get; set; }
    }
}