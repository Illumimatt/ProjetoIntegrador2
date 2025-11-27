using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    [Header("Grid Settings")]
    public float gridSize = 1.0f; // Set this to match your floor tile size
    public float liftHeight = 0.5f; // How high it lifts when dragging

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalPosition;
    private float targetY; // The height where the object should sit
    private Camera cam;

    // Physics variables
    private Rigidbody rb;
    private BoxCollider col;

    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<BoxCollider>();

        // Disable gravity so it doesn't fall through the floor while we drag
        rb.useGravity = false;
        rb.isKinematic = true; // We control movement manually

        targetY = transform.position.y; // Remember floor height
    }

    void OnMouseDown()
    {
        isDragging = true;
        originalPosition = transform.position;

        // 1. Calculate offset so we don't snap to center
        offset = transform.position - GetMouseWorldPos();

        // 2. Visual "Pop": Lift the object up
        transform.position = new Vector3(transform.position.x, targetY + liftHeight, transform.position.z);
    }

    void OnMouseUp()
    {
        isDragging = false;

        // 1. Drop it (Return to normal height)
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

        // 2. Snap to Grid
        SnapPosition();

        // 3. Check for Collisions (Prevent stacking)
        if (CheckForOverlap())
        {
            Debug.Log("Can't place here!");
            transform.position = originalPosition; // Return to start
        }
    }

    void Update()
    {
        if (isDragging)
        {
            // Move object with mouse
            Vector3 mousePos = GetMouseWorldPos();
            transform.position = new Vector3(mousePos.x + offset.x, targetY + liftHeight, mousePos.z + offset.z);

            // ROTATION (Right Click)
            if (Input.GetMouseButtonDown(1)) // 0 is Left, 1 is Right
            {
                transform.Rotate(0, 90, 0); // Rotate 90 degrees on Y axis
            }
        }
    }

    // Helper to get mouse position on the 3D plane
    private Vector3 GetMouseWorldPos()
    {
        // Create an invisible plane at the object's height
        Plane plane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return transform.position;
    }

    void SnapPosition()
    {
        // Round X and Z to nearest grid size
        float x = Mathf.Round(transform.position.x / gridSize) * gridSize;
        float z = Mathf.Round(transform.position.z / gridSize) * gridSize;

        transform.position = new Vector3(x, targetY, z);
    }

    bool CheckForOverlap()
    {
        // Make the detection box slightly smaller than the object to avoid edge-touching
        Vector3 size = transform.localScale * 0.9f;

        Collider[] hits = Physics.OverlapBox(transform.position, size / 2, transform.rotation);

        foreach (Collider hit in hits)
        {
            // If we hit something that is NOT the floor and NOT ourselves
            if (hit.gameObject != gameObject && !hit.CompareTag("Floor"))
            {
                return true; // Overlap detected
            }
        }
        return false;
    }
}