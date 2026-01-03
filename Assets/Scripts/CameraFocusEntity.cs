using UnityEngine;

public class CameraFocusEntity : MonoBehaviour
{
    [SerializeField] private KeyCode assignedKey = KeyCode.None;
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private CameraFocusController focusController;

    public KeyCode AssignedKey => assignedKey;
    public GameObject TargetGameObject => targetGameObject != null ? targetGameObject : gameObject;

    public void SubscribeFocus()
    {
        if (focusController == null)
        {
            focusController = FindObjectOfType<CameraFocusController>();
        }
        focusController?.RegisterEntity(this);
    }

    private void OnDisable()
    {
        focusController?.UnregisterEntity(this);
    }
}

