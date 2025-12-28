using UnityEngine;

public sealed class ConstructionInteractionUIView : MonoBehaviour
{
    [SerializeField] private GameObject buttonProduceUnit;
    [SerializeField] private ConstructionInteractionUIController controller;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<ConstructionInteractionUIController>();
        }

        if (controller != null)
        {
            controller.Initialize(this);
        }
    }

    public void ClickProduceUnit()
    {
        if (controller != null)
        {
            controller.ProduceFirstUnit();
        }
    }

    public void SetProduceButtonVisible(bool visible)
    {
        if (buttonProduceUnit != null)
        {
            buttonProduceUnit.gameObject.SetActive(visible);
        }
    }
}

