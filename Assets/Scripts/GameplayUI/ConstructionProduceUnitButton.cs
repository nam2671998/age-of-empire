using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ConstructionProduceUnitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private RequirementEventChannelSo onRequirementHovered;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onRequirementHovered == null)
        {
            return;
        }

        if (ConfigManager.TryGetUnitCosts(unitId, out var costs))
        {
            onRequirementHovered.Raise(costs);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onRequirementHovered == null)
        {
            return;
        }

        onRequirementHovered.Raise(Array.Empty<ResourceCost>());
    }
}

