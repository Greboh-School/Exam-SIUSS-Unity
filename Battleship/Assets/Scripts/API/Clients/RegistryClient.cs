﻿using Assets.Scripts.API.Models.DTOs;
using Assets.Scripts.API.Models.Requests;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.API.Clients
{
    public class RegistryClient : BaseClient
    {
        public async Task<PlayerDTO> GetServer(PlayerConnectionRequest request)
        {
            //var response = await Client.PostAsJsonAsync($"{Client.BaseAddress}/PlayerRegistry", request);

#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR && UNITY_SERVER
            Client.BaseAddress = new Uri("http://api-services-registry:5277/api/v1");
#endif

            var response = await Client.PostAsJsonAsync($"http://localhost:7223/api/v1/Players", request);
        

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Status: {response.StatusCode} : Reason {response.RequestMessage}");
            }

            var dto = await response.Content.ReadFromJsonAsync<PlayerDTO>();

            if (dto is null)
            {
                Debug.LogError("dto is null");
            }

            return dto;
        }

        public async Task RemovePlayerFromRegistry(Guid id)
        {
            var response = await Client.DeleteAsync($"{Client.BaseAddress}/PlayerRegistry/{id}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError("Failed deleting Player from Registry!");
            }
        }
    }
}