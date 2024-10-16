using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class GameServer : MonoBehaviour
    {
        [Header("Boards")]
        public GameClient ClientA;
        public GameClient ClientB;

        public void RegisterPlayer(GameClient client)
        {
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

        public void CheckForGameStart(GameClient client)
        {
            if (ClientA.Phase is GamePhase.Ready && ClientB.Phase is GamePhase.Ready)
            {
                //Both clients ready to fire! Let ClientA take first turn!
                ClientA.ChangePhase(GamePhase.Shoot);
            }
        } 

        public void SendAttack(Vector2 gridPosition, GameClient attackingClient)
        {
            var targetClient = GetOtherClient(attackingClient.Id);

            var isHit = targetClient.AnyShipsHit(gridPosition);

            attackingClient.EnemyBoard.InstantiateHitmarker(gridPosition, isHit);
            targetClient.SelfBoard.InstantiateHitmarker(gridPosition, isHit);

            attackingClient.InstantiateHitmarkerClientRpc(gridPosition, isHit, true);
            targetClient.InstantiateHitmarkerClientRpc(gridPosition, isHit, false);

            attackingClient.ChangePhase(GamePhase.Wait);
            targetClient.ChangePhase(GamePhase.Shoot);

            CheckForGameOver();
        }

        private GameClient GetOtherClient(ulong knownClient)
        {
            if (knownClient == ClientA.Id)
            {
                return ClientB;
            }

            return ClientA;
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
    }
}