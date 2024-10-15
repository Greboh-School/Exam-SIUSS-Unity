using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Game
{
    public class GameServer : MonoBehaviour
    {
        [Header("Boards")]
        public GameClient ClientA;
        public GameClient ClientB;

        public void RegisterPlayerAsReady(GameClient client)
        {
            if(ClientA is null)
            {
                ClientA = client;
            }
            else if (ClientB is null)
            {
                ClientB = client;
            }

            if (ClientA is not null && ClientB is not null)
            {
                //Both clients ready to fire! Let ClientA take first turn!
                ClientA.ChangePhase(GamePhase.Shoot);
            }
        }

        public void AttackOtherClient(Vector2 gridPosition, GameClient attackingClient)
        {
            var targetClient = GetOtherClient(attackingClient.Id);

            var isHit = targetClient.AnyShipsHit(gridPosition);

            attackingClient.ShotResponseClientRpc(gridPosition, isHit, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { attackingClient.Id }
                }
            });

            targetClient.SendAttackClientRpc(gridPosition, isHit, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { targetClient.Id }
                }
            });

            CheckForGameOver();
        }

        private GameClient GetOtherClient(ulong knownClientId)
        {
            if (knownClientId == ClientA.Id) return ClientB;
            else return ClientA;
        }

        private void CheckForGameOver()
        {
            if (ClientA.Health is 0)
            {
                ClientA.GameOverClientRpc(ClientB.name);
                ClientA.Phase = GamePhase.Ended;
                ClientB.Phase = GamePhase.Ended;
            }
            else if (ClientB.Health is 0)
            {
                ClientB.GameOverClientRpc(ClientA.name);
                ClientA.Phase = GamePhase.Ended;
                ClientB.Phase = GamePhase.Ended;
            }
        }
        
        
        
        
        
        //TODO: Test shooting logic
    }
}