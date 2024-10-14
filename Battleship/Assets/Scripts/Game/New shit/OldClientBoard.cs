using Assets.Scripts.Game.New_shit;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class OldClientBoard : NetworkBehaviour
    {
        [Header("Server")]
        [SerializeField]
        private OldGameServer _gameServer;

        [Header("Ship placement")]
        [SerializeField]
        private PrefabManager _prefabManager;
        [SerializeField]
        private GameObject ShipBeingPlaced { get; set; }
        [SerializeField]
        private int ShipRotation;

        [Header("UI")]
        [SerializeField]
        private TMP_Text _textField;
        [SerializeField]
        private GameObject _mouseMarker;

        [field: Header("Markers & Ships")]
        [field: SerializeField]
        protected List<GameObject> _hitMarkers;
        [field: SerializeField]
        protected List<Ship> _ships;

        private void Start()
        {
            _prefabManager = FindObjectOfType<PrefabManager>();

            ShipBeingPlaced = _prefabManager.GetShipPrefabFromIndex(_ships.Count());

            ShipBeingPlaced = Instantiate(ShipBeingPlaced, this.gameObject.transform);

            _hitMarkers = new List<GameObject>();
        }

        public void InstantiateMarker(Vector2 gridPosition, bool isHit)
        {
            var marker = isHit ? _prefabManager.HitMarkerPrefab : _prefabManager.MissedMarkerPrefab;

            Vector3 gridWorldPosition = GridTools.GetWorldPositionFromGrid(gridPosition, this.gameObject);

            Instantiate(marker, this.gameObject.transform);

            marker.transform.position = gridWorldPosition;
        }

        public void PlaceShip(PlaceShipDTO dto)
        {
            Vector3 gridWorldPosition = GridTools.GetWorldPositionFromGrid(dto.GridPosition, this.gameObject);

            ShipBeingPlaced.transform.position = gridWorldPosition;

            ShipBeingPlaced = _prefabManager.GetShipPrefabFromIndex(_ships.Count());

            ShipBeingPlaced = Instantiate(ShipBeingPlaced, this.gameObject.transform);
        }

        public void SetUsername(string username)
        {
            _textField.text = username;
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

                _gameServer.TryPlaceShipServerRpc(dto);
            }

            if (Input.GetMouseButtonDown(1))
            {
                ShipRotation += 90;

                if (ShipRotation > 270) ShipRotation = 0;
            }
        }

        public void Shoot()
        {
            if (_mouseMarker is null)
            {
                _mouseMarker = _prefabManager.MouseMarkerPrefab;

                _mouseMarker = Instantiate(_mouseMarker, this.gameObject.transform);
            }

            Vector2 gridPosition = GridTools.GetGridPositionFromRayCast(this.gameObject);

            if (gridPosition.x is -1)
            {
                return;
            }

            Vector3 gridWorldPosition = GridTools.GetWorldPositionFromGrid(gridPosition, this.gameObject);

            _mouseMarker.transform.position = gridWorldPosition;

            if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
            {
                _gameServer.SendShotToServerRpc(gridPosition);
            }
        }

        public void StartAsPlayerBoard(OldGameServer gameServer)
        {
            _ships = new List<Ship>(5);
            _prefabManager = FindObjectOfType<PrefabManager>();
            _gameServer = gameServer;
        }
    }
}