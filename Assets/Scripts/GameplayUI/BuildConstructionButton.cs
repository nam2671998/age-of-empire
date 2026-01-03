using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildConstructionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private RequirementEventChannelSo onRequirementHovered;
    private BuildConstructionUIController controller;
    private int buildingId;
    private string displayName;

    public void Initialize(BuildConstructionUIController controller, int buildingId, string displayName)
    {
        this.controller = controller;
        this.buildingId = buildingId;
        this.displayName = displayName;
        Addressables.LoadAssetAsync<Sprite>($"Icons/{buildingId}.png").Completed += handle => icon.sprite = handle.Result;
    }

    public void OnClick()
    {
        if (controller != null)
        {
            controller.SelectBuildOption(buildingId);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onRequirementHovered == null)
        {
            return;
        }

        if (ConfigManager.TryGetConstructionCosts(buildingId, out ResourceCost[] costs))
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

