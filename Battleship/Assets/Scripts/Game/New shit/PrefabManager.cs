using Unity.Netcode;
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
    public NetworkObject ClientPrefab;
    public NetworkObject ServerPrefab;


    public GameObject GetShipPrefab(int index)
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
}
