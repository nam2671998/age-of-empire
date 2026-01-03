using UnityEngine;

public class MainUIController : MonoBehaviour
{
    [SerializeField] private Faction faction = Faction.Player1;
    [SerializeField] private VoidEventChannelSO onResourceInventoryChanged;

    private MainUIView view;
    private MainUIModel model;

    public void Initialize(MainUIView view)
    {
        this.view = view;
        model = new MainUIModel(faction);
        RefreshView(force: true);
    }

    private void OnEnable()
    {
        if (onResourceInventoryChanged != null)
        {
            onResourceInventoryChanged.Register(OnResourceInventoryChanged);
        }

        RefreshView(force: true);
    }

    private void OnDisable()
    {
        if (onResourceInventoryChanged != null)
        {
            onResourceInventoryChanged.Unregister(OnResourceInventoryChanged);
        }
    }

    private void OnResourceInventoryChanged()
    {
        RefreshView(force: false);
    }

    private void RefreshView(bool force)
    {
        if (view == null || model == null)
        {
            return;
        }

        if (!force && !model.Refresh())
        {
            return;
        }

        if (force)
        {
            model.Refresh();
        }

        view.SetWood(model.Wood);
        view.SetStone(model.Stone);
        view.SetFood(model.Food);
    }
}
