using TMPro;
using UnityEngine;

public sealed class MainUIView : MonoBehaviour
{
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private MainUIController controller;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<MainUIController>();
        }

        if (controller != null)
        {
            controller.Initialize(this);
        }
    }

    public void SetWood(int amount)
    {
        if (woodText != null)
        {
            woodText.text = amount.ToString();
        }
    }

    public void SetStone(int amount)
    {
        if (stoneText != null)
        {
            stoneText.text = amount.ToString();
        }
    }

    public void SetFood(int amount)
    {
        if (foodText != null)
        {
            foodText.text = amount.ToString();
        }
    }
}

