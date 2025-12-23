using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera targetCamera;
    
    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 2f; // Starting speed when keys are first pressed
    [SerializeField] private float maxSpeed = 20f; // Maximum speed when holding keys
    [SerializeField] private float acceleration = 10f; // How fast speed increases per second
    
    private float currentSpeed = 0f;
    
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
    }

    void Update()
    {
        if (targetCamera == null) return;
        
        Vector3 moveDirection;
        bool anyKeyHeld;
        GetMovementInput(out moveDirection, out anyKeyHeld);
        
        UpdateSpeed(anyKeyHeld);
        
        ApplyMovement(moveDirection);
    }
    
    private void GetMovementInput(out Vector3 moveDirection, out bool anyKeyHeld)
    {
        // Check which movement keys are being held down
        // Build up a movement direction vector based on key presses
        moveDirection = Vector3.zero;
        anyKeyHeld = false;
        
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector3.forward; // Move forward (positive Z)
            anyKeyHeld = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector3.back; // Move backward (negative Z)
            anyKeyHeld = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector3.left; // Move left (negative X)
            anyKeyHeld = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += Vector3.right; // Move right (positive X)
            anyKeyHeld = true;
        }
        
        // Normalize so diagonal movement isn't faster than cardinal directions
        if (moveDirection.magnitude > 0)
        {
            moveDirection.Normalize();
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
            currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);
        }
    }
    
    private void ApplyMovement(Vector3 moveDirection)
    {
        // Calculate movement for this frame using current speed
        // Only move along X and Z, keep Y unchanged
        Vector3 movement = new Vector3(
            moveDirection.x * currentSpeed * Time.deltaTime,
            0f, // Don't move on Y axis
            moveDirection.z * currentSpeed * Time.deltaTime
        );
        
        // Apply the movement directly to the camera position
        targetCamera.transform.position += movement;
    }
}
