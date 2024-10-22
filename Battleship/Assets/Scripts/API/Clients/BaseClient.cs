using System;
using System.Net.Http;
using Player;
using UnityEngine;

public class BaseClient : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private string _baseAddress = "http://localhost:8080/api/v1";

    [SerializeField, Range(10, 60)]
    private int _timeoutInSeconds = 10;
    
    protected HttpClient Client = default!;
    protected ProfileManager ProfileManager = default!;

    private void Awake()
    {
        Client = new()
        {
            BaseAddress = new(_baseAddress),
            Timeout = TimeSpan.FromSeconds(_timeoutInSeconds)
        };
    }
    
    protected string GetAccessToken()
    {
        ProfileManager ??= FindObjectOfType<ProfileManager>();
        return ProfileManager.Profile.AccessToken;
    }
}
