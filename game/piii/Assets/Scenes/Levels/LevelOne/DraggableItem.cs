using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    // --- SETTINGS ---
    public enum PlacementType { FloorOnly, WallOnly, Any }

    [Header("Item Type")]
    public PlacementType placementType = PlacementType.FloorOnly;

    [Header("Grid Settings")]
    public float gridSize = 1.0f;
    public float floorLift = 1.0f;

    // We calculate these automatically now
    private Vector3 calculatedOffset;
    private float dynamicWallOffset;

    // --- INTERNAL VARIABLES ---
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool isDragging = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Camera cam;
    private Rigidbody rb;

    // Track what surface is UNDER the item
    private GameObject currentSurface;

    void Start()
    {
        cam = Camera.main;
        if (cam == null) Debug.LogError("DraggableItem: Camera.main is null!");

        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Auto-Calculate Grid Offset
        int sizeX = Mathf.RoundToInt(transform.localScale.x);
        int sizeY = Mathf.RoundToInt(transform.localScale.y);
        int sizeZ = Mathf.RoundToInt(transform.localScale.z);

        calculatedOffset = new Vector3(
            (sizeX % 2 != 0) ? 0.5f : 0.0f,
            (sizeY % 2 != 0) ? 0.5f : 0.0f,
            (sizeZ % 2 != 0) ? 0.5f : 0.0f
        );

        dynamicWallOffset = transform.localScale.z / 2.0f;

        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void OnMouseDown()
    {
        isDragging = true;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void OnMouseUp()
    {
        isDragging = false;

        if (IsValidPlacement())
        {
            if (currentSurface == null)
            {
                // Dropped in void -> Return to spawn
                transform.position = initialPosition;
                transform.rotation = initialRotation;
            }
            else
            {
                // Valid placement -> Save state
                originalPosition = transform.position;
                originalRotation = transform.rotation;
            }
        }
        else
        {
            Debug.Log("Invalid Placement! Returning...");
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            MoveAndAlign();

            if (currentSurface != null && currentSurface.CompareTag("Floor"))
            {
                if (Input.GetMouseButtonDown(1))
                {
                    transform.Rotate(0, 90, 0);
                }
            }
        }
    }

    void MoveAndAlign()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        bool surfaceFound = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;

            if (hit.collider.CompareTag("Wall"))
            {
                // FIX: Ignore the top/bottom faces of the wall
                // If the normal points Up (1) or Down (-1), it's not a valid side face.
                if (Mathf.Abs(hit.normal.y) > 0.1f) continue;

                currentSurface = hit.collider.gameObject;
                surfaceFound = true;

                Vector3 targetPos = hit.point + (hit.normal * dynamicWallOffset);
                transform.rotation = Quaternion.LookRotation(hit.normal);

                Vector3 snappedPos = SnapToWallGrid(targetPos, hit.normal);

                // Keep the existing Clamp logic to stop it sliding off the sides/top
                transform.position = ClampToSurfaceBounds(snappedPos, currentSurface);
                break;
            }
            else if (hit.collider.CompareTag("Floor"))
            {
                currentSurface = hit.collider.gameObject;
                surfaceFound = true;

                Vector3 currentOffset = GetRotatedFloorOffset();
                float x = (Mathf.Floor(hit.point.x / gridSize) * gridSize) + currentOffset.x;
                float z = (Mathf.Floor(hit.point.z / gridSize) * gridSize) + currentOffset.z;

                Vector3 targetPos = new Vector3(x, hit.point.y + floorLift, z);
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

                transform.position = ClampToSurfaceBounds(targetPos, currentSurface);
                break;
            }
        }

        if (!surfaceFound)
        {
            currentSurface = null;
            transform.position = GetMouseWorldPos();
        }
    }

    // --- HELPER LOGIC ---

    bool IsValidPlacement()
    {
        if (currentSurface == null) return true; // Handle void drop in OnMouseUp

        bool onFloor = currentSurface.CompareTag("Floor");
        bool onWall = currentSurface.CompareTag("Wall");

        if (placementType == PlacementType.FloorOnly && !onFloor) return false;
        if (placementType == PlacementType.WallOnly && !onWall) return false;

        if (CheckForOverlap()) return false;

        // NEW: Check if object is strictly inside the bounds of the floor/wall
        if (!IsInsideSurfaceBounds()) return false;

        return true;
    }

    bool IsInsideSurfaceBounds()
    {
        if (currentSurface == null) return true;

        Collider itemCol = GetComponent<Collider>();
        Collider surfCol = currentSurface.GetComponent<Collider>();

        if (!itemCol || !surfCol) return true;

        Bounds iBounds = itemCol.bounds;
        Bounds sBounds = surfCol.bounds;

        // Small tolerance to handle floating point imprecision
        float tolerance = 0.05f;

        if (currentSurface.CompareTag("Floor"))
        {
            // For Floor, we check if the item's footprint (X and Z) is inside the floor
            // We ignore Y because the item sits *on top* of the floor
            bool insideX = iBounds.min.x >= sBounds.min.x - tolerance && iBounds.max.x <= sBounds.max.x + tolerance;
            bool insideZ = iBounds.min.z >= sBounds.min.z - tolerance && iBounds.max.z <= sBounds.max.z + tolerance;

            return insideX && insideZ;
        }
        else if (currentSurface.CompareTag("Wall"))
        {
            // For Walls, first check Height (Y)
            bool insideY = iBounds.min.y >= sBounds.min.y - tolerance && iBounds.max.y <= sBounds.max.y + tolerance;
            if (!insideY) return false;

            // Then check Width based on wall orientation
            // If Wall is wider along X, it's a Back/Front wall -> Check X
            if (sBounds.size.x > sBounds.size.z)
            {
                bool insideX = iBounds.min.x >= sBounds.min.x - tolerance && iBounds.max.x <= sBounds.max.x + tolerance;
                return insideX;
            }
            else
            {
                // Wall is wider along Z, it's a Side wall -> Check Z
                bool insideZ = iBounds.min.z >= sBounds.min.z - tolerance && iBounds.max.z <= sBounds.max.z + tolerance;
                return insideZ;
            }
        }

        return true;
    }

    bool CheckForOverlap()
    {
        Vector3 size = transform.localScale * 0.9f;
        Collider[] hits = Physics.OverlapBox(transform.position, size / 2, transform.rotation);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject && hit.gameObject != currentSurface)
            {
                return true;
            }
        }
        return false;
    }

    Vector3 SnapToWallGrid(Vector3 rawPos, Vector3 normal)
    {
        float xOffset = calculatedOffset.x;
        float yOffset = calculatedOffset.y;

        if (Mathf.Abs(normal.z) > 0.5f)
        {
            float x = (Mathf.Floor(rawPos.x / gridSize) * gridSize) + xOffset;
            float y = (Mathf.Floor(rawPos.y / gridSize) * gridSize) + yOffset;
            return new Vector3(x, y, rawPos.z);
        }
        else
        {
            float z = (Mathf.Floor(rawPos.z / gridSize) * gridSize) + xOffset;
            float y = (Mathf.Floor(rawPos.y / gridSize) * gridSize) + yOffset;
            return new Vector3(rawPos.x, y, z);
        }
    }

    Vector3 GetRotatedFloorOffset()
    {
        float currentX = calculatedOffset.x;
        float currentZ = calculatedOffset.z;

        float angle = transform.eulerAngles.y;
        angle = angle % 360f;
        if (angle < 0) angle += 360f;

        if (Mathf.Abs(angle - 90f) < 1f || Mathf.Abs(angle - 270f) < 1f)
        {
            currentX = calculatedOffset.z;
            currentZ = calculatedOffset.x;
        }
        return new Vector3(currentX, 0, currentZ);
    }

    private Vector3 GetMouseWorldPos()
    {
        if (cam == null) return transform.position;
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance)) return ray.GetPoint(distance);
        return transform.position;
    }

    Vector3 ClampToSurfaceBounds(Vector3 targetPos, GameObject surface)
    {
        Collider surfaceCol = surface.GetComponent<Collider>();
        if (surfaceCol == null) return targetPos;

        Bounds sBounds = surfaceCol.bounds;

        // Calculate half-size of the item to keep it fully inside
        Vector3 itemHalfSize = transform.localScale * 0.5f;

        // Swap dimensions if rotated 90 degrees
        if (Mathf.Abs(transform.forward.x) > 0.5f)
        {
            float temp = itemHalfSize.x;
            itemHalfSize.x = itemHalfSize.z;
            itemHalfSize.z = temp;
        }

        // Define valid range
        float minX = sBounds.min.x + itemHalfSize.x;
        float maxX = sBounds.max.x - itemHalfSize.x;
        float minY = sBounds.min.y + itemHalfSize.y;
        float maxY = sBounds.max.y - itemHalfSize.y;
        float minZ = sBounds.min.z + itemHalfSize.z;
        float maxZ = sBounds.max.z - itemHalfSize.z;

        if (surface.CompareTag("Floor"))
        {
            // Floor: Clamp X and Z
            float clampedX = (minX > maxX) ? sBounds.center.x : Mathf.Clamp(targetPos.x, minX, maxX);
            float clampedZ = (minZ > maxZ) ? sBounds.center.z : Mathf.Clamp(targetPos.z, minZ, maxZ);
            return new Vector3(clampedX, targetPos.y, clampedZ);
        }
        else // Wall
        {
            // Wall: Always Clamp Y (Height) - This stops it going too high!
            float clampedY = (minY > maxY) ? sBounds.center.y : Mathf.Clamp(targetPos.y, minY, maxY);

            // Clamp Width based on orientation
            if (sBounds.size.x > sBounds.size.z) // Back/Front Wall
            {
                float clampedX = (minX > maxX) ? sBounds.center.x : Mathf.Clamp(targetPos.x, minX, maxX);
                return new Vector3(clampedX, clampedY, targetPos.z);
            }
            else // Side Wall
            {
                float clampedZ = (minZ > maxZ) ? sBounds.center.z : Mathf.Clamp(targetPos.z, minZ, maxZ);
                return new Vector3(targetPos.x, clampedY, clampedZ);
            }
        }
    }
}