using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public enum ShipType { Carrier, Battleship, Cruiser, Submarine, Destoyer };

    public class Ship : MonoBehaviour
    {
        [Header("Ship Status")]
        [field: Tooltip("Ship positions and isHit status")]
        [field: SerializeField]
        private List<PositionStatus> _shipStatus;

        [Header("Ship Info")]
        [SerializeField]
        public ShipType Type;

        /// <summary>
        /// Sets up the List of PositionStatus which contains position and hit data.
        /// </summary>
        /// <param name="dto"></param>
        public void ConfigureValues(PlaceShipDTO dto)
        {
            Type = dto.Type;
            var pos = dto.GridPosition;

            int shipLength = GetShipLength();

            _shipStatus = new List<PositionStatus>(shipLength);

            for (int i = 0; i < shipLength; i++)
            {
                _shipStatus.Add(new PositionStatus { Position = pos, IsHit = false });

                switch (dto.Rotation)
                {
                    case 0:
                        pos.x += 1;
                        break;

                    case 90:
                        pos.y -= 1;
                        break;

                    case 180:
                        pos.x -= 1;
                        break;

                    case 270:
                        pos.y += 1;
                        break;

                    default:
                        Debug.LogError("Ship expects rotation of 90* intervals!");
                        break;
                }
            }
        }

        /// <summary>
        /// Checks internal list of PositionStatus wether your shot hits
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <returns>boolean if shot it</returns>
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

            positionStatus.IsHit = true;
            return true;
        }

        /// <summary>
        /// Uses internal ShipType and give back the length
        /// </summary>
        /// <returns>Ship length</returns>
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

        /// <summary>
        /// Gets location and hit data
        /// </summary>
        /// <returns></returns>
        public List<PositionStatus> GetShipStatus()
        {
            return _shipStatus;
        }
    }
}