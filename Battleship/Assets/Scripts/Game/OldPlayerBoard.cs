using Game;
using Player;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class OldPlayerBoard : NetworkBehaviour
    {
        [Header("Boards")]
        [SerializeField]
        private GameObject _myBoard;
        [SerializeField]
        private GameObject _enemyBoard;

        [Header("Player names")] //TODO: IMPLEMENT
        [SerializeField]
        private string _myName;
        [SerializeField]
        private string _enemyName;

        [Header("Ships")]
        [SerializeField]
        private GameObject _carrierPrefab;
        [SerializeField]
        private GameObject _battleshipPrefab;
        [SerializeField]
        private GameObject _cruiserPrefab;
        [SerializeField]
        private GameObject _submarinePrefab;
        [SerializeField]
        private GameObject _destroyerPrefab;

        [Header("Game Logic - Server controlled")]
        [SerializeField]
        public bool IsMyTurn = false;

        private GameManager _gameManager;
        private List<GameObject> _hitMarkers = new List<GameObject>();

        private GameObject[] _ships = new GameObject[5];
        private int _currentShipIndex = 0;

        private void Start()
        {
            _myName = FindObjectOfType<ProfileManager>().name;
            _gameManager = FindAnyObjectByType<GameManager>();
        }

        public void Update()
        {
            if (IsMyTurn)
            {
                // Ship placement phase
                if (_currentShipIndex < _ships.Length && Input.GetMouseButtonDown(0))
                {
                    Vector2 gridPosition = GetGridPositionFromRayCast();
                    if (gridPosition.x != -1 && CheckShipPlacementValidity(gridPosition))
                    {
                        PlaceShipOnBoard(gridPosition);
                    }
                }

                // Shooting phase
                else if (_currentShipIndex == _ships.Length)
                {
                    var gridPosition = GetGridPositionFromRayCast();
                    if (gridPosition.x is -1) return;

                    if (Input.GetMouseButtonDown(0)) // Left-click for shooting
                    {
                        _gameManager.SendShotToServerRpc(gridPosition);
                    }
                }
            }
        }

        //[ClientRpc]
        //public void SyncronizeAllClientRpc(List<GameObject> hitMarkers, GameObject[] ships) //Hopefully we never need to call this.
        //{
        //    DestroyObjects(_hitMarkers.ToArray());
        //    DestroyObjects(_ships);

        //    InstantiateObjects(hitMarkers.ToArray());
        //    InstantiateObjects(ships);

        //    _hitMarkers = hitMarkers;
        //    _ships = ships;
        //}

        public bool AnyShipsHit(Vector2 gridPosition)
        {
            foreach (var shipObject in _ships)
            {
                var ship = shipObject.GetComponent<Ship>();

                if (ship.DoesShotHit(gridPosition))
                {
                    return true;
                }
            }

            return false;
        }

        public void PlaceHitMarker(Vector2 gridPosition, bool isHit)
        {
            GameObject activeBoard = IsMyTurn ? _enemyBoard : _myBoard;

            GameObject markerObject = isHit ? _gameManager._hitMarkerPrefab : _gameManager._missedMarkerPrefab;

            Vector3 worldPosition = GetWorldPositionFromGrid(gridPosition, activeBoard);

            GameObject markerInstance = Instantiate(markerObject, worldPosition, Quaternion.identity, activeBoard.transform);

            _hitMarkers.Add(markerInstance);
        }

        private void DestroyObjects(GameObject[] objects)
        {
            foreach (GameObject obj in objects)
            {
                Destroy(obj);
            }
        }

        private Vector2 GetGridPositionFromRayCast()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == _myBoard) // Ensure the raycast hits plane
                {
                    Vector3 localHitPoint = _myBoard.transform.InverseTransformPoint(hit.point);

                    float planeWidth = _myBoard.GetComponent<Renderer>().bounds.size.x;
                    float planeHeight = _myBoard.GetComponent<Renderer>().bounds.size.z;

                    int gridSize = 10;
                    float cellWidth = planeWidth / gridSize;
                    float cellHeight = planeHeight / gridSize;

                    int gridX = Mathf.FloorToInt((localHitPoint.x + planeWidth / 2) / cellWidth); // Convert the local hitpoint to grid pos
                    int gridY = Mathf.FloorToInt((localHitPoint.z + planeHeight / 2) / cellHeight);

                    return new Vector2(gridX, gridY);
                }
            }

            return new Vector2(-1, -1); // Invalid position return
        }

        public Vector3 GetWorldPositionFromGrid(Vector2 gridPosition, GameObject parent)
        {
            float gridWidth = parent.transform.localScale.x;
            float gridHeight = parent.transform.localScale.z;

            int gridSize = 10;
            float cellWidth = gridWidth / gridSize;
            float cellHeight = gridHeight / gridSize;

            float localPosX = (gridPosition.x * cellWidth) - (gridWidth / 2) + (cellWidth / 2);
            float localPosZ = (gridPosition.y * cellHeight) - (gridHeight / 2) + (cellHeight / 2);

            Vector3 localPosition = new Vector3(localPosX, 0.1f, localPosZ); // Adjust to be slightly above plane
            Vector3 worldPosition = parent.transform.TransformPoint(localPosition); // Convert local to world

            return worldPosition;
        }

        private void InstantiateObjects(GameObject[] objects)
        {
            foreach (GameObject obj in objects)
            {
                Instantiate(obj);
            }
        }

        private bool CheckShipPlacementValidity(Vector2 gridPosition)
        {
            // Check grid boundaries and overlapping logic (to be implemented)
            return gridPosition.x >= 0 && gridPosition.x < 10 && gridPosition.y >= 0 && gridPosition.y < 10;
        }

        private void PlaceShipOnBoard(Vector2 gridPosition)
        {
            GameObject shipPrefab = GetCurrentShipPrefab();
            if (shipPrefab == null) return;

            GameObject shipInstance = Instantiate(shipPrefab, GetWorldPositionFromGrid(gridPosition, _myBoard), Quaternion.identity, _myBoard.transform);

            // Rotate and configure ship
            int rotation = GetRotationInput(); // Function to get rotation input (90 degrees)
            Ship ship = shipInstance.GetComponent<Ship>();
            //ship.ConfigureValues(gridPosition, rotation);

            // Add ship to array
            _ships[_currentShipIndex] = shipInstance;
            _currentShipIndex++;

            // Send data to the server
            //_gameManager.AddShipToServerRpc((Ship.ShipType)_currentShipIndex, gridPosition, rotation, NetworkManager.Singleton.LocalClientId);
        }

        private GameObject GetCurrentShipPrefab()
        {
            switch (_currentShipIndex)
            {
                case 0: return _carrierPrefab;
                case 1: return _battleshipPrefab;
                case 2: return _cruiserPrefab;
                case 3: return _submarinePrefab;
                case 4: return _destroyerPrefab;
                default: return null;
            }
        }

        private int GetRotationInput()
        {
            // Use input to get the current rotation angle, e.g., 0, 90, 180, 270 degrees
            return 0; // Placeholder for rotation logic
        }
    }
}