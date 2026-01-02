using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public sealed class RequirementCostView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text quantityText;

    private ResourceType lastType;

    public void Set(ResourceCost cost)
    {
        if (quantityText != null)
        {
            quantityText.text = cost.amount.ToString();
        }

        if (icon == null)
        {
            return;
        }

        if (lastType == (ResourceType)cost.type)
        {
            return;
        }

        lastType = (ResourceType)cost.type;
        string key = $"Icons/{lastType}.png";
        Addressables.LoadAssetAsync<Sprite>(key).Completed += handle =>
        {
            if (handle.Result != null)
            {
                icon.sprite = handle.Result;
            }
        };
    }
}

