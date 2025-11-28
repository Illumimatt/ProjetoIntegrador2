using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    // --- SETTINGS ---
    public enum PlacementType { FloorOnly, WallOnly, Any }

    [Header("Item Type")]
    public PlacementType placementType = PlacementType.FloorOnly;

    [Header("Grid Settings")]
    public float gridSize = 1.0f;
    public float floorLift = 0.5f;

    // We calculate these automatically now (No manual setting needed)
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
        if (cam == null)
        {
            Debug.LogError("DraggableItem: Camera.main is null! Please ensure a camera is tagged as MainCamera.");
        }

        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

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
            if (currentSurface == null) {
                transform.position = initialPosition;
                transform.rotation = initialRotation;
            } else {
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

        // Sort hits by distance so we process the closest surface first
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        bool surfaceFound = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;

            if (hit.collider.CompareTag("Wall"))
            {
                currentSurface = hit.collider.gameObject;
                surfaceFound = true;

                Vector3 targetPos = hit.point + (hit.normal * dynamicWallOffset);
                transform.rotation = Quaternion.LookRotation(hit.normal);
                transform.position = SnapToWallGrid(targetPos, hit.normal);
                break;
            }
            else if (hit.collider.CompareTag("Floor"))
            {
                currentSurface = hit.collider.gameObject;
                surfaceFound = true;

                Vector3 currentOffset = GetRotatedFloorOffset();

                float x = (Mathf.Floor(hit.point.x / gridSize) * gridSize) + currentOffset.x;
                float z = (Mathf.Floor(hit.point.z / gridSize) * gridSize) + currentOffset.z;

                transform.position = new Vector3(x, hit.point.y + floorLift, z);

                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
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

    Vector3 SnapToWallGrid(Vector3 rawPos, Vector3 normal)
    {
        float xOffset = calculatedOffset.x;
        float yOffset = calculatedOffset.y;

        // Z-Axis Wall (Front/Back)
        if (Mathf.Abs(normal.z) > 0.5f)
        {
            float x = (Mathf.Floor(rawPos.x / gridSize) * gridSize) + xOffset;
            float y = (Mathf.Floor(rawPos.y / gridSize) * gridSize) + yOffset;
            return new Vector3(x, y, rawPos.z);
        }
        // X-Axis Wall (Sides)
        else
        {
            // On side walls, Global Z acts as the item's Width
            float z = (Mathf.Floor(rawPos.z / gridSize) * gridSize) + xOffset;
            float y = (Mathf.Floor(rawPos.y / gridSize) * gridSize) + yOffset;
            return new Vector3(rawPos.x, y, z);
        }
    }

    Vector3 GetRotatedFloorOffset()
    {
        // Swap offsets if rotated 90 or 270 degrees
        float currentX = calculatedOffset.x;
        float currentZ = calculatedOffset.z;

        // Normalize angle to 0-360 range and check if rotated 90 or 270 degrees
        float angle = transform.eulerAngles.y;
        angle = angle % 360f;
        if (angle < 0) angle += 360f;

        // Check if angle is approximately 90 or 270 degrees (with small tolerance)
        if (Mathf.Abs(angle - 90f) < 1f || Mathf.Abs(angle - 270f) < 1f)
        {
            currentX = calculatedOffset.z;
            currentZ = calculatedOffset.x;
        }
        return new Vector3(currentX, 0, currentZ);
    }

    bool IsValidPlacement()
    {
        if (currentSurface == null) return true;

        bool onFloor = currentSurface.CompareTag("Floor");
        bool onWall = currentSurface.CompareTag("Wall");

        if (placementType == PlacementType.FloorOnly && !onFloor) return false;
        if (placementType == PlacementType.WallOnly && !onWall) return false;

        if (CheckForOverlap()) return false;

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

    private Vector3 GetMouseWorldPos()
    {
        if (cam == null) return transform.position;

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance)) return ray.GetPoint(distance);
        return transform.position;
    }
}