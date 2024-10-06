using Assets.Scripts.Game;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField]
    protected GameObject _groundPlane;
    private int _planeSize = 10;

    [Header("Effects")]
    [SerializeField]
    protected GameObject _mouseMarkerPrefab;
    [SerializeField]
    protected GameObject _hitMarkerPrefab;
    [SerializeField]
    protected GameObject _missedMarkerPrefab;

    [Header("Ships")]
    [SerializeField]
    protected GameObject _carrierPrefab;
    [SerializeField]
    protected GameObject _battleshipPrefab;
    [SerializeField]
    protected GameObject _cruiserPrefab;
    [SerializeField]
    protected GameObject _submarinePrefab;
    [SerializeField]
    protected GameObject _destroyerPrefab;

    void Start()
    {
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform the raycast and check if we hit the plane
        if (Physics.Raycast(ray, out hit))
        {
            // Ensure we hit this particular plane
            if (hit.collider.gameObject == _groundPlane)
            {
                // Get the local hit point (relative to the plane's center)
                Vector3 localHitPoint = transform.InverseTransformPoint(hit.point);

                // Convert the local hit position to a grid coordinate
                int gridX, gridY;
                GetGridCell(localHitPoint, out gridX, out gridY);

                Vector3 gridCellCenter = GetGridCellCenter(gridX, gridY);
                MoveMarker(gridCellCenter);

                // Check for mouse click and handle the raycast
                if (Input.GetMouseButtonDown(0)) // Left-click
                {
                // Log or handle the result
                Debug.Log("Hit Grid Cell: (" + gridX + ", " + gridY + ")");
                }
            }
        }
    }

    void GetGridCell(Vector3 localHitPoint, out int gridX, out int gridY)
    {
        // Shift the plane's center to be at (0, 0)
        float halfPlaneSize = _planeSize / 2f;

        // Map the localHitPoint (-halfPlaneSize to halfPlaneSize) to (0 to planeSize)
        float hitX = localHitPoint.x + halfPlaneSize;
        float hitY = localHitPoint.z + halfPlaneSize;

        // Calculate grid cell size
        float cellSize = _planeSize / 10;

        // Determine which cell (0-based index) was hit
        gridX = Mathf.FloorToInt(hitX / cellSize);
        gridY = Mathf.FloorToInt(hitY / cellSize);

        // Clamp to ensure the indices don't go out of bounds
        gridX = Mathf.Clamp(gridX, 0, 10 - 1);
        gridY = Mathf.Clamp(gridY, 0, 10 - 1);
    }

    // Function to get the world position of the center of the grid cell
    Vector3 GetGridCellCenter(int gridX, int gridY)
    {
        // Calculate grid cell size
        float cellSize = _planeSize / 10;

        // Calculate the center of the grid cell in local coordinates
        float halfPlaneSize = _planeSize / 2f;
        float xPosition = (gridX * cellSize) - halfPlaneSize + (cellSize / 2f);
        float zPosition = (gridY * cellSize) - halfPlaneSize + (cellSize / 2f);

        // Return the world position by transforming from local to world space
        Vector3 localPosition = new Vector3(xPosition, 0, zPosition);
        return _groundPlane.transform.TransformPoint(localPosition);
    }

    // Function to move the marker to the specified position
    void MoveMarker(Vector3 position)
    {
        if (_mouseMarkerPrefab != null)
        {
            _mouseMarkerPrefab.transform.position = position;
        }
    }
}
