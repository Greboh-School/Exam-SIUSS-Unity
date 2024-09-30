using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkSession : MonoBehaviour {
    public string defaultServerIP = "127.0.0.1";
    public string defaultServerPort = "40000";
    public string loginAPI = "localhost:7000/api/User";
    public string registryAPI = "localhost:7002/v1/Gameserver/verify";
    public string jwt;
    public bool AutoStartNetworkSession = false;
    public enum NetworkType { Client, Host, Server }
    public NetworkType type;

    private string serverIP;
    private string serverPort;
    private void Start()
    {
        if (AutoStartNetworkSession)
        {
            StartSession();
        }
    }
    
    public void StartSession()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (type == NetworkType.Client) StartClient();
            if (type == NetworkType.Host) StartHost();
            if (type == NetworkType.Server) StartServer();
        }
    }

    private void SetIP()
    {
        serverIP = Environment.GetEnvironmentVariable("SERVER_IP") ?? defaultServerIP;
        serverPort = Environment.GetEnvironmentVariable("SERVER_PORT") ?? defaultServerPort;

        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = serverIP;
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = ushort.Parse(serverPort);
    }

    private void SetupCallback()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void StartServer()
    {
        SetIP();
        SetupCallback();
        bool succesfulStart = NetworkManager.Singleton.StartServer();

        DebugSessionState(succesfulStart);
    }

    public void StartHost()
    {
        SetIP();
        SetupCallback();
        bool succesfulStart = NetworkManager.Singleton.StartHost();

        DebugSessionState(succesfulStart);
    }

    public void StartClient()
    {
        bool succesfulStart = NetworkManager.Singleton.StartClient();

        DebugSessionState(succesfulStart);
    }

    private void DebugSessionState(bool succesfulStart)
    {
        if (!succesfulStart) Debug.LogError($"Failed to start network session! {serverIP}:{serverPort} as a {type}");
        else Debug.Log($"Started session {serverIP}:{serverPort} as a {type}");
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}, hello");
    }

    // Callback for when a client disconnects
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}, bye");
    }
}