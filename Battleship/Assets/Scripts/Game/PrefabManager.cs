using Game;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    [Header("Ships")]
    public GameObject CarrierPrefab;
    public GameObject BattleshipPrefab;
    public GameObject CruiserPrefab;
    public GameObject SubmarinePrefab;
    public GameObject DestroyerPrefab;

    [Header("Markers")]
    public GameObject HitMarkerPrefab;
    public GameObject MissedMarkerPrefab;
    public GameObject MouseMarkerPrefab;

    [Header("Game")]
    public GameObject ClientPrefab;
    public GameObject ServerPrefab;

    public GameObject GetShipPrefab(ShipType type)
    {
        switch (type)
        {
            case ShipType.Carrier: return CarrierPrefab;
            case ShipType.Battleship: return BattleshipPrefab;
            case ShipType.Cruiser: return CruiserPrefab;
            case ShipType.Submarine: return SubmarinePrefab;
            case ShipType.Destoyer: return DestroyerPrefab;
            default: return null;
        }
    }
}
