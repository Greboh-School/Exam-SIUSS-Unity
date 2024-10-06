using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class Board : GameManager
    {
        private GameObject[] _ships = new GameObject[5];
        private List<GameObject> _hitMarkers = new List<GameObject>();

        void Start()
        {
            PlaceShip(new Vector2(0,0), 90, Ship.ShipType.Carrier);
        }

        [ClientRpc]
        public void SyncronizeAll(List<GameObject> hitMarkers, GameObject[] ships) //Hopefully we never need to call this.
        {
            DestroyObjects(_hitMarkers.ToArray());
            DestroyObjects(_ships);

            InstantiateObjects(hitMarkers.ToArray());
            InstantiateObjects(ships);
            
            _hitMarkers = hitMarkers;
            _ships = ships;
        }

        private void DestroyObjects(GameObject[] objects)
        {
            foreach (GameObject obj in objects)
            {
                Destroy(obj);
            }
        }

        private void InstantiateObjects(GameObject[] objects)
        {
            foreach (GameObject obj in objects)
            {
                Instantiate(obj);
            }
        }

        public void PlaceHitMarker(Vector2 gridPosition)
        {
            GameObject markerObject;

            if (AnyShipsHit(gridPosition))
            {
                markerObject = _hitMarkerPrefab;
            }
            else
            {
                markerObject= _missedMarkerPrefab;
            }

            _hitMarkers.Add(markerObject);

            Instantiate(markerObject, new Vector3(gridPosition.x, 0, gridPosition.y), Quaternion.Euler(0, 0, 0));
        }

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

        public void PlaceShip(Vector2 position, int rotation, Ship.ShipType type) //TODO: DELETE THIS!
        {
            switch (type)
            {
                case Ship.ShipType.Carrier:
                    var shipPrefab = _carrierPrefab;
                    var ship = shipPrefab.GetComponent<Ship>();

                    ship.ConfigureValues(position, rotation, type);
                    Instantiate(shipPrefab, new Vector3(position.x, 0, position.y), Quaternion.Euler(0, rotation, 0));
                    break;
                case Ship.ShipType.Battleship:
                    break;
                case Ship.ShipType.Cruiser:
                    break;
                case Ship.ShipType.Submarine:
                    break;
                case Ship.ShipType.Destoyer:
                    break;
            }
        }
    }
}