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


    public GameObject GetShipPrefabFromIndex(int index)
    {
        switch (index)
        {
            case 0: return CarrierPrefab;
            case 1: return BattleshipPrefab;
            case 2: return CruiserPrefab;
            case 3: return SubmarinePrefab;
            case 4: return DestroyerPrefab;
            default: return null;
        }
    }

    public GameObject GetShipPrefabFromType(ShipType type)
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
