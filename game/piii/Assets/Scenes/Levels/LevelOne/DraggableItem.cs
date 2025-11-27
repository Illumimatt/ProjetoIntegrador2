using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    // --- SETTINGS ---
    public enum PlacementType { FloorOnly, WallOnly, Any }
    
    [Header("Item Type")]
    public PlacementType placementType = PlacementType.FloorOnly; // Set this in Inspector!

    [Header("Grid Settings")]
    public float gridSize = 1.0f;
    public float floorLift = 0.5f;     // Height when on floor
    public float wallOffset = 1;    // Distance sticking out of wall
    public Vector3 snapOffset = new Vector3(0.5f, 0, 0.5f); 

    // --- INTERNAL VARIABLES ---
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation; // To remember rotation if we snap back
    private Camera cam;
    private Rigidbody rb;

    // Track what we are currently hovering over
    private GameObject currentSurface; 

    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
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

        // 1. Check if the placement is valid
        if (IsValidPlacement())
        {
            // Valid! Save this spot.
            originalPosition = transform.position;
            originalRotation = transform.rotation;
        }
        else
        {
            // Invalid! Snap back to start.
            Debug.Log("Invalid Surface! Returning...");
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            MoveAndAlign();

            // Allow rotation ONLY if we are on the floor (Wall items usually rely on wall normal)
            if (currentSurface != null && currentSurface.CompareTag("Floor"))
            {
                if (Input.GetMouseButtonDown(1)) // Right Click
                {
                    transform.Rotate(0, 90, 0);
                }
            }
        }
    }

    // --- CORE MOVEMENT LOGIC ---
    void MoveAndAlign()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Raycast against everything (Floor and Walls)
        if (Physics.Raycast(ray, out hit))
        {
            currentSurface = hit.collider.gameObject;

            // CASE A: We hit a WALL
            if (hit.collider.CompareTag("Wall"))
            {
                // 1. Position: Stick to the wall point + offset
                Vector3 targetPos = hit.point + (hit.normal * wallOffset);
                
                // 2. Snap: We need to figure out which axes to snap based on the wall direction
                targetPos = SnapToWallGrid(targetPos, hit.normal);
                transform.position = targetPos;

                // 3. Rotation: Face away from the wall
                transform.rotation = Quaternion.LookRotation(hit.normal);
            }
            // CASE B: We hit a FLOOR
            else if (hit.collider.CompareTag("Floor"))
            {
                // 1. Position: Move flat on X/Z
                float x = (Mathf.Floor(hit.point.x / gridSize) * gridSize) + GetCurrentOffset().x;
                float z = (Mathf.Floor(hit.point.z / gridSize) * gridSize) + GetCurrentOffset().z;
                
                transform.position = new Vector3(x, hit.point.y + floorLift, z);

                // 2. Rotation: Reset X/Z tilt, keep Y rotation (user rotation)
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            }
        }
        else
        {
            currentSurface = null; // Dragging in void
            transform.position = GetMouseWorldPos();
        }
    }

    // --- HELPER FUNCTIONS ---

    private Vector3 GetMouseWorldPos()
    {
        Plane plane = new Plane(Vector3.up, new Vector3(0, 0, 0));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance)) return ray.GetPoint(distance);
        return transform.position;
    }

    Vector3 SnapToWallGrid(Vector3 rawPos, Vector3 normal)
    {
        // If normal is Z (facing Forward/Back), we snap X and Y
        if (Mathf.Abs(normal.z) > 0.5f)
        {
            float x = Mathf.Round(rawPos.x / gridSize) * gridSize;
            float y = Mathf.Round(rawPos.y / gridSize) * gridSize;
            return new Vector3(x, y, rawPos.z);
        }
        // If normal is X (facing Left/Right), we snap Z and Y
        else 
        {
            float z = Mathf.Round(rawPos.z / gridSize) * gridSize;
            float y = Mathf.Round(rawPos.y / gridSize) * gridSize;
            return new Vector3(rawPos.x, y, z);
        }
    }

    bool IsValidPlacement()
    {
        if (currentSurface == null) return false;

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

    Vector3 GetCurrentOffset()
    {
        // Handles the rotation offset swapping for 2x1 items on floor
        float currentX = snapOffset.x;
        float currentZ = snapOffset.z;
        int angle = Mathf.RoundToInt(transform.eulerAngles.y);
        if (angle % 180 != 0) 
        {
            currentX = snapOffset.z;
            currentZ = snapOffset.x;
        }
        return new Vector3(currentX, 0, currentZ);
    }
}