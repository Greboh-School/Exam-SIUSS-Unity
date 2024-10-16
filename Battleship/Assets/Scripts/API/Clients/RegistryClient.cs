using Assets.Scripts.API.Models.DTOs;
using Assets.Scripts.API.Models.Requests;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.API.Clients
{
    public class RegistryClient : BaseClient
    {
        public async Task<ServerDTO> GetServer(GetServerRequest getServerRequest)
        {
            var response = await Client.PostAsJsonAsync($"{Client.BaseAddress}/registry", getServerRequest);

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Status: {response.StatusCode} : Reason {response.RequestMessage}");
            }

            var dto = await response.Content.ReadFromJsonAsync<ServerDTO>();

            if (dto is null)
            {
                Debug.LogError("dto is null");
            }

            return dto;
        }
    }
}