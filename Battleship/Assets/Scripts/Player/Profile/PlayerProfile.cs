using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using UnityEngine;

namespace Player
{
    public enum RoleType : int
    {
        Unknown = 0,
        Game = 1,
        Website = 2
    }

    public enum RoleLevel : int
    {
        Unknown = 0,
        ReadOnly = 1,
        User = 2,
        Admin = 3
    }

    [Serializable]
    public class PlayerProfile
    {
        public Guid UserId { get; set; }
        public List<Claim> Claims { get; set; }

        [field: Header("Profile")]
        [field: SerializeField]
        public string Username { get; set; }
        [field: SerializeField]
        public string AccessToken { get; set; }
        
        public Dictionary<RoleType, RoleLevel> Roles { get; set; }

        public bool HasRole(RoleType type)
        {
            return Roles.ContainsKey(type);
        }

        public RoleLevel? TryGetRoleLevel(RoleType type)
        {
            var exists = Roles.TryGetValue(type, out var level);

            if (exists)
            {
                return level;
            }

            return null;
        }

        public bool IsAdmin()
        {
            Roles.TryGetValue(RoleType.Game, out var level);

            return level is RoleLevel.Admin;
        }
        
        public void ResetProfile()
        {
            UserId = Guid.Empty;
            Claims = new();
            AccessToken = string.Empty;
            Username = string.Empty;
            Roles = new();
        }
    }
}