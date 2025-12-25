using UnityEngine;

public class ToggleAnimationMode : MonoBehaviour
{
    private bool AnimatorsActive = true;
    [SerializeField]
    private AnimatorController SMRController;
    [SerializeField]
    private AnimatedMeshController ThrottledController;

    private void Start()
    {
        ThrottledController.DeactivateAll();
        SMRController.ActivateAll();
    }
}
