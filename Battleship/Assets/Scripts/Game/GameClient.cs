using Player;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public enum GamePhase { Build, Ready, Wait, Shoot, Ended }

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

                SendInfoAndStartServerRpc(NetworkManager.Singleton.LocalClientId, profile.Username);

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
        private void ChangePhaseClientRpc(GamePhase phase, ClientRpcParams clientRpcParams = default)
        {
            Phase = phase;
        }

        [ClientRpc]
        private void BuildNextShipClientRpc(PlaceShipDTO dto, ClientRpcParams clientRpcParams = default)
        {
            SelfBoard.BuildNextShip(dto);
        }

        [ServerRpc]
        public void TryPlaceShipServerRpc(PlaceShipDTO dto, ServerRpcParams rpcParams = default)
        {
            var isValidPosition = SelfBoard.IsShipPositionValid(dto);

            if (isValidPosition)
            {
                SelfBoard.PlaceShip(dto);

                //Advance ship to next variant
                var type = (int)dto.Type;
                dto.Type = (ShipType)type + 1;

                var isNextPhase = HasBuildPhaseEnded();

                if (!isNextPhase)
                {
                    BuildNextShipClientRpc(dto, new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { Id }
                        }
                    });
                }
            }
        }

        private bool HasBuildPhaseEnded()
        {
            if (SelfBoard.Ships.Count is 5)
            {
                ChangePhase(GamePhase.Ready);

                Server.CheckForGameStart(this);

                return true;
            }

            return false;
        }

        [ClientRpc]
        public void SendOppenentNameClientRpc(string userName, ClientRpcParams clientRpcParams = default)
        {
            EnemyBoard.Text_UserName.text = userName;
        }

        [ServerRpc]
        private void SendInfoAndStartServerRpc(ulong clientId, string userName)
        {
            Id = clientId;
            UserName = userName;
            SelfBoard.GameClient = this;

            Server = FindObjectOfType<GameServer>();

            Server.RegisterPlayer(this);
        }

        public void ChangePhase(GamePhase phase)
        {
            Phase = phase;

            ChangePhaseClientRpc(Phase, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { Id }
                }
            });
        }

        [ServerRpc]
        public void SendShotToServerRpc(Vector2 gridPosition)
        {
            Server.AttackOtherClient(gridPosition, this);

            Destroy(EnemyBoard.MouseMarker);
            EnemyBoard.MouseMarker = null;
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

        public void InstantiateHitmarker(Vector2 gridPosition, bool isHit, bool isShooter)
        {
            var board = isShooter ? EnemyBoard : SelfBoard;

            board.InstantiateHitmarker(gridPosition, isHit);
        }

        [ClientRpc]
        public void GameOverClientRpc(string winningUser)
        {
            Phase = GamePhase.Ended;
            //TODO: Winning logic
        }
    }
}