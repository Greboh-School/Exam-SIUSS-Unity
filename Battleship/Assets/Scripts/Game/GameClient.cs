using Player;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class GameClient : NetworkBehaviour
    {
        [Header("Controlling Server")]
        public GameServer Server;

        [Header("Boards")]
        public GameBoard SelfBoard;
        public GameBoard EnemyBoard;

        [Header("Client Info")]
        public ulong Id;
        public int Health;
        public GamePhase Phase;
        public string UserName;

        public void Start()
        {
            if (!IsOwner && IsClient) Destroy(this.gameObject);

            EnemyBoard.GameClient = this;
            SelfBoard.GameClient = this;

            if (IsOwner && IsClient)
            {
                var profile = FindObjectOfType<ProfileManager>().Profile;

                RecieveClientInfoAndStartServerRpc(NetworkManager.Singleton.LocalClientId, profile.Username);

                SelfBoard.Text_UserName.text = UserName;
                Id = NetworkManager.Singleton.LocalClientId;

                SelfBoard.InstantiateShipForPlacing = true;
            }
        }

        public void Update()
        {
            if (!IsClient || !IsOwner)
            {
                return;
            }

            switch (Phase)
            {
                case GamePhase.Build:
                    SelfBoard.BuildPhase();
                    break;
                case GamePhase.Wait:
                    break;
                case GamePhase.Shoot:
                    EnemyBoard.ShootingPhase();
                    break;
                case GamePhase.Ended:
                    break;
                default:
                    break;
            }
        }

        [ClientRpc]
        public void ChangePhaseClientRpc(GamePhase phase, ClientRpcParams clientRpcParams = default)
        {
            Phase = phase;
        }

        [ClientRpc]
        public void PlaceShipClientRpc(PlaceShipDTO dto, ClientRpcParams clientRpcParams = default)
        {
            SelfBoard.ClientPlaceShip(dto);
        }

        [ServerRpc]
        public void TryPlaceShipServerRpc(PlaceShipDTO dto, ServerRpcParams rpcParams = default)
        {
            var isValidPosition = SelfBoard.IsShipPositionValid(dto);

            if (isValidPosition)
            {
                SelfBoard.ServerPlaceShip(dto);

                //Advance ship to next variant
                var type = (int)dto.Type;
                dto.Type = (ShipType)type + 1;

                var allPlaced = AreAllShipsArePlaced();

                if (!allPlaced)
                {
                    PlaceShipClientRpc(dto, new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { Id }
                        }
                    });
                }
            }
        }

        private bool AreAllShipsArePlaced()
        {
            if (SelfBoard.Ships.Count is 5)
            {
                ChangePhaseClientRpc(GamePhase.Wait, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { Id }
                    }
                });

                Phase = GamePhase.Wait;

                Server.RegisterPlayerAsReady(this);

                return true;
            }

            return false;
        }

        [ServerRpc]
        private void RecieveClientInfoAndStartServerRpc(ulong clientId, string userName)
        {
            Id = clientId;
            UserName = userName;
            SelfBoard.GameClient = this;

            Server = FindObjectOfType<GameServer>();

            Debug.Log($"Client registered: {userName}, {clientId}");
        }

        public void ChangePhase(GamePhase phase)
        {
            Phase = phase;
            ChangePhaseClientRpc(phase);
        }

        [ServerRpc]
        public void SendShotToServerRpc(Vector2 gridPosition)
        {
            Server.AttackOtherClient(gridPosition, this);
        }

        public bool AnyShipsHit(Vector2 gridPosition)
        {
            foreach (var shipObject in SelfBoard.Ships)
            {
                var ship = shipObject.GetComponent<Ship>();

                if (ship.DoesShotHit(gridPosition))
                {
                    Health--;
                    return true;
                }
            }

            return false;
        }

        [ClientRpc]
        public void ShotResponseClientRpc(Vector2 gridPosition, bool isHit, ClientRpcParams clientRpcParams = default)
        {
            EnemyBoard.InstantiateMarker(gridPosition, isHit);

            Phase = GamePhase.Wait;
        }

        [ClientRpc]
        public void SendAttackClientRpc(Vector2 gridPosition, bool isHit, ClientRpcParams clientRpcParams = default)
        {
            SelfBoard.InstantiateMarker(gridPosition, isHit);

            Phase = GamePhase.Shoot;
        }

        [ClientRpc]
        public void GameOverClientRpc(string winningUser)
        {
            Phase = GamePhase.Ended;
            //TODO: Winning logic
        }
    }
}