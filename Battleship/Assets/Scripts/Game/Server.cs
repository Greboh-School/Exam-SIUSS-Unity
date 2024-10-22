using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using API.Models.Requests;
using Assets.Scripts.API.Models.Requests;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Game
{
    public class Server : NetworkBehaviour
    {
        [Header("Boards")]
        public Client ClientA;

        public Client ClientB;

        public Guid Id;

        private string Ip = "127.0.0.1:6969";
        private APIHandler _api;
        private PrefabManager _prefabManager;

        private Dictionary<ulong, Guid> _players = new();

        private void Awake()
        {
            _api = FindObjectOfType<APIHandler>();
            _prefabManager = FindObjectOfType<PrefabManager>();
        }

        private void Start()
        {
            TryGetEnvVars();
            NetworkManager.Singleton.StartServer();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Debug.Log("Network Server spawned");

            NetworkManager.Singleton.OnConnectionEvent += async (manager, data) =>
            {
                if (data.EventType is ConnectionEvent.ClientDisconnected)
                {
                    if (manager.IsServer)
                    {
                        await RemoveClient(data.ClientId);
                    }
                }
                else if (data.EventType is ConnectionEvent.ClientConnected)
                {
                    Debug.Log($"Player spawning {data.ClientId}");
                }
            };

            NetworkManager.Singleton.OnServerStarted += async () => await InitializeServer();
            NetworkManager.Singleton.OnServerStopped += async obj => await StopServer(obj);
        }

        private async Task InitializeServer()
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            var request = new ServerRegistrationRequest
            {
                Address = transport.ConnectionData.Address,
                Port = transport.ConnectionData.Port,
                ListenAddress = transport.ConnectionData.ServerListenAddress
            };

            var dto = await _api.RegisterServer(request);

            if (dto is null)
            {
                Application.Quit();
            }

            Id = dto!.Id;

            Debug.Log($"Server registered at {Ip} with id {Id}");
        }

        private async Task StopServer(bool obj)
        {
            await _api.RemoveServer(Id);
        }

        private void TryGetEnvVars()
        {
            Ip = Environment.GetEnvironmentVariable("UNITY_SERVER_IP") ?? Ip;
            string[] splitIP = Ip.Split(':');

            if (splitIP.Length != 2)
            {
                Debug.LogError("Invalid server IP format. Expected format is 'IP:Port'.");
                return;
            }

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.Address = splitIP[0];
            transport.ConnectionData.Port = ushort.Parse(splitIP[1]);
            transport.ConnectionData.ServerListenAddress = "0.0.0.0";
        }

        private async Task RemoveClient(ulong id)
        {
            var userId = _players[id];
            await _api.RemovePlayerFromRegistry(userId);
            _players.Remove(id);
            
            OnClientDisconnect(id);
        }

        /// <summary>
        /// Once both players have finished building phase the shooting phase should start.
        /// </summary>
        /// <param name="client"></param>
        public void CheckForGameStart(Client client)
        {
            if (ClientA.Phase is GamePhase.Ready && ClientB.Phase is GamePhase.Ready)
            {
                //Both clients ready to fire! Let ClientA take first turn!
                ClientA.ChangePhase(GamePhase.Shoot);
            }
        }

        /// <summary>
        /// Destroy Objects and references of disconnected client.
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns>UserId Guid of the disconnected client</returns>
        public Guid OnClientDisconnect(ulong clientId)
        {
            Client remainingClient = null;
            Guid disconnectedClientUserId = Guid.Empty;

            if (ClientA?.Id == clientId)
            {
                disconnectedClientUserId = ClientA.UserId;
                Destroy(ClientA.gameObject);
                ClientA = null;

                remainingClient = ClientB;
            }

            if (ClientB?.Id == clientId)
            {
                disconnectedClientUserId = ClientB.UserId;
                Destroy(ClientB.gameObject);
                ClientB = null;

                remainingClient = ClientA;
            }

            if (remainingClient is null)
            {
                Debug.Log("Both clients have disconneted");
                return Guid.Empty;
            }

            remainingClient.DisconnectClientRpc();

            if (remainingClient.Phase != GamePhase.Build || remainingClient.Phase != GamePhase.Ended)
            {
                remainingClient.ChangePhase(GamePhase.Ended);
            }

            return disconnectedClientUserId;
        }

        /// <summary>
        /// Register presence of a client, will share names once both are present.
        /// </summary>
        /// <param name="client"></param>
        public void RegisterPlayer(Client client)
        {
            _players.Add(client.Id, client.UserId);

            Debug.Log($"Player {client.Id}:{client.UserId} has joined the server");
            
            if (ClientA is null)
            {
                ClientA = client;
            }
            else if (ClientB is null)
            {
                ClientB = client;
            }

            if (ClientA is not null & ClientB is not null)
            {
                ClientA.SendOppenentNameClientRpc(ClientB.UserName, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { ClientA.Id }
                    }
                });

                ClientB.SendOppenentNameClientRpc(ClientA.UserName, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { ClientB.Id }
                    }
                });
            }
        }

        /// <summary>
        /// Send attack from one client to another
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <param name="attackingClient"></param>
        public void SendAttack(Vector2 gridPosition, Client attackingClient)
        {
            var targetClient = GetOtherClient(attackingClient.Id);

            var isHit = targetClient.AnyShipsHit(gridPosition);

            attackingClient.EnemyBoard.InstantiateHitmarker(gridPosition, isHit);
            targetClient.SelfBoard.InstantiateHitmarker(gridPosition, isHit);

            attackingClient.ShotResponseClientRpc(gridPosition, isHit);
            targetClient.SendShotToClientRpc(gridPosition, isHit);

            attackingClient.ChangePhase(GamePhase.Wait);
            targetClient.ChangePhase(GamePhase.Shoot);

            CheckForGameOver();
        }

        /// <summary>
        /// Checks if any clients hp is 0, then ends game if true
        /// </summary>
        private void CheckForGameOver()
        {
            string winningClientName = string.Empty;

            if (ClientA.Health is 0)
            {
                winningClientName = ClientB.UserName;
            }
            else if (ClientB.Health is 0)
            {
                winningClientName = ClientA.UserName;
            }

            if (winningClientName != string.Empty)
            {
                ClientA.GameOverClientRpc(winningClientName);
                ClientB.GameOverClientRpc(winningClientName);

                ClientA.Phase = GamePhase.Ended;
                ClientB.Phase = GamePhase.Ended;
            }
        }

        /// <summary>
        /// Simple method to get the other client
        /// </summary>
        /// <param name="knownClient"></param>
        /// <returns></returns>
        private Client GetOtherClient(ulong knownClient)
        {
            if (knownClient == ClientA.Id)
            {
                return ClientB;
            }

            return ClientA;
        }
    }
}