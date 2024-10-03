using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Player
{
    public sealed class ProfileManager : MonoBehaviour
    {
        public PlayerProfile Profile;

        private JsonWebTokenHandler _tokenHandler = new();
        
        public void OnSuccessfulLogin(ApplicationUserDTO dto)
        {
            Profile = ReadSession(dto);
        }

        private PlayerProfile ReadSession(ApplicationUserDTO dto)
        {
            var content = _tokenHandler.ReadJsonWebToken(dto.AccessToken);
            var claims = content.Claims.ToList();

            var userId = claims.FirstOrDefault(x => x.Type == "uid")!.Value;
            var userName = claims.FirstOrDefault(x => x.Type == "sub")!.Value;

            Dictionary<RoleType, RoleLevel> roles = new();

            // Filter out all non-custom claims by checking for ":"
            foreach (var claim in claims.Where(x => x.Type.Contains(":")))
            {
                var type = claim.Type switch
                {
                    "systems:website:role" => "website",
                    "systems:game:role" => "game",
                    _ => string.Empty
                };
                
                roles.Add(Enum.Parse<RoleType>(type, true), Enum.Parse<RoleLevel>(claim.Value, true));
            }

            return new()
            {
                UserId = Guid.Parse(userId),
                Claims = claims,
                Username = userName,
                AccessToken = dto.AccessToken,
                Roles = roles
            };
        }
    }
}