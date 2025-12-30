using UnityEngine;

public sealed class SettlerActionUIView : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private SettlerActionUIController controller;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<SettlerActionUIController>();
        }

        if (controller != null)
        {
            controller.Initialize(this);
        }

        SetPanelVisible(false);
    }

    public void ClickBuildNewConstruction()
    {
        if (controller != null)
        {
            controller.RequestOpenBuildConstructionUI();
            uiPanel.SetActive(false);
        }
    }

    public void ClickAbortCurrentCommand()
    {
        if (controller != null)
        {
            controller.AbortCurrentCommand();
        }
    }

    public void SetPanelVisible(bool visible)
    {
        if (uiPanel == null)
        {
            return;
        }

        if (uiPanel != gameObject)
        {
            uiPanel.SetActive(visible);
        }
    }
}

