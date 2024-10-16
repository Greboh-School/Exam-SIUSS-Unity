using UnityEngine;

namespace Game
{
    public class GridTools : MonoBehaviour
    {
        /// <summary>
        /// Input <paramref name="obj"/> must contain a Plane
        /// <para>Will raycast using plane and convert hitpoint to local grid of 10x10</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Vector2 GetGridPositionFromRayCast(GameObject obj)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float enter;

            // Define the plane based on the object's up direction
            Plane plane = new Plane(Vector3.up, obj.transform.position);

            if (plane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                // Convert hit point to local coordinates of the object containing the plane
                Vector3 localHitPoint = obj.transform.InverseTransformPoint(hitPoint);

                // Get the MeshRenderer from the child plane object
                MeshRenderer meshRenderer = obj.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer == null)
                {
                    Debug.LogError("MeshRenderer component not found in children of " + obj.name);
                    return new Vector2(-1, -1); // Return an invalid grid position
                }

                float planeWidth = meshRenderer.bounds.size.x;
                float planeHeight = meshRenderer.bounds.size.z;

                int gridSize = 10;
                float cellWidth = planeWidth / gridSize;
                float cellHeight = planeHeight / gridSize;

                // Convert the local hit point to grid coordinates
                int gridX = Mathf.FloorToInt((localHitPoint.x + (planeWidth / 2)) / cellWidth);
                int gridY = Mathf.FloorToInt((localHitPoint.z + (planeHeight / 2)) / cellHeight);

                // Ensure grid positions are within bounds
                gridX = Mathf.Clamp(gridX, 0, gridSize - 1);
                gridY = Mathf.Clamp(gridY, 0, gridSize - 1);

                return new Vector2(gridX, gridY);
            }

            // Return an invalid grid position if the ray doesn't hit the plane
            return new Vector2(-1, -1);
        }

        /// <summary>
        /// Will convert <paramref name="gridPosition"/> into worldspace position for given input <paramref name="obj"/>
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Vector3 GetWorldPositionFromGrid(Vector2 gridPosition, GameObject obj)
        {
            // Get the MeshRenderer from the child plane object
            MeshRenderer meshRenderer = obj.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogError("MeshRenderer component not found in children of " + obj.name);
                return Vector3.zero; // Return a zero vector for an invalid position
            }

            // Get the bounds of the mesh to determine size
            float planeWidth = meshRenderer.bounds.size.x;
            float planeHeight = meshRenderer.bounds.size.z; // Usually the Z-axis in a 2D view

            int gridSize = 10; // Assuming a 10x10 grid
            float cellWidth = planeWidth / gridSize;   // Width of each grid cell
            float cellHeight = planeHeight / gridSize; // Height of each grid cell

            // Calculate the center position of the specified grid cell
            float centerX = (gridPosition.x * cellWidth) + (cellWidth / 2) - (planeWidth / 2);
            float centerZ = (gridPosition.y * cellHeight) + (cellHeight / 2) - (planeHeight / 2);

            // Set Y to the height of the plane
            float centerY = obj.transform.position.y; // Assuming the plane's Y position is what you want

            // Return the center position in world space
            return new Vector3(centerX, centerY, centerZ);
        }
    }
}