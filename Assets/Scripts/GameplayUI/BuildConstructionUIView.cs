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
            
            BuildConstructionButton buildButton = button.GetComponent<BuildConstructionButton>();
            if (buildButton == null)
            {
                buildButton = button.gameObject.AddComponent<BuildConstructionButton>();
            }
            buildButton.Initialize(controller, localOption.id, localOption.displayName);
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

    private static List<BuildOption> GetFakeBuildOptions()
    {
        return new List<BuildOption>
        {
            new BuildOption(1001001, "Barrack"),
            new BuildOption(1001002, "Archery"),
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
