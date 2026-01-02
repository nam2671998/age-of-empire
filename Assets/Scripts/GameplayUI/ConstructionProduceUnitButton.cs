using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public sealed class ConstructionProduceUnitButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    private ConstructionInteractionUIController controller;
    private int unitId;

    public void Initialize(ConstructionInteractionUIController controller, int unitId)
    {
        this.controller = controller;

        if (this.unitId == unitId)
        {
            return;
        }

        this.unitId = unitId;
        Addressables.LoadAssetAsync<Sprite>($"Icons/{unitId}.png").Completed += handle => icon.sprite = handle.Result;
    }

    public void OnClick()
    {
        if (controller != null)
        {
            controller.SelectProduceUnit(unitId);
        }
    }
}

