using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class GameBoard : NetworkBehaviour
    {
        [Header("Ship building phase")]
        public PrefabManager PrefabManager;
        public GameObject ShipBeingPlaced;
        public int ShipRotation;
        public bool InstantiateShipForPlacing = false;

        [Header("UI")]
        public TMP_Text Text_UserName;
        public GameObject MouseMarker;

        [field: Header("Markers & Ships")]
        public List<GameObject> HitMarkers = new List<GameObject>();
        public List<Ship> Ships = new List<Ship>();

        [Header("Controlling GameClient")]
        public GameClient GameClient;

        private void Start()
        {
            PrefabManager = FindObjectOfType<PrefabManager>();

            var prefab = PrefabManager.GetShipPrefabFromIndex(Ships.Count());

            if (InstantiateShipForPlacing)
            {
                ShipBeingPlaced = Instantiate(prefab, this.gameObject.transform);
            }
        }

        public void BuildPhase()
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

        /// <summary>
        /// Client side logic for placing current ship at server told position, then getting next ship prepared.
        /// </summary>
        /// <param name="dto"></param>
        public void BuildNextShip(PlaceShipDTO dto)
        {
            Vector3 gridWorldPosition = GridTools.GetWorldPositionFromGrid(dto.GridPosition, this.gameObject);

            ShipBeingPlaced.transform.localPosition = gridWorldPosition;
            ShipBeingPlaced.transform.localRotation = Quaternion.Euler(0, dto.Rotation, 0);

            ShipBeingPlaced = PrefabManager.GetShipPrefabFromType(dto.Type);

            ShipBeingPlaced = Instantiate(ShipBeingPlaced, this.gameObject.transform);
        }

        /// <summary>
        /// Server side logic for instantiating ship and filling ship values.
        /// </summary>
        /// <param name="dto"></param>
        public void PlaceShip(PlaceShipDTO dto)
        {
            Vector3 gridWorldPosition = GridTools.GetWorldPositionFromGrid(dto.GridPosition, this.gameObject);

            var shipObj = Instantiate(PrefabManager.GetShipPrefabFromType(dto.Type), this.gameObject.transform);

            shipObj.transform.localPosition = gridWorldPosition;
            shipObj.transform.localRotation = Quaternion.Euler(0, dto.Rotation, 0);

            var shipScript = shipObj.GetComponent<Ship>();
            shipScript.ConfigureValues(dto);

            GameClient.Health += shipScript.GetShipLength();

            Ships.Add(shipScript);
        }

        public bool IsShipPositionValid(PlaceShipDTO dto)
        {
            var shipObj = Instantiate(PrefabManager.GetShipPrefabFromType(dto.Type));
            var shipScript = shipObj.GetComponent<Ship>();

            shipScript.ConfigureValues(dto);

            var positionStatusList = shipScript.GetShipStatus();

            foreach (var positionStatus in positionStatusList)
            {
                var wantedPosition = positionStatus.Position;

                if(wantedPosition.x < 0 || wantedPosition.x > 9)
                {
                    Destroy(shipObj);
                    return false;
                }
                if (wantedPosition.y < 0 || wantedPosition.y > 9)
                {
                    Destroy(shipObj);
                    return false;
                }

                foreach (var placedShips in Ships)
                {
                    var takenPositions = placedShips.GetShipStatus();

                    var isOccupied = takenPositions.Any(x => x.Position == wantedPosition);

                    if (isOccupied is true)
                    {
                        Destroy(shipObj);
                        return false;
                    }
                }
            }

            Destroy(shipObj);
            return true;
        }

        public void ShootingPhase()
        {
            if(MouseMarker == null)
            {
                var prefab = PrefabManager.MouseMarkerPrefab;

                MouseMarker = Instantiate(prefab, this.gameObject.transform);
            }

            Vector2 gridPosition = GridTools.GetGridPositionFromRayCast(this.gameObject);

            if (gridPosition.x is -1)
            {
                return;
            }

            Vector3 gridWorldPosition = GridTools.GetWorldPositionFromGrid(gridPosition, this.gameObject);

            MouseMarker.transform.localPosition = gridWorldPosition;

            if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
            {
                GameClient.SendShotToServerRpc(gridPosition);
            }
        }

        public void InstantiateHitmarker(Vector2 gridPosition, bool isHit)
        {
            var markerPrefab = isHit ? PrefabManager.HitMarkerPrefab : PrefabManager.MissedMarkerPrefab;
            var marker = Instantiate(markerPrefab);

            Vector3 gridWorldPosition = GridTools.GetWorldPositionFromGrid(gridPosition, this.gameObject);

            marker.transform.SetParent(this.transform);
            marker.transform.localPosition = gridWorldPosition;
        }
    }
}