using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class Server : MonoBehaviour
    {
        [Header("Boards")]
        public Client ClientA;
        public Client ClientB;

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