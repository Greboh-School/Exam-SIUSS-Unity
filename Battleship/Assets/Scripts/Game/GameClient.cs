using Player;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Game
{
    public class GameClient : NetworkBehaviour
    {
        [SerializeField]
        public GameBoard EnemyBoard;

        public int Health;

        [Header("Client Info")]
        public ulong Id;

        public GamePhase Phase;

        [Header("Boards")]
        [SerializeField]
        public GameBoard SelfBoard;

        public string UserName;

        public void Start()
        {
            if (!IsOwner && IsClient) Destroy(this.gameObject);

            if (IsOwner && IsClient)
            {
                var profile = FindObjectOfType<ProfileManager>().Profile;

                RecieveClientInfoAndStartServerRpc(NetworkManager.Singleton.LocalClientId, profile.Username);

                SelfBoard.Text_UserName.text = UserName;
                Id = NetworkManager.Singleton.LocalClientId;
                SelfBoard.GameClient = this;
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
                    SelfBoard.ShipBuilder();
                    break;

                case GamePhase.Wait:
                    break;

                case GamePhase.Shoot:
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
                SelfBoard.AddShipToServer(dto);

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
        }
    }
}