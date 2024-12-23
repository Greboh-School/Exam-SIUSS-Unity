﻿﻿using Assets.Scripts.API.Models.DTOs;
using Assets.Scripts.API.Models.Requests;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using API.Models.Requests;
using Assets.Scripts.UI.Views.SubViews;
using Network.API.Models;
using UnityEngine;

namespace Assets.Scripts.API.Clients
{
    public class RegistryClient : BaseClient
    {
        public async Task<PlayerDTO> RegisterClient(PlayerConnectionRequest request)
        {
            Client.DefaultRequestHeaders.Authorization = new("Bearer", GetAccessToken());
            var response = await Client.PostAsJsonAsync($"{Client.BaseAddress}/players", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var model = await response.Content.ReadFromJsonAsync<ErrorModel>();
                
                Debug.LogError($"Status: {response.StatusCode} : Reason {model.Message}");

                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<PlayerDTO>();
            
            return dto;
        }

        public async Task RemovePlayerFromRegistry(Guid id)
        {
            var response = await Client.DeleteAsync($"{Client.BaseAddress}/players/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var model = await response.Content.ReadFromJsonAsync<ErrorModel>();
                
                Debug.LogError($"Failed to remove player from registry! Status: {response.StatusCode} : Reason {model.Message}");
            }
        }

        public async Task<ServerDTO> RegisterServer(ServerRegistrationRequest request)
        {
#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR && UNITY_SERVER
            Client.BaseAddress = new Uri("http://api-services-registry:5128/api/v1");
#endif
            Console.WriteLine($"Client Base Address: {Client.BaseAddress}");
            var response = await Client.PostAsJsonAsync($"{Client.BaseAddress}/servers", request);

            if (!response.IsSuccessStatusCode)
            {
                var model = await response.Content.ReadFromJsonAsync<ErrorModel>();
                
                Debug.LogError($"Status: {response.StatusCode} : Reason {model.Message}");

                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<ServerDTO>();

            return dto;
        }

        public async Task RemoveServer(Guid id)
        {
            await Client.DeleteAsync($"{Client.BaseAddress}/servers/{id}");
        }
        
        public async Task SendMessage(MessageDTO dto)
        {
            var response = await Client.PostAsJsonAsync($"{Client.BaseAddress}/brokers/message", dto);

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Failed to send public message! {response.StatusCode} ");

                if (dto.Type is MessageType.Private)
                {
                    FindFirstObjectByType<GameHUD>().SetErrorMessage($"Failed to send private message, {dto.Recipient} is not online!");
                }
            }
            else Debug.Log($"Sent {dto.Type} message");
        }
        
        public async Task CreateServerQueues(Guid serverId)
        {
            var response = await Client.PostAsync($"{Client.BaseAddress}/brokers/queue/{serverId}", null);

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Failed to create server queue! {response.ReasonPhrase}");
            }
            else
            {
                Debug.Log($"Created server queue for server: {serverId}");
            }
        }
    }
}