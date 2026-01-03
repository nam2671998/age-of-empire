using UnityEngine;

public class RequirementUIController : MonoBehaviour
{
    [SerializeField] private RequirementEventChannelSo onRequirementChanged;

    private RequirementUIView view;
    private RequirementUIModel model;

    private void Awake()
    {
        model = new RequirementUIModel();
    }

    public void Initialize(RequirementUIView view)
    {
        this.view = view;
        if (view != null)
        {
            view.Show(default);
        }
    }

    private void OnEnable()
    {
        if (onRequirementChanged != null)
        {
            onRequirementChanged.Register(OnRequirementChanged);
        }
    }

    private void OnDisable()
    {
        if (onRequirementChanged != null)
        {
            onRequirementChanged.Unregister(OnRequirementChanged);
        }
    }

    private void OnRequirementChanged(ResourceCost[] data)
    {
        model.Set(data);
        if (view != null)
        {
            view.Show(data);
        }
    }
}

