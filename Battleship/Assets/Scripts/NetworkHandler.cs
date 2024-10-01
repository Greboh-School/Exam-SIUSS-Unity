using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkHandler : MonoBehaviour
{
    [Header("Server Settings")]
    [Tooltip("The IP which will be connected to / hosted on")]
    public string serverIP = "127.0.0.1:40000";
    [Tooltip("Should a Client/Server start instantly - Mainly meant for server")]
    public bool AutoStartNetworkSession = false;
    public NetworkType sessionType;

    [Header("API Endpoints")]
    [Tooltip("Can also be configured with Environment variables: LOGIN_API and REGISTRY_API")]
    public string loginAPI;
    [Tooltip("Can also be configured with Environment variables: LOGIN_API and REGISTRY_API")]
    public string registryAPI;

    [Tooltip("JWT for authorization of user, will be auto filled by scripts")]
    public string jwt;

    public enum NetworkType { Client, Server }

    private void Start()
    {
        SetAPIEndpoints();

        if (AutoStartNetworkSession)
        {
            StartSession();
        }
    }

    private void SetAPIEndpoints()
    {
        loginAPI = Environment.GetEnvironmentVariable("LOGIN_API") ?? "localhost:8080/api/v1";
        registryAPI = Environment.GetEnvironmentVariable("REGISTRY_API") ?? "localhost:7002/v1/Gameserver/verify";
    }

    public void StartSession()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Network session is already running.");
            return;
        }

        bool successfulStart = false;
        SetIP();

        switch (sessionType)
        {
            case NetworkType.Client:
                successfulStart = NetworkManager.Singleton.StartClient();
                break;

            case NetworkType.Server:
                RegisterServerCallbacks();
                successfulStart = NetworkManager.Singleton.StartServer();
                break;
        }

        DebugSessionState(successfulStart);
    }

    private void SetIP()
    {
        serverIP = Environment.GetEnvironmentVariable("UNITY_SERVER_IP") ?? serverIP;
        string[] splitIP = serverIP.Split(':');

        if (splitIP.Length != 2)
        {
            Debug.LogError("Invalid server IP format. Expected format is 'IP:Port'.");
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = splitIP[0];
        transport.ConnectionData.Port = ushort.Parse(splitIP[1]);
    }

    private void RegisterServerCallbacks()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void DebugSessionState(bool successfulStart)
    {
        if (successfulStart)
        {
            Debug.Log($"Network session started at {serverIP} as {sessionType}");
        }
        else
        {
            Debug.LogError($"Failed to start network session at {serverIP} as {sessionType}");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");
    }
}