using Assets.Scripts.API.Clients;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models.Requests;
using Assets.Scripts.API.Models.Requests;
using JetBrains.Annotations;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Network
{
    public enum NetworkType { Client, Server }

    public class NetworkHandler : MonoBehaviour
    {
        [Header("Server Settings")]
        [Tooltip("The IP which will be connected to / hosted on, in format IP:PORT")]
        public string serverIP = "127.0.0.1";
        public ushort serverPort = 4242;
        [Tooltip("Should a Client/Server start instantly - Mainly meant for server")]
        public bool AutoStartNetworkSession = false;
        public NetworkType sessionType;

        private PrefabManager _prefabManager;
        private Server _server;
        private APIHandler _api;
        
        private void Awake()
        {
            _api = FindObjectOfType<APIHandler>();
        }

        private void Start()
        {
            var autoStart = AutoStartNetworkSession;
#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR && UNITY_SERVER
            autoStart = true;
            sessionType = NetworkType.Server;
#endif
            if (autoStart)
            {
                StartSession();
            }
        }

        public void StartSession([CanBeNull] string ip = null, ushort port = 0)
        {
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Network session is already running.");
                return;
            }
            
            _prefabManager = FindObjectOfType<PrefabManager>();

            switch (sessionType)
            {
                case NetworkType.Client:
                    Debug.Log($"Starting client session on {ip}:{port}");
                    var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                    transport.ConnectionData.Address = ip;
                    transport.ConnectionData.Port = port;
                    transport.ConnectionData.ServerListenAddress = "0.0.0.0";
                    NetworkManager.Singleton.StartClient();

                    break;
                case NetworkType.Server:
                    Application.targetFrameRate = 30;
                    Instantiate(_prefabManager.ServerPrefab).GetComponent<Server>();
                    
                    break;
            }
        }
    }
}