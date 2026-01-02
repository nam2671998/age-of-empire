using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class RequirementUIView : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Transform requirementsContainer;
    [SerializeField] private RequirementCostView requirementCostPrefab;
    [SerializeField] private RequirementUIController controller;

    private readonly List<RequirementCostView> spawnedEntries = new List<RequirementCostView>();

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<RequirementUIController>();
        }

        if (controller != null)
        {
            controller.Initialize(this);
        }

        SetVisible(false);
    }

    public void Show(ResourceCost[] data)
    {
        SetVisible(true);
        SetRequirements(data);
    }

    private void SetVisible(bool visible)
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

    private void SetRequirements(ResourceCost[] requirements)
    {
        if (requirementsContainer == null || requirementCostPrefab == null)
        {
            return;
        }

        int requiredCount = requirements != null ? requirements.Length : 0;
        EnsureEntryCount(requiredCount);

        for (int i = 0; i < requiredCount; i++)
        {
            RequirementCostView view = spawnedEntries[i];
            if (view == null)
            {
                view = Instantiate(requirementCostPrefab, requirementsContainer);
                spawnedEntries[i] = view;
            }
            else if (view.transform.parent != requirementsContainer)
            {
                view.transform.SetParent(requirementsContainer, false);
            }

            view.gameObject.SetActive(true);
            view.Set(requirements[i]);
        }

        for (int i = requiredCount; i < spawnedEntries.Count; i++)
        {
            RequirementCostView view = spawnedEntries[i];
            if (view != null)
            {
                view.gameObject.SetActive(false);
            }
        }
    }

    private void EnsureEntryCount(int requiredCount)
    {
        for (int i = spawnedEntries.Count; i < requiredCount; i++)
        {
            RequirementCostView cost = Instantiate(requirementCostPrefab, requirementsContainer);
            cost.gameObject.SetActive(false);
            spawnedEntries.Add(cost);
        }
    }
}

