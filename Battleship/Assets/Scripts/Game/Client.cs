using Player;
<<<<<<< Updated upstream
=======
using System;
using Assets.Scripts.UI.Views.SubViews;
>>>>>>> Stashed changes
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public enum GamePhase
    { Build, Ready, Wait, Shoot, Ended }

    public class Client : NetworkBehaviour
    {
        [Header("Controlling Server")]
        [SerializeField]
        private Server Server;

        [Header("Boards")]
        public Board SelfBoard;
        public Board EnemyBoard;

        [Header("Client Info")]
        public ulong Id;
        public int Health;
        public GamePhase Phase;
        public string UserName;

        [Header("UI")]
        [SerializeField]
        private TMP_Text Text_TurnDisplay;
<<<<<<< Updated upstream

        public void Start()
=======
        [SerializeField]
        private GameHUD _hud;
        public override void OnNetworkSpawn()
>>>>>>> Stashed changes
        {
            if (!IsOwner && IsClient)
            {
                // Hacky implementation to 'hide' enemy client board - No ships are shared, so no cheating!
                transform.localPosition = new Vector3(0, 0, 30);
            }

            EnemyBoard.GameClient = this;
            SelfBoard.GameClient = this;

            if (IsOwner && IsClient)
            {
                var profile = FindObjectOfType<ProfileManager>().Profile;

                SendInfoAndStartServerRpc(NetworkManager.Singleton.LocalClientId, profile.Username);

                SelfBoard.Text_UserName.text = UserName;
                Id = NetworkManager.Singleton.LocalClientId;

                SelfBoard.InstantiateShipForPlacing = true;

                Text_TurnDisplay.text = "Place your ships! Use both mousebuttons!";
            }
<<<<<<< Updated upstream
=======
            
            var profile = FindObjectOfType<ProfileManager>().Profile;
            AccessToken = profile.AccessToken;

            SendInfoAndStartServerRpc(NetworkManager.Singleton.LocalClientId, profile.Username, profile.UserId.ToString());

            SelfBoard.Text_UserName.text = UserName;
            Id = NetworkManager.Singleton.LocalClientId;

            SelfBoard.InstantiateShipForPlacing = true;

            Text_TurnDisplay.text = "Place your ships! Use both mousebuttons!";
            _hud = FindFirstObjectByType<GameHUD>();
>>>>>>> Stashed changes
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

        /// <summary>
        /// Send client info and register on server
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="userName"></param>
        [ServerRpc]
        private void SendInfoAndStartServerRpc(ulong clientId, string userName)
        {
            Id = clientId;
            UserName = userName;
            SelfBoard.GameClient = this;

            Server = FindObjectOfType<Server>();

            Server.RegisterPlayer(this);
        }

        [ClientRpc]
        public void SendOppenentNameClientRpc(string userName, ClientRpcParams clientRpcParams = default)
        {
            EnemyBoard.Text_UserName.text = userName;
        }

        /// <summary>
        /// Checks if position of ship is valid, then places for both server and respective client.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="rpcParams"></param>
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

        [ClientRpc]
        private void BuildNextShipClientRpc(PlaceShipDTO dto, ClientRpcParams clientRpcParams = default)
        {
            SelfBoard.BuildNextShip(dto);
        }

        /// <summary>
        /// Checks for all ships being placed, then checks for 'Server GameStart'
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Changes Phase on Server side and Client side
        /// </summary>
        /// <param name="phase"></param>
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

        /// <summary>
        /// Change the current phase on the clients side
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="clientRpcParams"></param>
        [ClientRpc]
        private void ChangePhaseClientRpc(GamePhase phase, ClientRpcParams clientRpcParams = default)
        {
            Phase = phase;

            switch (Phase)
            {
                case GamePhase.Ready:
                    Text_TurnDisplay.text = $"Waiting on '{EnemyBoard.Text_UserName.text}' to finish";
                    break;
                case GamePhase.Wait:
                    Text_TurnDisplay.text = $"'{EnemyBoard.Text_UserName.text}' is shooting";
                    break;
                case GamePhase.Shoot:
                    Text_TurnDisplay.text = $"Your turn";
                    break;
            }
        }

        /// <summary>
        /// Checks through all ships and their respective positions for a hit
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <returns></returns>
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

        [ServerRpc]
        public void SendShotToServerRpc(Vector2 gridPosition)
        {
            Server.SendAttack(gridPosition, this);
        }

        [ClientRpc]
        public void ShotResponseClientRpc(Vector2 gridPosition, bool isHit)
        {
            EnemyBoard.InstantiateHitmarker(gridPosition, isHit);

            Destroy(EnemyBoard.MouseMarker.gameObject);
        }

        [ClientRpc]
        public void SendShotToClientRpc(Vector2 gridPosition, bool isHit)
        {
            SelfBoard.InstantiateHitmarker(gridPosition, isHit);
        }

        [ClientRpc]
        public void DisconnectClientRpc()
        {
            Text_TurnDisplay.text = $"Your opponent has disconnected - The game will reset";
        }

        [ClientRpc]
        public void GameOverClientRpc(string winningUser)
        {
            Phase = GamePhase.Ended;

            Text_TurnDisplay.text = $"'{winningUser}' has won!";

            //TODO: Winning logic
        }
        
        [ClientRpc]
        public void SetHudMessageClientRPC(string messageType, string message, string sender, ClientRpcParams clientRpcParams = default)
        {
            _hud.SetMessage(messageType, message, sender);
        }
    }
}