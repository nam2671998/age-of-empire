using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildConstructionUIView : MonoBehaviour
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
        if (buildOptionsContainer == null || buildOptionButtonPrefab == null)
        {
            return;
        }

        int requiredCount = ConfigManager.BuildableConstructions.Count;
        EnsureButtonCount(requiredCount);

        for (int i = 0; i < requiredCount; i++)
        {
            var constructionId = ConfigManager.BuildableConstructions[i];
            Button button = spawnedButtons[i];
            if (button == null)
            {
                button = Instantiate(buildOptionButtonPrefab, buildOptionsContainer);
                spawnedButtons[i] = button;
            }
            else if (button.transform.parent != buildOptionsContainer)
            {
                button.transform.SetParent(buildOptionsContainer, false);
            }

            button.gameObject.SetActive(true);
            
            BuildConstructionButton buildButton = button.GetComponent<BuildConstructionButton>();
            if (buildButton == null)
            {
                buildButton = button.gameObject.AddComponent<BuildConstructionButton>();
            }
            buildButton.Initialize(controller, constructionId);
        }

        for (int i = requiredCount; i < spawnedButtons.Count; i++)
        {
            Button button = spawnedButtons[i];
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    private void EnsureButtonCount(int requiredCount)
    {
        for (int i = spawnedButtons.Count; i < requiredCount; i++)
        {
            Button button = Instantiate(buildOptionButtonPrefab, buildOptionsContainer);
            button.gameObject.SetActive(false);
            spawnedButtons.Add(button);
        }
    }
}
