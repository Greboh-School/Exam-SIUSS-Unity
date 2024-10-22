using System;
using System.Threading.Tasks;
using API.Models.Requests;
using Assets.Scripts.API.Clients;
using Assets.Scripts.API.Models.DTOs;
using Assets.Scripts.API.Models.Requests;
using Clients;
using Player;
using Requests;
using UnityEngine;

public class APIHandler : MonoBehaviour
{
    private AuthenticationClient _authenticationClient;
    private RegistryClient _registryClient;

    private ProfileManager _profileManager;
    
    private void Start()
    {
        _authenticationClient = FindObjectOfType<AuthenticationClient>();
        _registryClient = FindObjectOfType<RegistryClient>();
        _profileManager = FindObjectOfType<ProfileManager>();
    }
    
    public async Task<PlayerProfile> Login(LoginRequest request)
    {
        var dto = await _authenticationClient.Login(request);

        if (dto is null)
        {
            Debug.LogError("DTO is null!");
            return null;
        }
        
        return _profileManager.OnSuccessfulLogin(dto);
    }

    public async Task<bool> Register(RegistrationRequest request)
    {
        return await _authenticationClient.Register(request);
    }


    public async Task<PlayerDTO> RegisterClient(PlayerConnectionRequest request)
    {
        return await _registryClient.RegisterClient(request);
    }
    
    public async Task RemovePlayerFromRegistry(Guid id)
    {
        await _registryClient.RemovePlayerFromRegistry(id);
    }

    public async Task<ServerDTO> RegisterServer(ServerRegistrationRequest request)
    {
        var dto = await _registryClient.RegisterServer(request);

        return dto;
    }
    
    public async Task RemoveServer(Guid id)
    {
        await _registryClient.RemoveServer(id);
    }

    public async Task CreateServerQueues(Guid id)
    {
        await _registryClient.CreateServerQueues(id);
    }
    
    public async Task SendMessage(MessageDTO dto)
    {
        await _registryClient.SendMessage(dto);
    }
}