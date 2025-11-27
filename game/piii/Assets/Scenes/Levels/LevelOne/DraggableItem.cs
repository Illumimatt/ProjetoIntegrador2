using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    [Header("Grid Settings")]
    public float gridSize = 1.0f;
    public float liftHeight = 0.5f;

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalPosition;
    private float targetY;
    private Camera cam;
    private Rigidbody rb;

    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;

        targetY = transform.position.y;
    }

    void OnMouseDown()
    {
        isDragging = true;
        originalPosition = transform.position; // Remember where we started

        offset = transform.position - GetMouseWorldPos();

        // Lift up visually
        transform.position = new Vector3(transform.position.x, targetY + liftHeight, transform.position.z);
    }

    void OnMouseUp()
    {
        isDragging = false;

        // 1. Drop down
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

        // 2. Snap to Grid
        SnapPosition();

        // 3. VALIDATION: Must be Over Floor AND Not overlapping obstacles
        bool isSafe = IsOverFloor() && !CheckForOverlap();

        if (isSafe)
        {
            // Valid placement! Update the "original position" to this new spot
            originalPosition = transform.position;
        }
        else
        {
            // Invalid! Go back to start
            Debug.Log("Invalid Placement! Returning...");
            transform.position = originalPosition;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePos = GetMouseWorldPos();
            transform.position = new Vector3(mousePos.x + offset.x, targetY + liftHeight, mousePos.z + offset.z);

            if (Input.GetMouseButtonDown(1))
            {
                transform.Rotate(0, 90, 0);
            }
        }
    }

    bool IsOverFloor()
    {
        // Cast a ray from the center of the object DOWNWARDS
        // We check 2 units down just to be safe
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.0f))
        {
            // If we hit something tagged "Floor", we are safe
            if (hit.collider.CompareTag("Floor"))
            {
                return true;
            }
        }
        return false; // We hit nothing (void) or something that isn't floor
    }

    bool CheckForOverlap()
    {
        // Box is slightly smaller (0.9) to avoid accidental edge hits
        Vector3 size = transform.localScale * 0.9f;

        Collider[] hits = Physics.OverlapBox(transform.position, size / 2, transform.rotation);

        foreach (Collider hit in hits)
        {
            // If we hit something that is NOT us and NOT the floor...
            if (hit.gameObject != gameObject && !hit.CompareTag("Floor"))
            {
                // It's a wall or another item!
                return true;
            }
        }
        return false;
    }

    void SnapPosition()
    {
        float x = Mathf.Round(transform.position.x / gridSize) * gridSize;
        float z = Mathf.Round(transform.position.z / gridSize) * gridSize;
        transform.position = new Vector3(x, targetY, z);
    }

    private Vector3 GetMouseWorldPos()
    {
        Plane plane = new Plane(Vector3.up, new Vector3(0, targetY, 0));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance)) return ray.GetPoint(distance);
        return transform.position;
    }

    // VISUAL DEBUGGING: Draws a grid in the Scene and Game view
    void OnDrawGizmos()
    {
        // 1. Pick a color (Yellow is easy to see against blue/dark floors)
        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.5f);

        // 2. Define how big of an area you want to see
        float roomSize = 10.0f;
        float floorHeight = 0.01f; // Slightly above 0 so it doesn't clip with the floor

        // 3. Draw Lines along the Z-axis (Vertical)
        for (float x = -roomSize; x <= roomSize; x += gridSize)
        {
            Gizmos.DrawLine(new Vector3(x, floorHeight, -roomSize), new Vector3(x, floorHeight, roomSize));
        }

        // 4. Draw Lines along the X-axis (Horizontal)
        for (float z = -roomSize; z <= roomSize; z += gridSize)
        {
            Gizmos.DrawLine(new Vector3(-roomSize, floorHeight, z), new Vector3(roomSize, floorHeight, z));
        }

        // 5. Draw a sphere at the item's "Target Snap Position" to see where it wants to go
        if (isDragging)
        {
            Gizmos.color = Color.red;
            float snapX = Mathf.Round(transform.position.x / gridSize) * gridSize;
            float snapZ = Mathf.Round(transform.position.z / gridSize) * gridSize;
            Gizmos.DrawSphere(new Vector3(snapX, transform.position.y, snapZ), 0.1f);
        }
    }
}