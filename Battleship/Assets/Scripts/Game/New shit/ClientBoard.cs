using Assets.Scripts.Game.New_shit;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class ClientBoard : NetworkBehaviour
    {
        [Header("Server")]
        [SerializeField]
        private GameServer _gameServer;

        [Header("Ship placement")]
        [SerializeField]
        private PrefabManager _prefabManager;
        [SerializeField]
        private GameObject _shipBeingPlaced;
        [SerializeField]
        private int _shipRotation;

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

            _shipBeingPlaced = _prefabManager.GetShipPrefab(_ships.Count());

            _shipBeingPlaced = Instantiate(_shipBeingPlaced, this.gameObject.transform);

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

            _shipBeingPlaced.transform.position = gridWorldPosition;

            _shipBeingPlaced = _prefabManager.GetShipPrefab(_ships.Count());

            _shipBeingPlaced = Instantiate(_shipBeingPlaced, this.gameObject.transform);
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

            _shipBeingPlaced.transform.localPosition = gridWorldPosition;
            _shipBeingPlaced.transform.localRotation = Quaternion.Euler(0, _shipRotation, 0);

            if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
            {
                var Ship = _shipBeingPlaced.GetComponent<Ship>();

                var dto = new PlaceShipDTO { GridPosition = gridPosition, Rotation = _shipRotation, Type = Ship.Type };

                _gameServer.TryPlaceShipServerRpc(dto);
            }

            if (Input.GetMouseButtonDown(1))
            {
                _shipRotation += 90;

                if (_shipRotation > 270) _shipRotation = 0;
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

        public void StartAsPlayerBoard(GameServer gameServer)
        {
            _ships = new List<Ship>(5);
            _prefabManager = FindObjectOfType<PrefabManager>();
            _gameServer = gameServer;
        }
    }
}