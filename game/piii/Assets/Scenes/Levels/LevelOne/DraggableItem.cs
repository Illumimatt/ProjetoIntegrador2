using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    [Header("Settings")]
    public float gridSize = 0.5f; // How big each "tile" is
    public bool snapToGrid = true;

    private bool isDragging = false;
    private Vector3 offset;
    private SpriteRenderer spriteRenderer;
    private int originalOrder;
    private Vector3 startPosition;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        // 1. Pick up
        isDragging = true;
        startPosition = transform.position; // Remember where we were in case the move is invalid

        // Calculate offset so the item doesn't snap weirdly to the center of the mouse
        offset = transform.position - GetMouseWorldPos();

        // Visual feedback: Pop item to the front while holding
        originalOrder = spriteRenderer.sortingOrder;
        spriteRenderer.sortingOrder = 100;
    }

    void OnMouseUp()
    {
        // 2. Drop
        isDragging = false;

        if (snapToGrid)
        {
            SnapPosition();
        }

        // Restore layer order
        spriteRenderer.sortingOrder = originalOrder;

        // 3. Validation
        if (IsOverlapping())
        {
            Debug.Log("Space Occupied! Returning to start.");
            transform.position = startPosition; // Bounce back
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 targetPos = GetMouseWorldPos() + offset;
            transform.position = targetPos;
        }
    }

    void SnapPosition()
    {
        // Round position to nearest Grid Size
        float x = Mathf.Round(transform.position.x / gridSize) * gridSize;
        float y = Mathf.Round(transform.position.y / gridSize) * gridSize;

        transform.position = new Vector3(x, y, 0);
    }

    // Check if we dropped on top of another Collider
    bool IsOverlapping()
    {
        // Create a hidden box slightly smaller than our grid size to check for hits
        Collider2D hit = Physics2D.OverlapBox(transform.position, Vector2.one * (gridSize * 0.9f), 0);

        // If we hit something that is NOT ourselves
        if (hit != null && hit.gameObject != this.gameObject)
        {
            return true;
        }
        return false;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 10.0f; // Distance from camera
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    // Draw grid lines in the Editor so you can see them (Debugging)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (float x = -10; x < 10; x += gridSize)
            Gizmos.DrawLine(new Vector3(x, -10, 0), new Vector3(x, 10, 0));
        for (float y = -10; y < 10; y += gridSize)
            Gizmos.DrawLine(new Vector3(-10, y, 0), new Vector3(10, y, 0));
    }
}