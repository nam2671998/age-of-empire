using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles bound selection (horizontal rectangle) by dragging the mouse to select multiple GameObjects
/// </summary>
public class BoxSelection : MonoBehaviour
{
    [Header("Selection Settings")]
    [SerializeField] private Camera selectionCamera;
    [SerializeField] private LayerMask selectableLayerMask = -1; // All layers by default
    [SerializeField] private KeyCode selectionKey = KeyCode.Mouse0; // Left mouse button
    
    [Header("Selection Box Visual")]
    [SerializeField] private SpriteRenderer selectionBoxRenderer;
    [SerializeField] private float selectionBoxHeight = 0f; // Y position of the horizontal selection plane
    
    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0f, 0.5f);
    
    private Vector3 selectionStartPosition;
    private Vector3 selectionEndPosition;
    private Vector3 worldStartCorner; // World position of the first corner (where mouse was pressed)
    private bool isSelecting = false;
    private List<IGameSelectable> selectedObjects = new List<IGameSelectable>();
    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
    private Dictionary<GameObject, Renderer> objectRenderers = new Dictionary<GameObject, Renderer>();
    
    private Material highlightMaterial;
    
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
        
        // Create highlight material
        CreateHighlightMaterial();
    }
    
    void Update()
    {
        HandleSelectionInput();
        UpdateSelectionBox();
    }
    
    private void HandleSelectionInput()
    {
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
        selectionStartPosition = Input.mousePosition;
        selectionEndPosition = selectionStartPosition;
        
        // Convert the start position to world space on the horizontal plane (this is our fixed corner)
        Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, selectionBoxHeight, 0));
        Ray startRay = selectionCamera.ScreenPointToRay(selectionStartPosition);
        worldStartCorner = GetRayPlaneIntersection(startRay, horizontalPlane);
        worldStartCorner.y = 0.5f; // Set the Y position
        
        // Deselect all previous selections
        DeselectAll();
    }
    
    private void UpdateSelection()
    {
        selectionEndPosition = Input.mousePosition;
    }
    
    private void EndSelection()
    {
        isSelecting = false;
        
        // Perform selection
        SelectObjectsInBox();
        
        // Hide selection box
        if (selectionBoxRenderer != null)
        {
            selectionBoxRenderer.gameObject.SetActive(false);
        }
    }
    
    private void UpdateSelectionBox()
    {
        if (!isSelecting || selectionBoxRenderer == null)
        {
            return;
        }
        
        // Calculate selection rectangle in screen space
        Rect selectionRect = GetSelectionRect();
        
        if (selectionRect.width < 5f || selectionRect.height < 5f)
        {
            // Too small, hide the box
            selectionBoxRenderer.gameObject.SetActive(false);
            return;
        }
        
        // Show and update selection box
        selectionBoxRenderer.gameObject.SetActive(true);
        
        // Convert current mouse position to world space on the horizontal plane
        Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, selectionBoxHeight, 0));
        Ray endRay = selectionCamera.ScreenPointToRay(selectionEndPosition);
        Vector3 worldEndCorner = GetRayPlaneIntersection(endRay, horizontalPlane);
        worldEndCorner.y = 0.5f; // Set the Y position
        
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
        center.y = 2;
        
        // Position the rectangle and resize
        selectionBoxRenderer.transform.position = center;
        
        // Resize the sprite using size property
        selectionBoxRenderer.size = new Vector2(width, height);
    }
    
    private Vector3 GetRayPlaneIntersection(Ray ray, Plane plane)
    {
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        // Fallback: return point at default distance if ray doesn't hit plane
        return ray.GetPoint(selectionBoxHeight);
    }
    
    private Rect GetSelectionRect()
    {
        float xMin = Mathf.Min(selectionStartPosition.x, selectionEndPosition.x);
        float xMax = Mathf.Max(selectionStartPosition.x, selectionEndPosition.x);
        float yMin = Mathf.Min(selectionStartPosition.y, selectionEndPosition.y);
        float yMax = Mathf.Max(selectionStartPosition.y, selectionEndPosition.y);
        
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
    
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
        
        // Check if selection is too small
        if (Mathf.Abs(maxX - minX) < 0.1f || Mathf.Abs(maxZ - minZ) < 0.1f)
        {
            return; // Too small to be a valid selection
        }
        
        // Find all objects with IGameSelectable
        IGameSelectable[] allSelectables = FindObjectsOfType<MonoBehaviour>()
            .Where(mb => mb is IGameSelectable)
            .Cast<IGameSelectable>()
            .ToArray();
        
        selectedObjects.Clear();
        
        foreach (IGameSelectable selectable in allSelectables)
        {
            GameObject obj = selectable.GetGameObject();
            
            // Check layer mask
            if ((selectableLayerMask.value & (1 << obj.layer)) == 0)
            {
                continue;
            }
            
            // Check if object transform position (x, z) is within selection bounds
            if (IsObjectInSelectionBox(selectable, minX, maxX, minZ, maxZ))
            {
                selectedObjects.Add(selectable);
                selectable.OnSelected();
                HighlightObject(obj);
                Debug.Log($"Selected: {obj.name}", obj);
            }
        }
    }
    
    private bool IsObjectInSelectionBox(IGameSelectable selectable, float minX, float maxX, float minZ, float maxZ)
    {
        // Get the object's transform position (x and z only)
        Vector3 position = selectable.GetPosition();
        
        // Check if the position is within the selection bounds
        return position.x >= minX && position.x <= maxX &&
               position.z >= minZ && position.z <= maxZ;
    }
    
    private void HighlightObject(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }
        
        // Store original material if not already stored
        if (!originalMaterials.ContainsKey(obj))
        {
            originalMaterials[obj] = renderer.material;
            objectRenderers[obj] = renderer;
        }
        
        // Apply highlight material
        if (highlightMaterial != null)
        {
            renderer.material = highlightMaterial;
        }
    }
    
    private void DeselectAll()
    {
        foreach (IGameSelectable selectable in selectedObjects)
        {
            selectable.OnDeselected();
            RestoreObjectMaterial(selectable.GetGameObject());
        }
        
        selectedObjects.Clear();
    }
    
    private void RestoreObjectMaterial(GameObject obj)
    {
        if (originalMaterials.ContainsKey(obj))
        {
            Renderer renderer = objectRenderers[obj];
            if (renderer != null)
            {
                renderer.material = originalMaterials[obj];
            }
            
            originalMaterials.Remove(obj);
            objectRenderers.Remove(obj);
        }
    }
    
    private void SetupSelectionBox()
    {
        selectionBoxRenderer.sortingOrder = 1000; // Make sure it renders on top
        selectionBoxRenderer.gameObject.SetActive(false);
    }
    
    private void CreateHighlightMaterial()
    {
        highlightMaterial = new Material(Shader.Find("Standard"));
        highlightMaterial.SetFloat("_Mode", 3); // Transparent mode
        highlightMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        highlightMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        highlightMaterial.SetInt("_ZWrite", 0);
        highlightMaterial.DisableKeyword("_ALPHATEST_ON");
        highlightMaterial.EnableKeyword("_ALPHABLEND_ON");
        highlightMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        highlightMaterial.renderQueue = 3000;
        highlightMaterial.color = highlightColor;
    }
    
    /// <summary>
    /// Gets the currently selected objects
    /// </summary>
    public List<IGameSelectable> GetSelectedObjects()
    {
        return new List<IGameSelectable>(selectedObjects);
    }
    
    /// <summary>
    /// Clears the current selection
    /// </summary>
    public void ClearSelection()
    {
        DeselectAll();
    }
    
    void OnDestroy()
    {
        // Clean up materials
        if (highlightMaterial != null)
        {
            Destroy(highlightMaterial);
        }
    }
}

