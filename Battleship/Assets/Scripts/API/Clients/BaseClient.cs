using System;
using System.Net.Http;
using UnityEngine;

public class BaseClient : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private string _baseAddress = "http://localhost:8080/api/v1";

    [SerializeField, Range(10, 60)]
    private int _timeoutInSeconds = 10;
    
    protected HttpClient Client = default!;

    private void Awake()
    {
        Client = new()
        {
            BaseAddress = new(_baseAddress),
            Timeout = TimeSpan.FromSeconds(_timeoutInSeconds)
        };
    }
}
