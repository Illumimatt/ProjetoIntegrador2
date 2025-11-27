using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    [Header("Grid Settings")]
    public float gridSize = 1.0f;
    public float liftHeight = 0.5f;

    // NEW: Use 0.5 for odd sizes (1x1, 3x3) to center them in the tile.
    // Use 0.0 for even sizes (2x2) to snap to the lines.
    public Vector3 snapOffset = new Vector3(0.5f, 0, 0.5f);

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
        originalPosition = transform.position;
        offset = transform.position - GetMouseWorldPos();
        transform.position = new Vector3(transform.position.x, targetY + liftHeight, transform.position.z);
    }

    void OnMouseUp()
    {
        isDragging = false;

        // Snap first so we land in the right spot
        SnapPosition();

        // Apply visual drop height
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

        // Check Logic
        bool isSafe = IsOverFloor() && !CheckForOverlap();

        if (isSafe)
        {
            originalPosition = transform.position;
        }
        else
        {
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

    void SnapPosition()
    {
        float currentXOffset = snapOffset.x;
        float currentZOffset = snapOffset.z;

        // Check our rotation (Y angle). 
        // If it is 90 or 270 degrees, we have swapped orientation.
        // We use Mathf.Round because rotation might be 90.0001
        int angle = Mathf.RoundToInt(transform.eulerAngles.y);

        // If angle is 90 or 270 (Odd multiples of 90), we swap dimensions
        if (angle % 180 != 0)
        {
            currentXOffset = snapOffset.z;
            currentZOffset = snapOffset.x;
        }

        // Use the calculated "current" offsets
        float x = (Mathf.Floor(transform.position.x / gridSize) * gridSize) + currentXOffset;
        float z = (Mathf.Floor(transform.position.z / gridSize) * gridSize) + currentZOffset;

        transform.position = new Vector3(x, targetY, z);
    }

    bool IsOverFloor()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.0f))
        {
            if (hit.collider.CompareTag("Floor")) return true;
        }
        return false;
    }

    bool CheckForOverlap()
    {
        Vector3 size = transform.localScale * 0.9f;
        Collider[] hits = Physics.OverlapBox(transform.position, size / 2, transform.rotation);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject && !hit.CompareTag("Floor"))
            {
                return true;
            }
        }
        return false;
    }

    private Vector3 GetMouseWorldPos()
    {
        Plane plane = new Plane(Vector3.up, new Vector3(0, targetY, 0));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float distance;
        if (plane.Raycast(ray, out distance)) return ray.GetPoint(distance);
        return transform.position;
    }

    // --- UPDATED GIZMOS ---
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.5f);
        float roomSize = 10.0f;
        float floorHeight = 0.01f;

        // Draw the Grid Lines
        for (float x = -roomSize; x <= roomSize; x += gridSize)
            Gizmos.DrawLine(new Vector3(x, floorHeight, -roomSize), new Vector3(x, floorHeight, roomSize));

        for (float z = -roomSize; z <= roomSize; z += gridSize)
            Gizmos.DrawLine(new Vector3(-roomSize, floorHeight, z), new Vector3(roomSize, floorHeight, z));

        // Draw the Snap Target (Accounting for Offset)
        if (isDragging)
        {
            Gizmos.color = Color.red;
            float snapX = (Mathf.Floor(transform.position.x / gridSize) * gridSize) + snapOffset.x;
            float snapZ = (Mathf.Floor(transform.position.z / gridSize) * gridSize) + snapOffset.z;

            Gizmos.DrawSphere(new Vector3(snapX, transform.position.y, snapZ), 0.1f);
        }
    }
}