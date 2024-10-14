using Assets.Scripts.Game.New_shit;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class GameBoard : NetworkBehaviour
    {
        [Header("Ship placement")]
        public PrefabManager PrefabManager;
        public GameObject ShipBeingPlaced;
        public int ShipRotation;

        [Header("UI")]
        public TMP_Text Text_UserName;
        public GameObject MouseMarker;

        [field: Header("Markers & Ships")]
        public List<GameObject> HitMarkers = new List<GameObject>();
        public List<Ship> Ships = new List<Ship>();

        [Header("GameClient reference")]
        public GameClient GameClient;

        private void Start()
        {
            PrefabManager = FindObjectOfType<PrefabManager>();

            ShipBeingPlaced = PrefabManager.GetShipPrefabFromIndex(Ships.Count());

            ShipBeingPlaced = Instantiate(ShipBeingPlaced, this.gameObject.transform);
        }

        public void ShipBuilder()
        {
            Vector2 gridPosition = GridTools.GetGridPositionFromRayCast(this.gameObject);

            if (gridPosition.x is -1)
            {
                return;
            }

            Vector3 gridWorldPosition = GridTools.GetWorldPositionFromGrid(gridPosition, this.gameObject);

            ShipBeingPlaced.transform.localPosition = gridWorldPosition;
            ShipBeingPlaced.transform.localRotation = Quaternion.Euler(0, ShipRotation, 0);

            if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
            {
                var Ship = ShipBeingPlaced.GetComponent<Ship>();

                var dto = new PlaceShipDTO { GridPosition = gridPosition, Rotation = ShipRotation, Type = Ship.Type };

                GameClient.TryPlaceShipServerRpc(dto);
            }

            if (Input.GetMouseButtonDown(1))
            {
                ShipRotation += 90;

                if (ShipRotation > 270) ShipRotation = 0;
            }
        }

        public void ClientPlaceShip(PlaceShipDTO dto)
        {
            Vector3 gridWorldPosition = GridTools.GetWorldPositionFromGrid(dto.GridPosition, this.gameObject);

            ShipBeingPlaced.transform.localPosition = gridWorldPosition;
            ShipBeingPlaced.transform.localRotation = Quaternion.Euler(0, dto.Rotation, 0);

            ShipBeingPlaced = PrefabManager.GetShipPrefabFromType(dto.Type);

            ShipBeingPlaced = Instantiate(ShipBeingPlaced, this.gameObject.transform);
        }

        public void AddShipToServer(PlaceShipDTO dto)
        {
            Vector3 gridWorldPosition = GridTools.GetWorldPositionFromGrid(dto.GridPosition, this.gameObject);

            var shipObj = Instantiate(PrefabManager.GetShipPrefabFromType(dto.Type), this.gameObject.transform);

            shipObj.transform.localPosition = gridWorldPosition;
            shipObj.transform.localRotation = Quaternion.Euler(0, dto.Rotation, 0);

            var shipScript = shipObj.GetComponent<Ship>();

            GameClient.Health += shipScript.GetShipLength();

            Ships.Add(shipScript);
        }

        public bool IsShipPositionValid(PlaceShipDTO dto)
        {
            var shipObj = Instantiate(PrefabManager.GetShipPrefabFromType(dto.Type));
            var shipScript = shipObj.GetComponent<Ship>();

            shipScript.ConfigureValues(dto.GridPosition, dto.Rotation, dto.Type);

            var positionStatusList = shipScript.GetShipStatus();

            foreach (var positionStatus in positionStatusList)
            {
                var wantedPosition = positionStatus.Position;

                foreach (var placedShips in Ships)
                {
                    var takenPositions = placedShips.GetShipStatus();

                    var isOccupied = takenPositions.Any(x => x.Position == wantedPosition);

                    if (isOccupied is true)
                    {
                        return false;
                    }
                }
            }

            Destroy(shipObj);

            return true;
        }
    }
}