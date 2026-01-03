using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionInteractionUIView : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Transform produceOptionsContainer;
    [SerializeField] private Button produceOptionButtonPrefab;
    [SerializeField] private ConstructionInteractionUIController controller;

    private readonly List<Button> spawnedButtons = new List<Button>();

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<ConstructionInteractionUIController>();
        }

        if (controller != null)
        {
            controller.Initialize(this);
        }

        SetProduceButtonVisible(false);
    }

    public void RefreshProduceOptions(int[] producibleUnitIds)
    {
        if (produceOptionsContainer == null || produceOptionButtonPrefab == null)
        {
            return;
        }

        int requiredCount = producibleUnitIds != null ? producibleUnitIds.Length : 0;
        EnsureButtonCount(requiredCount);

        for (int i = 0; i < requiredCount; i++)
        {
            int unitId = producibleUnitIds[i];
            Button button = spawnedButtons[i];
            if (button == null)
            {
                button = Instantiate(produceOptionButtonPrefab, produceOptionsContainer);
                spawnedButtons[i] = button;
            }
            else if (button.transform.parent != produceOptionsContainer)
            {
                button.transform.SetParent(produceOptionsContainer, false);
            }

            button.gameObject.SetActive(true);

            ConstructionProduceUnitButton produceButton = button.GetComponent<ConstructionProduceUnitButton>();
            if (produceButton == null)
            {
                produceButton = button.gameObject.AddComponent<ConstructionProduceUnitButton>();
            }

            produceButton.Initialize(controller, unitId);
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

    public void SetProduceButtonVisible(bool visible)
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

    private void EnsureButtonCount(int requiredCount)
    {
        for (int i = spawnedButtons.Count; i < requiredCount; i++)
        {
            Button button = Instantiate(produceOptionButtonPrefab, produceOptionsContainer);
            button.gameObject.SetActive(false);
            spawnedButtons.Add(button);
        }
    }
}
