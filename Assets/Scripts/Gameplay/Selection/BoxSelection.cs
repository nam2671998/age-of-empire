using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

/// <summary>
/// Handles bound selection (horizontal rectangle) by dragging the mouse to select multiple GameObjects
/// </summary>
public class BoxSelection : MonoBehaviour
{
    [SerializeField] private Faction selectableFaction = Faction.Player1;
    
    [Header("Selection Settings")]
    [SerializeField] private Camera selectionCamera;
    [SerializeField] private LayerMask selectableLayerMask = -1; // All layers by default
    [SerializeField] private KeyCode selectionKey = KeyCode.Mouse0; // Left mouse button
    [SerializeField] private float clickSelectRadius = 1f;
    [SerializeField] private float clickDragThresholdPixels = 10f;
    [SerializeField] private int maxDragSelection = 20;
    
    [Header("Selection Box Visual")]
    [SerializeField] private SpriteRenderer selectionBoxRenderer;
    [SerializeField] private float selectionBoxHeight = 0f; // Y position of the horizontal selection plane
    
    private Vector3 selectionStartPosition;
    private Vector3 selectionEndPosition;
    private Vector3 worldStartCorner; // World position of the first corner (where mouse was pressed)
    private bool isSelecting = false;
    private bool isDragging = false;
    private List<IGameSelectable> selectedObjects = new List<IGameSelectable>();
    
    void Start()
    {
        // Find camera if not assigned
        if (selectionCamera == null)
        {
            selectionCamera = Camera.main;
        }
        
        // Create selection box material if renderer exists
        if (selectionBoxRenderer != null)
        {
            SetupSelectionBox();
        }
    }
    
    void LateUpdate()
    {
        HandleSelectionInput();
        UpdateSelectionBox();
    }
    
    private void HandleSelectionInput()
    {
        if (IsPointerOverUI())
        {
            return;
        }

        // Start selection
        if (Input.GetKeyDown(selectionKey))
        {
            StartSelection();
        }
        
        // Update selection while dragging
        if (isSelecting && Input.GetKey(selectionKey))
        {
            UpdateSelection();
        }
        
        // End selection
        if (isSelecting && Input.GetKeyUp(selectionKey))
        {
            EndSelection();
        }
    }
    
