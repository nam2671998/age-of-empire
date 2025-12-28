using UnityEngine;

public sealed class SettlerActionUIView : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private SettlerActionUIController controller;

    private CanvasGroup panelCanvasGroup;

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

        if (uiPanel == gameObject)
        {
            uiPanel.TryGetComponent(out panelCanvasGroup);
        }

        SetPanelVisible(false);
    }

    public void ClickBuildNewConstruction()
    {
        if (controller != null)
        {
            controller.RequestOpenBuildConstructionUI();
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
            return;
        }

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = visible ? 1f : 0f;
            panelCanvasGroup.interactable = visible;
            panelCanvasGroup.blocksRaycasts = visible;
        }
    }
}

