using System;
using System.Net.Http;
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

    public enum NetworkType { Client, Server }

    private void Start()
    {
        loginAPI = Environment.GetEnvironmentVariable("LOGIN_API") ?? "http://localhost:8080/api/v1/sessions";
        registryAPI = Environment.GetEnvironmentVariable("REGISTRY_API") ?? "http://localhost:7002/v1/Gameserver/verify";

        if (AutoStartNetworkSession)
        {
            StartSession();
        }
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
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

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