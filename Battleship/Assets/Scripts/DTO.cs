using System;
using UnityEngine;

public class DTO : MonoBehaviour
{
    [Serializable]
    public class User
    {
        public string Username;
        public string Password;
    }

    [Serializable]
    public class IPResponse
    {
        public string IP;
        public string Port;
    }
}
