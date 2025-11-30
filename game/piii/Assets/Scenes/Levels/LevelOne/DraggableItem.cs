using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

public class DraggableItem : MonoBehaviour
{
    // --- SETTINGS ---
    public enum PlacementType { FloorOnly, WallOnly, Any }

    [Header("Item Type")]
    public PlacementType placementType = PlacementType.FloorOnly;

    [Header("Grid Settings")]
    public float gridSize = 1.0f;
    public float floorLift = 0.5f;

    [Header("Room Logic")]
    public Vector3 roomCenter = Vector3.zero;
    public bool IsPlaced { get; private set; } = false;

    // --- VISUALS ---
    private MeshRenderer meshRenderer;
    private Color originalColor;
    private int originalRenderQueue; // To remember the default sorting

    // --- AUTO-CALCULATED ---
    private Vector3 calculatedOffset;
    private float dynamicWallOffset;

    // --- INTERNAL ---
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Camera cam;
    private Rigidbody rb;
    private GameObject currentSurface;

    void Start()
    {
        cam = Camera.main;
        if (cam == null) Debug.LogError("DraggableItem: Camera.main is null!");

        rb = GetComponent<Rigidbody>();
        if (rb) { rb.useGravity = false; rb.isKinematic = true; }

        // VISUAL SETUP
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
            // Store the original render queue (usually 3000 for Transparent)
            originalRenderQueue = meshRenderer.material.renderQueue;
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
        IsPlaced = false;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void OnMouseUp()
    {
        isDragging = false;

        // RESET VISUALS ON DROP
        if (meshRenderer != null)
        {
            meshRenderer.material.color = originalColor;
            meshRenderer.material.renderQueue = originalRenderQueue; // Reset sorting
        }

        MoveAndAlign(true);

        if (IsValidPlacement())
        {
            if (currentSurface == null)
            {
                transform.position = initialPosition;
                transform.rotation = initialRotation;
            }
            else
            {
                originalPosition = transform.position;
                originalRotation = transform.rotation;
                IsPlaced = true;
                FindObjectOfType<LevelController>()?.CheckLevelCompletion();
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
            MoveAndAlign(false);

            if (currentSurface != null && currentSurface.CompareTag("Floor"))
            {
                if (Input.GetMouseButtonDown(1)) transform.Rotate(0, 90, 0);
            }

            UpdateVisuals();
        }
    }

    void UpdateVisuals()
    {
        if (meshRenderer == null) return;

        if (IsValidPlacement() || currentSurface == null)
        {
            Color transparentColor = originalColor;
            transparentColor.a = 0.5f;
            meshRenderer.material.color = transparentColor;
            meshRenderer.material.renderQueue = originalRenderQueue;
        }
        else
        {
            Color invalidColor = Color.red;
            invalidColor.a = 0.5f;
            meshRenderer.material.color = invalidColor;
            meshRenderer.material.renderQueue = ((int) RenderQueue.Geometry) + 2;
        }
    }

    void MoveAndAlign(bool isPlacing)
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        bool surfaceFound = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;

            if (placementType == PlacementType.FloorOnly && hit.collider.CompareTag("Wall")) continue;
            if (placementType == PlacementType.WallOnly && hit.collider.CompareTag("Floor")) continue;

            if (hit.collider.CompareTag("Wall"))
            {
                if (Mathf.Abs(hit.normal.y) > 0.1f) continue;

                Vector3 directionToRoom = roomCenter - hit.point;
                if (Vector3.Dot(hit.normal, directionToRoom) < 0) continue;

                currentSurface = hit.collider.gameObject;
                surfaceFound = true;

                Vector3 targetPos = hit.point + (hit.normal * dynamicWallOffset);
                transform.rotation = Quaternion.LookRotation(hit.normal);

                Vector3 snappedPos = SnapToWallGrid(targetPos, hit.normal);
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
        if (currentSurface == null) return true;

        bool onFloor = currentSurface.CompareTag("Floor");
        bool onWall = currentSurface.CompareTag("Wall");

        if (placementType == PlacementType.FloorOnly && !onFloor) return false;
        if (placementType == PlacementType.WallOnly && !onWall) return false;

        if (CheckForOverlap()) return false;
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
        float tolerance = 0.05f;

        if (currentSurface.CompareTag("Floor"))
        {
            return iBounds.min.x >= sBounds.min.x - tolerance && iBounds.max.x <= sBounds.max.x + tolerance &&
                   iBounds.min.z >= sBounds.min.z - tolerance && iBounds.max.z <= sBounds.max.z + tolerance;
        }
        else // Wall
        {
            if (iBounds.min.y < sBounds.min.y - tolerance || iBounds.max.y > sBounds.max.y + tolerance) return false;

            if (sBounds.size.x > sBounds.size.z)
                return iBounds.min.x >= sBounds.min.x - tolerance && iBounds.max.x <= sBounds.max.x + tolerance;
            else
                return iBounds.min.z >= sBounds.min.z - tolerance && iBounds.max.z <= sBounds.max.z + tolerance;
        }
    }

    bool CheckForOverlap()
    {
        Vector3 size = transform.localScale * 0.9f;
        Collider[] hits = Physics.OverlapBox(transform.position, size / 2, transform.rotation);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject && hit.gameObject != currentSurface) return true;
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
        float cx = calculatedOffset.x; float cz = calculatedOffset.z;
        int angle = Mathf.RoundToInt(transform.eulerAngles.y);
        angle = angle % 360; if (angle < 0) angle += 360;

        if (Mathf.Abs(angle - 90) < 1 || Mathf.Abs(angle - 270) < 1) { cx = calculatedOffset.z; cz = calculatedOffset.x; }
        return new Vector3(cx, 0, cz);
    }

    private Vector3 GetMouseWorldPos()
    {
        if (cam == null) return transform.position;
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float d; if (plane.Raycast(ray, out d)) return ray.GetPoint(d);
        return transform.position;
    }

    Vector3 ClampToSurfaceBounds(Vector3 targetPos, GameObject surface)
    {
        Collider surfaceCol = surface.GetComponent<Collider>();
        if (surfaceCol == null) return targetPos;

        Bounds sBounds = surfaceCol.bounds;
        Vector3 itemHalfSize = transform.localScale * 0.5f;

        if (Mathf.Abs(transform.forward.x) > 0.5f)
        {
            float temp = itemHalfSize.x;
            itemHalfSize.x = itemHalfSize.z;
            itemHalfSize.z = temp;
        }

        float minX = sBounds.min.x + itemHalfSize.x;
        float maxX = sBounds.max.x - itemHalfSize.x;
        float minY = sBounds.min.y + itemHalfSize.y;
        float maxY = sBounds.max.y - itemHalfSize.y;
        float minZ = sBounds.min.z + itemHalfSize.z;
        float maxZ = sBounds.max.z - itemHalfSize.z;

        if (surface.CompareTag("Floor"))
        {
            float clampedX = (minX > maxX) ? sBounds.center.x : Mathf.Clamp(targetPos.x, minX, maxX);
            float clampedZ = (minZ > maxZ) ? sBounds.center.z : Mathf.Clamp(targetPos.z, minZ, maxZ);
            return new Vector3(clampedX, targetPos.y, clampedZ);
        }
        else
        {
            float clampedY = (minY > maxY) ? sBounds.center.y : Mathf.Clamp(targetPos.y, minY, maxY);
            if (sBounds.size.x > sBounds.size.z)
                return new Vector3(Mathf.Clamp(targetPos.x, minX, maxX), clampedY, targetPos.z);
            else
                return new Vector3(targetPos.x, clampedY, Mathf.Clamp(targetPos.z, minZ, maxZ));
        }
    }
}