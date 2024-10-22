<<<<<<< Updated upstream
﻿using Unity.Netcode;
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API.Models.Requests;
using Assets.Scripts.API.Models.DTOs;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
>>>>>>> Stashed changes
using UnityEngine;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UnityEditor;
using MessageType = Assets.Scripts.API.Models.DTOs.MessageType;

namespace Game
{
    public class Server : MonoBehaviour
    {
        [Header("Boards")]
        public Client ClientA;
        public Client ClientB;

<<<<<<< Updated upstream
=======
        public Guid Id;

        private string Ip = "127.0.0.1:6969";
        private APIHandler _api;
        private PrefabManager _prefabManager;

        private Dictionary<ulong, Client> _players = new();

        private IConnection _connection;
        private IModel _channel;

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

            await _api.CreateServerQueues(Id);

            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var hostName = "localhost";
            
#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR && UNITY_SERVER
            hostName = "rabbitmq";
#endif
            
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = "Dev",
                Password = "Test-1234"
            };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceived;
            _channel.BasicConsume(queue: $"server.{Id}.public", autoAck: true, consumer: consumer);
            _channel.BasicConsume(queue: $"server.{Id}", autoAck: true, consumer: consumer);
            Debug.Log("RabbitMQ initialized");
            
        }

        private void OnMessageReceived(object obj, BasicDeliverEventArgs e)
        {
            var body = System.Text.Encoding.UTF8.GetString(e.Body.ToArray());
            var message = JsonSerializer.Deserialize<MessageDTO>(body);
            
            Debug.LogError($"Got message! {message.Content}");
            
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                if (message.Type == MessageType.Public)
                {
                    NotifyPublicMessage(message);
                }
                else if (message.Type == MessageType.Private) 
                {
                    NotifyPrivateMessage(message);
                }
            });
        }
        
        private void NotifyPublicMessage(MessageDTO dto)
        {
            foreach (var player in _players)
            {
                player.Value.SetHudMessageClientRPC(Enum.GetName(typeof(MessageType), dto.Type), dto.Content, dto.Sender);
            }
        }
        
        private void NotifyPrivateMessage(MessageDTO dto)
        {
            var player = _players.FirstOrDefault(x => x.Value.UserName == dto.Recipient).Value;
            player.SetHudMessageClientRPC(Enum.GetName(typeof(MessageType), dto.Type), dto.Content, dto.Sender);
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
            var client = _players[id];
            await _api.RemovePlayerFromRegistry(client.UserId);
            _players.Remove(id);
            
            OnClientDisconnect(id);
        }

>>>>>>> Stashed changes
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
        public void OnClientDisconnect(ulong clientId)
        {
            Client remainingClient = null;

            if (ClientA.Id == clientId)
            {
                Destroy(ClientA.gameObject);
                ClientA = null;

                remainingClient = ClientB;
            }
            if (ClientB.Id == clientId)
            {
                Destroy(ClientB.gameObject);
                ClientB = null;

                remainingClient = ClientA;
            }

            if (remainingClient is null)
            {
                Debug.Log("Both clients have disconneted");
                return;
            }

            remainingClient.DisconnectClientRpc();

            if (remainingClient.Phase != GamePhase.Build || remainingClient.Phase != GamePhase.Ended)
            {
                remainingClient.ChangePhase(GamePhase.Ready);
            }
        }

        /// <summary>
        /// Register presence of a client, will share names once both are present.
        /// </summary>
        /// <param name="client"></param>
        public void RegisterPlayer(Client client)
        {
<<<<<<< Updated upstream
=======
            _players.Add(client.Id, client);

            Debug.Log($"Player {client.Id}:{client.UserId} has joined the server");
            
>>>>>>> Stashed changes
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

            if(winningClientName != string.Empty)
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