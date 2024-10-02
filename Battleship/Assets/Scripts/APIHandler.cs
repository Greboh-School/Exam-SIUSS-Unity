using Requests;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UnityEngine;

public class APIHandler : BaseClient
{
    private NetworkHandler networkHandler;

    private void Start()
    {
        networkHandler = GetComponent<NetworkHandler>();
    }

    public async Task<ApplicationUserDTO> Login(LoginRequest loginRequest)
    {
        Debug.Log(networkHandler.loginAPI);
        Debug.Log($"{loginRequest.UserName}, {loginRequest.Password}");

        //{networkHandler.loginAPI}/api/v1/sessions

        var response = await HttpClient.PostAsJsonAsync($"http://localhost:5053/api/v1/Sessions", loginRequest);

        if (!response.IsSuccessStatusCode)
        {
            Debug.LogError(response.StatusCode);
        }

        var dto = await response.Content.ReadFromJsonAsync<ApplicationUserDTO>();

        if(dto is null)
        {
            Debug.LogError("dto is null");
        }

        return dto;
    }
}