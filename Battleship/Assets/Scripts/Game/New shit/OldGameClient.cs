using Unity.Netcode;
using UnityEngine;
using Player;

namespace Game
{
    public class OldGameClient : OldGameClientRpc
    {
        [Header("Server")]
        [SerializeField]
        private OldGameServer _gameServer;

        public void Start()
        {
            _gameServer = FindObjectOfType<OldGameServer>();

            if (_gameServer is null)
            {
                Debug.LogError("GameClient's '_gameServer' reference is null!");
            }

            ulong clientId = NetworkManager.Singleton.LocalClientId;
            string clientName = FindObjectOfType<ProfileManager>().name;

            _selfBoard.SetUsername(clientName);
            _selfBoard.StartAsPlayerBoard(_gameServer);

            clientName = "fuckyou";

            Debug.Log($"Sending client info name:{clientName}, id:{clientId}");
            _gameServer.RegisterClientServerRpc(clientName, clientId);
        }

        public void Update()
        {
            switch (Phase)
            {
                case GamePhase.Build:
                    _selfBoard.ShipBuilder();
                    break;
                case GamePhase.Wait:
                    break;
                case GamePhase.Shoot:
                    _selfBoard.Shoot();
                    break;
                case GamePhase.Ended:
                    break;
                default:
                    break;
            }
        }
    }
}