    private void StartSelection()
    {
        isSelecting = true;
        isDragging = false;
        selectionStartPosition = Input.mousePosition;
        selectionEndPosition = selectionStartPosition;
        
        // Convert the start position to world space on the horizontal plane (this is our fixed corner)
        Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, selectionBoxHeight, 0));
        Ray startRay = selectionCamera.ScreenPointToRay(selectionStartPosition);
        worldStartCorner = GetRayPlaneIntersection(startRay, horizontalPlane);
        worldStartCorner.y = 0.01f; // Set the Y position
        
        // Deselect all previous selections
        DeselectAll();
    }
    
    private void UpdateSelection()
    {
        selectionEndPosition = Input.mousePosition;

        if (!isDragging && Vector3.Distance(selectionStartPosition, selectionEndPosition) > clickDragThresholdPixels)
        {
            isDragging = true;
        }
    }
    
    private void EndSelection()
    {
        isSelecting = false;
        
        if (isDragging)
        {
            SelectObjectsInBox();
        }
        else
        {
            SelectNearestObjectAtClick();
        }
        isDragging = false;
        
        // Hide selection box
        if (selectionBoxRenderer != null)
        {
            selectionBoxRenderer.gameObject.SetActive(false);
        }
    }
    
    private void UpdateSelectionBox()
    {
        if (!isSelecting || !isDragging || selectionBoxRenderer == null)
        {
            return;
        }
        
        // Show and update selection box
        selectionBoxRenderer.gameObject.SetActive(true);
        float boxSelectionHeight = 2;
        
        // Convert current mouse position to world space on the horizontal plane
        Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, boxSelectionHeight, 0));
        Ray startRay = selectionCamera.ScreenPointToRay(selectionStartPosition);
        Vector3 worldStartCorner = GetRayPlaneIntersection(startRay, horizontalPlane);
        Ray endRay = selectionCamera.ScreenPointToRay(selectionEndPosition);
        Vector3 worldEndCorner = GetRayPlaneIntersection(endRay, horizontalPlane);
        worldEndCorner.y = 0.01f; // Set the Y position
        
        // Calculate width and height in world space (on the horizontal plane)
        // Width is the distance along X axis, height is the distance along Z axis
        float width = Mathf.Abs(worldEndCorner.x - worldStartCorner.x);
        float height = Mathf.Abs(worldEndCorner.z - worldStartCorner.z);
        
        // Calculate the center position, keeping the first corner fixed
        // The sprite pivot is at center (0.5, 0.5), so position = firstCorner + (size * 0.5)
        Vector3 center = worldStartCorner + new Vector3(
            (worldEndCorner.x - worldStartCorner.x) * 0.5f,
            0f,
            (worldEndCorner.z - worldStartCorner.z) * 0.5f
        );
        center.y = boxSelectionHeight;
        
        // Position the rectangle and resize
        selectionBoxRenderer.transform.position = center;
        
        // Resize the sprite using size property
        selectionBoxRenderer.size = new Vector2(width, height);
    }
    
    private Vector3 GetRayPlaneIntersection(Ray ray, Plane plane)
    {
        if (plane.Raycast(ray, out var distance))
        {
            return ray.GetPoint(distance);
        }
        // Fallback: return point at default distance if ray doesn't hit plane
        return ray.GetPoint(selectionBoxHeight);
    }

    private readonly Collider[] results = new Collider[256];
    private void SelectObjectsInBox()
    {
        // Get the current end corner in world space
        Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, selectionBoxHeight, 0));
        Ray endRay = selectionCamera.ScreenPointToRay(selectionEndPosition);
        Vector3 worldEndCorner = GetRayPlaneIntersection(endRay, horizontalPlane);
        
        // Calculate selection bounds in world space (X and Z only)
        float minX = Mathf.Min(worldStartCorner.x, worldEndCorner.x);
        float maxX = Mathf.Max(worldStartCorner.x, worldEndCorner.x);
        float minZ = Mathf.Min(worldStartCorner.z, worldEndCorner.z);
        float maxZ = Mathf.Max(worldStartCorner.z, worldEndCorner.z);

        Vector3 center = new Vector3((minX + maxX) * 0.5f, selectionBoxHeight + 1f, (minZ + maxZ) * 0.5f);
        float halfWidth = Mathf.Max((maxX - minX) * 0.5f, 0.01f);
        float halfDepth = Mathf.Max((maxZ - minZ) * 0.5f, 0.01f);
        Vector3 halfExtents = new Vector3(halfWidth, 100f, halfDepth);

        int hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, results, Quaternion.identity, selectableLayerMask);
        HashSet<IGameSelectable> found = HashSetPool<IGameSelectable>.Get();

        selectedObjects.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            if (selectedObjects.Count >= maxDragSelection)
                break;

            Collider col = results[i];
            if (col == null)
                continue;
            
            IFactionOwner faction = col.GetComponentInParent<IFactionOwner>();
            if (faction == null || faction.Faction != selectableFaction)
                continue;

            IGameSelectable selectable = col.GetComponentInParent<IGameSelectable>();
            if (selectable == null)
                continue;

            if (!selectable.CanSelectFromDrag)
                continue;

            if (!found.Add(selectable))
                continue;

            if (!IsObjectInSelectionBox(selectable, minX, maxX, minZ, maxZ))
                continue;

            GameObject obj = selectable.GetGameObject();
            selectedObjects.Add(selectable);
            selectable.OnSelected();
            Debug.Log($"Selected: {obj.name}", obj);
        }
        HashSetPool<IGameSelectable>.Release(found);
    }

    private void SelectNearestObjectAtClick()
    {
        if (selectionCamera == null)
            return;

        Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, selectionBoxHeight, 0));
        Ray clickRay = selectionCamera.ScreenPointToRay(selectionEndPosition);
        Vector3 clickWorldPosition = GetRayPlaneIntersection(clickRay, horizontalPlane);

        int hitCount = Physics.OverlapSphereNonAlloc(clickWorldPosition, clickSelectRadius, results, selectableLayerMask);
        if (hitCount == 0)
            return;

        IGameSelectable closest = null;
        float shortestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = results[i];
            if (col == null)
                continue;
            
            IFactionOwner faction = col.GetComponentInParent<IFactionOwner>();
            if (faction == null || faction.Faction != selectableFaction)
                continue;

            IGameSelectable selectable = col.GetComponentInParent<IGameSelectable>();
            if (selectable == null)
                continue;

            if (!selectable.CanSelectFromClick)
                continue;

            Vector3 pos = selectable.GetPosition();
            float dx = pos.x - clickWorldPosition.x;
            float dz = pos.z - clickWorldPosition.z;
            float distSqr = (dx * dx) + (dz * dz);

            if (distSqr < shortestDistance)
            {
                shortestDistance = distSqr;
                closest = selectable;
            }
        }

        if (closest == null)
            return;

        selectedObjects.Clear();
        selectedObjects.Add(closest);
        closest.OnSelected();
        Debug.Log($"Selected: {closest.GetGameObject().name}", closest.GetGameObject());
    }
    
    private bool IsObjectInSelectionBox(IGameSelectable selectable, float minX, float maxX, float minZ, float maxZ)
    {
        // Get the object's transform position (x and z only)
        Vector3 position = selectable.GetPosition();
        
        // Check if the position is within the selection bounds
        return position.x >= minX && position.x <= maxX &&
               position.z >= minZ && position.z <= maxZ;
    }
    
    private void DeselectAll()
    {
        foreach (IGameSelectable selectable in selectedObjects)
        {
            selectable.OnDeselected();
        }
        
        selectedObjects.Clear();
    }
    
    private void SetupSelectionBox()
    {
        selectionBoxRenderer.sortingOrder = 1000; // Make sure it renders on top
        selectionBoxRenderer.gameObject.SetActive(false);
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    
    /// <summary>
    /// Fills the currently selected objects to <paramref name="selectedObjectsHolder"/>
    /// </summary>
    /// <param name="selectedObjectsHolder"> An empty List to fill selected objects in.</param>
    public void GetSelectedObjects(List<IGameSelectable> selectedObjectsHolder)
    {
        selectedObjectsHolder.Clear();
        selectedObjectsHolder.AddRange(selectedObjects);
    }
}

