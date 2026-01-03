using System.Collections.Generic;
using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    [SerializeField] private float focusDurationSeconds = 0.5f;
    [SerializeField] private AnimationCurve focusEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private readonly Dictionary<KeyCode, List<GameObject>> targetsByKey = new Dictionary<KeyCode, List<GameObject>>();
    private readonly Dictionary<KeyCode, int> indexByKey = new Dictionary<KeyCode, int>();
    private readonly List<KeyCode> registeredKeys = new List<KeyCode>();

    private bool isFocusing;
    private float focusStartTime;
    private Vector3 focusStartPan;
    private Vector3 focusTargetPan;

    private void Awake()
    {
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }
    }

    private void Update()
    {
        for (int i = 0; i < registeredKeys.Count; i++)
        {
            KeyCode key = registeredKeys[i];
            if (Input.GetKeyDown(key))
            {
                FocusNext(key);
            }
        }

        if (isFocusing)
        {
            TickFocus();
        }
    }

    public void RegisterEntity(CameraFocusEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        KeyCode key = entity.AssignedKey;
        if (key == KeyCode.None)
        {
            return;
        }

        GameObject target = entity.TargetGameObject;
        if (target == null)
        {
            return;
        }

        if (!targetsByKey.TryGetValue(key, out List<GameObject> list))
        {
            list = new List<GameObject>();
            targetsByKey.Add(key, list);
            registeredKeys.Add(key);
            indexByKey[key] = -1;
        }

        if (!list.Contains(target))
        {
            list.Add(target);
        }
    }

    public void UnregisterEntity(CameraFocusEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        KeyCode key = entity.AssignedKey;
        if (key == KeyCode.None)
        {
            return;
        }

        GameObject target = entity.TargetGameObject;
        if (target == null)
        {
            return;
        }

        if (!targetsByKey.TryGetValue(key, out List<GameObject> list))
        {
            return;
        }

        int removedIndex = list.IndexOf(target);
        if (removedIndex >= 0)
        {
            list.RemoveAt(removedIndex);
        }

        CleanupNullTargets(list);

        if (list.Count == 0)
        {
            targetsByKey.Remove(key);
            indexByKey.Remove(key);
            registeredKeys.Remove(key);
            return;
        }

        if (indexByKey.TryGetValue(key, out int currentIndex))
        {
            if (currentIndex >= list.Count)
            {
                indexByKey[key] = list.Count - 1;
            }
        }
    }

    private void FocusNext(KeyCode key)
    {
        if (!targetsByKey.TryGetValue(key, out List<GameObject> list))
        {
            return;
        }

        CleanupNullTargets(list);
        if (list.Count == 0)
        {
            indexByKey[key] = -1;
            return;
        }

        if (!indexByKey.TryGetValue(key, out int previousIndex))
        {
            previousIndex = -1;
        }

        int nextIndex = previousIndex + 1;
        nextIndex %= list.Count;
        indexByKey[key] = nextIndex;

        GameObject target = list[nextIndex];
        Focus(target);
    }

    public void Focus(GameObject target, bool instant = false)
    {
        if (cameraController == null)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        Vector3 startPan = cameraController.GetPanPosition();
        Vector3 targetWorld = target.transform.position;

        focusStartPan = startPan;
        if (TryComputePanToCenterTarget(startPan, targetWorld, out Vector3 centeredPan))
        {
            focusTargetPan = centeredPan;
        }
        else
        {
            focusTargetPan = startPan;
            focusTargetPan.x = targetWorld.x;
            focusTargetPan.z = targetWorld.z;
        }

        if (instant)
        {
            cameraController.SetPanPositionAndApply(focusTargetPan);
            isFocusing = false;
            return;
        }

        focusStartTime = Time.time;
        isFocusing = true;
    }

    private bool TryComputePanToCenterTarget(Vector3 startPan, Vector3 targetWorldPosition, out Vector3 centeredPan)
    {
        centeredPan = startPan;

        Camera cam = cameraController != null ? cameraController.TargetCamera : null;
        if (cam == null)
        {
            return false;
        }

        Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Plane plane = new Plane(Vector3.up, new Vector3(0f, targetWorldPosition.y, 0f));
        if (!plane.Raycast(centerRay, out float distance))
        {
            return false;
        }

        Vector3 currentCenterWorld = centerRay.GetPoint(distance);
        Vector3 worldDelta = targetWorldPosition - currentCenterWorld;
        worldDelta.y = 0f;

        Vector3 localDelta;
        Transform parent = cam.transform.parent;
        if (parent != null)
        {
            localDelta = parent.InverseTransformVector(worldDelta);
        }
        else
        {
            localDelta = worldDelta;
        }

        centeredPan = startPan + new Vector3(localDelta.x, 0f, localDelta.z);
        return true;
    }

    private void TickFocus()
    {
        if (cameraController == null)
        {
            isFocusing = false;
            return;
        }

        float duration = Mathf.Max(0f, focusDurationSeconds);
        float t = duration <= 0.0001f ? 1f : (Time.time - focusStartTime) / duration;
        if (t >= 1f)
        {
            cameraController.SetPanPositionAndApply(focusTargetPan);
            isFocusing = false;
            return;
        }

        float eased = focusEasing != null ? focusEasing.Evaluate(t) : t;
        Vector3 pan = Vector3.LerpUnclamped(focusStartPan, focusTargetPan, eased);
        cameraController.SetPanPositionAndApply(pan);
    }

    private static void CleanupNullTargets(List<GameObject> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null)
            {
                list.RemoveAt(i);
            }
        }
    }
}
