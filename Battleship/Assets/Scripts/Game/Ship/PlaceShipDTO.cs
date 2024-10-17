using Unity.Netcode;
using UnityEngine;

namespace Game
{
    [System.Serializable]
    public class PlaceShipDTO : INetworkSerializable
    {
        public ShipType Type;
        public Vector2 GridPosition;
        public int Rotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref GridPosition);
            serializer.SerializeValue(ref Rotation);
        }
    }
}