using System.Threading.Tasks;
using Clients;
using Player;
using Requests;
using UnityEngine;

public class APIHandler : MonoBehaviour
{
    private AuthenticationClient _authenticationClient;
    private ProfileManager _profileManager;
    
    private void Start()
    {
        _authenticationClient = FindObjectOfType<AuthenticationClient>();
        _profileManager = FindObjectOfType<ProfileManager>();
    }
    
    public async Task Login(LoginRequest request)
    {
        var dto = await _authenticationClient.Login(request);

        if (dto is null)
        {
            Debug.LogError("DTO is null!");
            return;
        }
        
        _profileManager.OnSuccessfulLogin(dto);
    }

    // TODO: Implement Register
    // public async Task Register(RegistrationRequest request)
    // {
    //     await _authenticationClient.Register(request);
    // }
    
}