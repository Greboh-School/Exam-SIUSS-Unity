using Assets.Scripts.API.Models.DTOs;
using Assets.Scripts.API.Models.Requests;
using System.Net.Http.Json;
using System.Threading.Tasks;
<<<<<<< Updated upstream
=======
using API.Models.Requests;
using Assets.Scripts.UI.Views.SubViews;
using Game;
using Network.API.Models;
>>>>>>> Stashed changes
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

        public async Task CreateServerQueues(Guid id)
        {
            var response = await Client.PostAsync($"{Client.BaseAddress}/brokers/queue/{id}", null);
            
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Failed to create server queue! {response.ReasonPhrase}");
            }
            else
            {
                Debug.Log($"Created server queues for server: {id}");
            }
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
    }
}