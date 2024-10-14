using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public enum GamePhase { Build, Wait, Shoot, Ended }

    public class GameServer : NetworkBehaviour
    {
        [Header("Clients")]
        [SerializeField]
        private ServerBoard _clientA;
        [SerializeField]
        private ServerBoard _clientB;
        [SerializeField]
        private GameClientRpc _gameClient;

        public void Start()
        {
            if(_gameClient is null)
            {
                _gameClient = FindObjectOfType<GameClientRpc>();

                if (_gameClient is null)
                {
                    Debug.LogError("GameServer's '_gameClient' reference is null!");
                }
            }

            Debug.Log("GameServer script started!");
        }

        [ServerRpc(RequireOwnership = false)]
        public void RegisterClientServerRpc(string userName, ulong clientId, ServerRpcParams rpcParams = default)
        {
            if (_clientA is null)
            {
                _clientA = new ServerBoard() { UserName = userName, Id = clientId };
            }
            else
            {
                _clientB = new ServerBoard() { UserName = userName, Id = clientId };

            }

            if (_clientA is not null && _clientB is not null)
            {
                //Both clients connected.
                ShareUsernamesToClients();
            }
        }

        private void ShareUsernamesToClients()
        {
            //Send ClientA name to ClientB
            _gameClient.SetEnemyNameClientRpc(_clientA.UserName, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { _clientB.Id }
                }
            });

            //Send ClientB name to ClientA
            _gameClient.SetEnemyNameClientRpc(_clientB.UserName, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { _clientA.Id }
                }
            });
        }

        [ServerRpc(RequireOwnership = false)]
        public void TryPlaceShipServerRpc(PlaceShipDTO dto, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            var client = GetClientFromId(clientId);

            var valid = client.IsShipPositionValid(dto);

            if (valid)
            {
                client.AddShipToServer(dto);

                _gameClient.PlaceShipClientRpc(dto, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                });

                CheckIfBuildPhasePersists(client);
            }
        }

        private void CheckIfBuildPhasePersists(ServerBoard client)
        {
            if (client.GetShipCount() is 5)
            {
                client.Phase = GamePhase.Wait;

                _gameClient.ChangePhaseClientRpc(GamePhase.Wait, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { client.Id }
                    }
                });
            }

            if (_clientA.Phase is GamePhase.Wait && _clientB.Phase is GamePhase.Wait)
            {
                //Both clients finished building and entered waiting: allow ClientA to shoot.
                client.Phase = GamePhase.Shoot;

                _gameClient.ChangePhaseClientRpc(GamePhase.Shoot, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { client.Id }
                    }
                });
            }
        }

        private ServerBoard GetClientFromId(ulong clientId, bool getOpposite = false)
        {
            if (_clientA.Id == clientId)
            {
                return getOpposite ? _clientB : _clientA;
            }
            else
            {
                return getOpposite ? _clientA : _clientB;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendShotToServerRpc(Vector2 gridPosition, ServerRpcParams rpcParams = default)
        {
            ulong attackingClient = rpcParams.Receive.SenderClientId;
            var targetClient = GetClientFromId(attackingClient, true);

            var anyHits = targetClient.AnyShipsHit(gridPosition);

            _gameClient.ShotResponseClientRpc(gridPosition, anyHits, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { attackingClient }
                }
            });

            _gameClient.AttackClientRpc(gridPosition, anyHits, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { targetClient.Id }
                }
            });

            FindPotentialWinner();
        }

        private void FindPotentialWinner()
        {
            if (_clientA.Health is 0)
            {
                _gameClient.GameOverClientRpc(_clientB.name);
                _clientA.Phase = GamePhase.Ended;
                _clientB.Phase = GamePhase.Ended;
            }
            else if (_clientB.Health is 0)
            {
                _gameClient.GameOverClientRpc(_clientA.name);
                _clientA.Phase = GamePhase.Ended;
                _clientB.Phase = GamePhase.Ended;
            }
        }
    }
}