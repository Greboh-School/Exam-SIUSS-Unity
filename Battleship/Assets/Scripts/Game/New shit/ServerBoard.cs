using System.Linq;
using UnityEngine;

namespace Game
{
    public class ServerBoard : ClientBoard
    {
        [Header("Client Info")]
        public ulong Id;
        public string UserName;
        public int Health;
        public GamePhase Phase;

        public void AddShipToServer(PlaceShipDTO dto)
        {
            var ship = new Ship();

            ship.ConfigureValues(dto.GridPosition, dto.Rotation, dto.Type);

            Health += ship.GetShipLength();

            _ships.Add(ship);
        }

        public bool AnyShipsHit(Vector2 gridPosition)
        {
            foreach (var shipObject in _ships)
            {
                var ship = shipObject.GetComponent<Ship>();

                if (ship.DoesShotHit(gridPosition))
                {
                    Health--;
                    return true;
                }
            }

            return false;
        }

        public bool IsShipPositionValid(PlaceShipDTO dto)
        {
            var ship = new Ship();
            ship.ConfigureValues(dto.GridPosition, dto.Rotation, dto.Type);

            var positionStatusList = ship.GetShipStatus();

            foreach (var positionStatus in positionStatusList)
            {
                var wantedPosition = positionStatus.Position;

                foreach (var placedShips in _ships)
                {
                    var takenPositions = placedShips.GetShipStatus();

                    var isOccupied = takenPositions.Any(x => x.Position == wantedPosition);

                    if (isOccupied is true)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetShipCount()
        {
            return _ships.Count;
        }
    }
}