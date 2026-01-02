using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public sealed class BuildConstructionButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    private BuildConstructionUIController controller;
    private int buildingId;

    public void Initialize(BuildConstructionUIController controller, int buildingId, string displayName)
    {
        this.controller = controller;
        this.buildingId = buildingId;
        Addressables.LoadAssetAsync<Sprite>($"Icons/{buildingId}.png").Completed += handle => icon.sprite = handle.Result;
    }

    public void OnClick()
    {
        if (controller != null)
        {
            controller.SelectBuildOption(buildingId);
        }
    }
}

