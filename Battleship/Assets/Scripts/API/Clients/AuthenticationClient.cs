using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Assets.Scripts.API.Models.Requests;
using Requests;
using UnityEngine;

namespace Clients
{
    public class AuthenticationClient : BaseClient
    {
        public async Task<ApplicationUserDTO> Login(LoginRequest loginRequest)
        {
            var response = await Client.PostAsJsonAsync($"{Client.BaseAddress}/sessions", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Status: {response.StatusCode} : Reason {response.RequestMessage}");
            }

            var dto = await response.Content.ReadFromJsonAsync<ApplicationUserDTO>();

            if(dto is null)
            {
                Debug.LogError("dto is null");
            }

            return dto;
        }

        public async Task<bool> Register(RegistrationRequest request)
        {
            var response = await Client.PostAsJsonAsync($"{Client.BaseAddress}/identities", request);

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Status: {response.StatusCode} : Reason {response.RequestMessage}");
                return false;
            }

            return true;
        }
    }
}