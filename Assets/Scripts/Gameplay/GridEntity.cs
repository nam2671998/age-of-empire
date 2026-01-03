using UnityEngine;

public sealed class GridEntity : MonoBehaviour, IGridEntity
{
    [SerializeField] private Vector2Int gridSize = Vector2Int.one;
    private bool subscribed;

    private void OnEnable()
    {
        if (subscribed || GridManager.Instance == null)
        {
            return;
        }

        GridManager.Instance.ReserveArea(transform.position, gridSize, this);
        subscribed = true;
    }

    private void OnDisable()
    {
        if (!subscribed || GridManager.Instance == null)
        {
            subscribed = false;
            return;
        }

        GridManager.Instance.FreeUnitReservation(this);
        subscribed = false;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}

