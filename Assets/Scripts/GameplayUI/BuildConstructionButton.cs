using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BuildConstructionButton : MonoBehaviour
{
    [SerializeField] private TMP_Text textBuilding;
    private BuildConstructionUIController controller;
    private int buildingId;

    public void Initialize(BuildConstructionUIController controller, int buildingId, string displayName)
    {
        this.controller = controller;
        this.buildingId = buildingId;
        textBuilding.text = displayName;
    }

    public void OnClick()
    {
        if (controller != null)
        {
            controller.SelectBuildOption(buildingId);
        }
    }
}

