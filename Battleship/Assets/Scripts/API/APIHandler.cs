using System.Threading.Tasks;
using Assets.Scripts.API.Clients;
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

    public async Task<PlayerDTO> GetServer(PlayerConnectionRequest request)
    {
        return await _registryClient.GetServer(request);
    }
}