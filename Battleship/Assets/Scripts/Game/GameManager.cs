using Assets.Scripts.Game;
using Game;
using Player;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("Boards")]
    [SerializeField]
    public OldPlayerBoard PlayerBoardA;
    public OldPlayerBoard PlayerBoardB;
    private OldPlayerBoard _shootingBoard;
    private OldPlayerBoard _waitingBoard;

    [Header("Board Prefab")]
    [SerializeField]
    private OldPlayerBoard _boardPrefrab;

    [Header("Effects")]
    [SerializeField]
    public GameObject _mouseMarkerPrefab;
    [SerializeField]
    public GameObject _hitMarkerPrefab;
    [SerializeField]
    public GameObject _missedMarkerPrefab;

    void Start()
    {
        if (FindObjectOfType<NetworkHandler>().sessionType is NetworkHandler.NetworkType.Server)
        {
            if (PlayerBoardA is null) PlayerBoardA = Instantiate(_boardPrefrab).GetComponent<OldPlayerBoard>();
            if (PlayerBoardB is null) PlayerBoardB = Instantiate(_boardPrefrab).GetComponent<OldPlayerBoard>();
        }

        _shootingBoard = PlayerBoardA;
        _waitingBoard = PlayerBoardB;

        PlayerBoardA.IsMyTurn = true;
        PlayerBoardB.IsMyTurn = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendShotToServerRpc(Vector2 gridPosition, ServerRpcParams rpcParams = default)
    {
        Debug.Log("Server received shot at: " + gridPosition);

        // Check if the shot is a hit on the waiting board
        bool isHit = _waitingBoard.AnyShipsHit(gridPosition);

        // Send hit/miss info to both players
        ConfirmHitOnClientRpc(gridPosition, isHit, _shootingBoard.OwnerClientId, _waitingBoard.OwnerClientId);

        // Switch turns
        SwitchTurns();
    }

    [ClientRpc]
    public void ConfirmHitOnClientRpc(Vector2 gridPosition, bool isHit, ulong shootingPlayerId, ulong waitingPlayerId)
    {
        if (IsOwner)
        {
            if (NetworkManager.Singleton.LocalClientId == shootingPlayerId)
            {
                PlayerBoardA.PlaceHitMarker(gridPosition, isHit);
            }
        }
        else
        {
            if (NetworkManager.Singleton.LocalClientId == waitingPlayerId)
            {
                PlayerBoardB.PlaceHitMarker(gridPosition, isHit);
            }
        }
    }

    private void SwitchTurns()
    {
        if (_shootingBoard == PlayerBoardA)
        {
            _shootingBoard = PlayerBoardB;
            _waitingBoard = PlayerBoardA;
        }
        else
        {
            _shootingBoard = PlayerBoardA;
            _waitingBoard = PlayerBoardB;
        }

        // Update turn status for the player whose turn it is
        UpdateTurnStatusClientRpc(true, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { _shootingBoard.OwnerClientId } }
        });

        // Update turn status for the player whose turn it is NOT
        UpdateTurnStatusClientRpc(false, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { _waitingBoard.OwnerClientId } }
        });
    }

    /// <summary>
    /// Targeted clientRPC: Only send package to client who needs it!
    /// </summary>
    /// <param name="isMyTurn"></param>
    /// <param name="clientRpcParams"></param>
    [ClientRpc]
    public void UpdateTurnStatusClientRpc(bool isMyTurn, ClientRpcParams clientRpcParams = default)
    {
        if (PlayerBoardA.IsOwner)
        {
            PlayerBoardA.IsMyTurn = isMyTurn;
        }
        else
        {
            PlayerBoardB.IsMyTurn = isMyTurn;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddShipToServerRpc(ShipType type, Vector2 gridPosition, int rotation, ulong clientId)
    {

    }
}
