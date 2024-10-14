using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public enum ShipType { Carrier, Battleship, Cruiser, Submarine, Destoyer };

    public class Ship : MonoBehaviour
    {
        [Header("Ship Status")]
        [field: Tooltip("Ship filled position and isHit status")]
        [field: SerializeField]
        private List<PositionStatus> _shipStatus;

        [Header("Ship Info")]
        [Tooltip("ShipType")]
        [SerializeField]
        public ShipType Type;

        public void ConfigureValues(Vector2 gridPosition, int rotation, ShipType type)
        {
            Type = type;

            int shipLength = GetShipLength();

            _shipStatus = new List<PositionStatus>(shipLength);

            for (int i = 0; i < shipLength; i++)
            {
                _shipStatus.Add(new PositionStatus { Position = gridPosition, IsHit = false });

                switch (rotation)
                {
                    case 0:
                        gridPosition.x += 1;
                        break;
                    case 90:
                        gridPosition.y -= 1;
                        break;
                    case 180:
                        gridPosition.x -= 1;
                        break;
                    case 270:
                        gridPosition.y += 1;
                        break;
                    default:
                        Debug.LogError("Ship expects rotation of 90* intervals!");
                        break;
                }
            }
        }

        public List<PositionStatus> GetShipStatus()
        {
            return _shipStatus;
        }

        public bool DoesShotHit(Vector2 gridPosition)
        {
            var positionStatus = _shipStatus.FirstOrDefault(pos => pos.Position == gridPosition);

            if (positionStatus is null)
            {
                return false;
            }

            if (positionStatus.IsHit)
            {
                Debug.LogError($"Position {gridPosition} with ship {Type} already hit!");
                return false;
            }
            else
            {
                positionStatus.IsHit = true;
                return true;
            }
        }

        public int GetShipLength()
        {
            switch (Type)
            {
                case ShipType.Carrier:
                    return 5;
                case ShipType.Battleship:
                    return 4;
                case ShipType.Cruiser:
                    return 3;
                case ShipType.Submarine:
                    return 3;
                case ShipType.Destoyer:
                    return 2;
                default:
                    Debug.LogError("Exceed limits on ShipType enum, how the fuck?");
                    return 0;
            }
        }
    }
}