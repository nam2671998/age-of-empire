using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class BuildConstructionUIView : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Transform buildOptionsContainer;
    [SerializeField] private Button buildOptionButtonPrefab;
    [SerializeField] private BuildConstructionUIController controller;

    private readonly List<Button> spawnedButtons = new List<Button>();
    private CanvasGroup panelCanvasGroup;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<BuildConstructionUIController>();
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

    public void Open()
    {
        SetPanelVisible(true);
        RefreshBuildOptions();
    }

    public void Close()
    {
        ClearButtons();
        SetPanelVisible(false);
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

    private void RefreshBuildOptions()
    {
        ClearButtons();

        if (buildOptionsContainer == null || buildOptionButtonPrefab == null)
        {
            return;
        }

        foreach (var option in GetFakeBuildOptions())
        {
            var localOption = option;
            Button button = Instantiate(buildOptionButtonPrefab, buildOptionsContainer);
            button.gameObject.SetActive(true);
            spawnedButtons.Add(button);

            Text text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = localOption.displayName;
            }

            button.onClick.AddListener(() => OnBuildOptionClicked(localOption.id));
        }
    }

    private void ClearButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
            {
                Destroy(spawnedButtons[i].gameObject);
            }
        }

        spawnedButtons.Clear();
    }

    private void OnBuildOptionClicked(int buildingId)
    {
        if (controller != null)
        {
            controller.SelectBuildOption(buildingId);
        }
    }

    private static List<BuildOption> GetFakeBuildOptions()
    {
        return new List<BuildOption>
        {
            new BuildOption(1, "House"),
            new BuildOption(2, "Barracks"),
        };
    }

    private readonly struct BuildOption
    {
        public readonly int id;
        public readonly string displayName;

        public BuildOption(int id, string displayName)
        {
            this.id = id;
            this.displayName = displayName;
        }
    }
}

