using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera targetCamera;
    
    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 2f; // Starting speed when keys are first pressed
    [SerializeField] private float maxSpeed = 20f; // Maximum speed when holding keys
    [SerializeField] private float acceleration = 10f; // How fast speed increases per second
    [SerializeField] private bool allowArrowKeyMovement = true;
    [SerializeField] private BoxCollider movementBounds;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomStep = 5f;
    [SerializeField] private float minZoomOffset = -20f;
    [SerializeField] private float maxZoomOffset = 20f;

    [Header("Edge Scroll")]
    [SerializeField] private bool allowEdgeScroll = true;
    [SerializeField] private float edgeScrollSpeed = 12f;
    [SerializeField] private float edgeThresholdPixels = 16f;

    [Header("Middle Mouse Drag")]
    [SerializeField] private bool allowMiddleMouseDrag = true;
    [SerializeField] private float middleMouseDragSpeed = 0.02f;
    
    private float currentSpeed = 0f;
    private Vector3 panPosition;
    private float zoomOffset = 0f;
    private bool isMiddleMouseDragging;
    private Vector3 lastMousePosition;

    public Camera TargetCamera => targetCamera;

    void Start()
    {
        // If no camera assigned, try to find one on this GameObject
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }
        
        // If still no camera, try to find the main camera
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        currentSpeed = minSpeed;
        if (targetCamera != null)
        {
            panPosition = targetCamera.transform.localPosition;
        }

        if (movementBounds == null)
        {
            movementBounds = GetComponent<BoxCollider>();
        }

        ClampPanPositionToBounds();
    }

    public Vector3 GetPanPosition()
    {
        if (targetCamera == null)
        {
            return panPosition;
        }

        if (panPosition == Vector3.zero && targetCamera.transform.localPosition != Vector3.zero)
        {
            return targetCamera.transform.localPosition - GetZoomDirection() * zoomOffset;
        }

        return panPosition;
    }

    public void SetPanPosition(Vector3 newPanPosition, bool clampToBounds = true)
    {
        panPosition = newPanPosition;
        if (clampToBounds)
        {
            ClampPanPositionToBounds();
        }
    }

    public void SetPanPositionAndApply(Vector3 newPanPosition, bool clampToBounds = true)
    {
        panPosition = newPanPosition;
        if (clampToBounds)
        {
            ClampPanPositionToBounds();
        }

        if (targetCamera != null)
        {
            targetCamera.transform.localPosition = panPosition + GetZoomDirection() * zoomOffset;
        }
    }

    void Update()
    {
        if (targetCamera == null) return;
        
        HandleMiddleMouseDrag();

        Vector3 keyMoveDirection;
        Vector3 edgeScrollDirection;
        bool anyKeyHeld;
        GetMovementInput(out keyMoveDirection, out edgeScrollDirection, out anyKeyHeld);
        
        UpdateSpeed(anyKeyHeld);
        
        ApplyMovement(keyMoveDirection, edgeScrollDirection);
        ApplyZoom(Input.mouseScrollDelta.y);

        targetCamera.transform.localPosition = panPosition + GetZoomDirection() * zoomOffset;
    }
    
    private void GetMovementInput(out Vector3 keyMoveDirection, out Vector3 edgeScrollDirection, out bool anyKeyHeld)
    {
        // Check which movement keys are being held down
        // Build up a movement direction vector based on key presses
        keyMoveDirection = Vector3.zero;
        edgeScrollDirection = Vector3.zero;
        anyKeyHeld = false;
        
        if (allowArrowKeyMovement && Input.GetKey(KeyCode.UpArrow))
        {
            keyMoveDirection += Vector3.forward;
            anyKeyHeld = true;
        }
        if (allowArrowKeyMovement && Input.GetKey(KeyCode.DownArrow))
        {
            keyMoveDirection += Vector3.back;
            anyKeyHeld = true;
        }
        if (allowArrowKeyMovement && Input.GetKey(KeyCode.LeftArrow))
        {
            keyMoveDirection += Vector3.left;
            anyKeyHeld = true;
        }
        if (allowArrowKeyMovement && Input.GetKey(KeyCode.RightArrow))
        {
            keyMoveDirection += Vector3.right;
            anyKeyHeld = true;
        }

        if (!isMiddleMouseDragging && allowEdgeScroll)
        {
            edgeScrollDirection = GetEdgeScrollDirection();
            if (edgeScrollDirection != Vector3.zero)
            {
                anyKeyHeld = true;
            }
        }
        
        // Normalize so diagonal movement isn't faster than cardinal directions
        if (keyMoveDirection.magnitude > 0)
        {
            keyMoveDirection.Normalize();
        }

        if (edgeScrollDirection.magnitude > 0)
        {
            edgeScrollDirection.Normalize();
        }
    }
    
    private void UpdateSpeed(bool anyKeyHeld)
    {
        // Update speed based on whether keys are being held
        // If any key is held, start at min speed and accelerate toward max speed
        // If no keys are held, decelerate toward zero using the same acceleration rate
        if (anyKeyHeld)
        {
            // Increase speed over time, clamped to max speed
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);
        }
        else
        {
            // Decelerate using the same acceleration value, clamped to zero
            currentSpeed -= acceleration * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
        }
    }
    
    private void ApplyMovement(Vector3 keyMoveDirection, Vector3 edgeScrollDirection)
    {
        // Calculate movement for this frame using current speed
        // Only move along X and Z, keep Y unchanged
        Vector3 movement = new Vector3(
            (keyMoveDirection.x * currentSpeed + edgeScrollDirection.x * edgeScrollSpeed) * Time.deltaTime,
            0f, // Don't move on Y axis
            (keyMoveDirection.z * currentSpeed + edgeScrollDirection.z * edgeScrollSpeed) * Time.deltaTime
        );
        
        // Apply the movement directly to the camera position
        panPosition += movement;
        ClampPanPositionToBounds();
    }

    private void ApplyZoom(float scrollDelta)
    {
        if (Mathf.Approximately(scrollDelta, 0f))
            return;

        float desiredOffset = zoomOffset + scrollDelta * zoomStep;
        zoomOffset = Mathf.Clamp(desiredOffset, minZoomOffset, maxZoomOffset);
    }

    private Vector3 GetZoomDirection()
    {
        Vector3 direction = targetCamera.transform.forward;
        float magnitude = direction.magnitude;
        if (magnitude <= 0.0001f)
            return Vector3.forward;

        return direction / magnitude;
    }

    private void HandleMiddleMouseDrag()
    {
        if (!allowMiddleMouseDrag)
        {
            isMiddleMouseDragging = false;
            return;
        }

        if (Input.GetMouseButtonDown(2))
        {
            isMiddleMouseDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isMiddleMouseDragging = false;
        }

        if (!isMiddleMouseDragging)
        {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        Vector3 delta = mousePosition - lastMousePosition;
        lastMousePosition = mousePosition;

        Vector3 dragMovement = new Vector3(-delta.x, 0f, -delta.y) * middleMouseDragSpeed;
        panPosition += dragMovement;
        ClampPanPositionToBounds();
    }

    private Vector3 GetEdgeScrollDirection()
    {
        Vector3 mousePosition = Input.mousePosition;
        float width = Screen.width;
        float height = Screen.height;

        if (mousePosition.x < 0f || mousePosition.y < 0f || mousePosition.x > width || mousePosition.y > height)
        {
            return Vector3.zero;
        }

        Vector3 direction = Vector3.zero;
        if (mousePosition.x <= edgeThresholdPixels)
        {
            direction += Vector3.left;
        }
        else if (mousePosition.x >= width - edgeThresholdPixels)
        {
            direction += Vector3.right;
        }

        if (mousePosition.y <= edgeThresholdPixels)
        {
            direction += Vector3.back;
        }
        else if (mousePosition.y >= height - edgeThresholdPixels)
        {
            direction += Vector3.forward;
        }

        return direction;
    }

    private void ClampPanPositionToBounds()
    {
        if (movementBounds == null)
        {
            return;
        }

        Vector3 center = movementBounds.center;
        Vector3 halfSize = movementBounds.size * 0.5f;
        float minX = center.x - halfSize.x;
        float maxX = center.x + halfSize.x;
        float minZ = center.z - halfSize.z;
        float maxZ = center.z + halfSize.z;

        panPosition.x = Mathf.Clamp(panPosition.x, minX, maxX);
        panPosition.z = Mathf.Clamp(panPosition.z, minZ, maxZ);
    }
}
