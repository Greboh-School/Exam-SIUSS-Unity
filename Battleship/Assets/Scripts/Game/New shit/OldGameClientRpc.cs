using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class OldGameClientRpc : NetworkBehaviour
    {       
        [Header("Boards")]
        [SerializeField]
        protected OldClientBoard _selfBoard;
        [SerializeField]
        protected OldClientBoard _enemyBoard;

        [Header("Phase logic")]
        public GamePhase Phase;

        [ClientRpc]
        public void SetEnemyNameClientRpc(string userName, ClientRpcParams clientRpcParams = default)
        {
            _enemyBoard.SetUsername(userName);
        }

        [ClientRpc]
        public void PlaceShipClientRpc(PlaceShipDTO dto, ClientRpcParams clientRpcParams = default)
        {
            _selfBoard.PlaceShip(dto);
        }

        [ClientRpc]
        public void ChangePhaseClientRpc(GamePhase phase, ClientRpcParams clientRpcParams = default)
        {
            Phase = phase;
        }

        [ClientRpc]
        public void ShotResponseClientRpc(Vector2 gridPosition, bool isHit, ClientRpcParams clientRpcParams = default)
        {
            _enemyBoard.InstantiateMarker(gridPosition, isHit);

            Phase = GamePhase.Wait;
        }

        [ClientRpc]
        public void AttackClientRpc(Vector2 gridPosition, bool isHit, ClientRpcParams clientRpcParams = default)
        {
            _selfBoard.InstantiateMarker(gridPosition, isHit);

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