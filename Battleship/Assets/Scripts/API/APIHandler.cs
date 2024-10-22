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
    private ProfileManager _profileManager;
    private RegistryClient _registryClient;

    private void Start()
    {
        _authenticationClient = FindObjectOfType<AuthenticationClient>();
        _profileManager = FindObjectOfType<ProfileManager>();
        _registryClient = FindObjectOfType<RegistryClient>();
    }

    public async Task<PlayerProfile> Login(LoginRequest request)
    {
        var dto = await _authenticationClient.Login(request);

        if (dto is not null)
        {
            return _profileManager.OnSuccessfulLogin(dto);
        }
        
        Debug.LogError("DTO is null!");
        return null;
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
    
}