using UnityEngine;

public class GridEntity : MonoBehaviour, IGridEntity
{
    [SerializeField] private Vector2Int gridSize = Vector2Int.one;

    private void OnEnable()
    {
        GridManager.Instance.ReserveArea(transform.position, gridSize, this);
    }

    private void OnDisable()
    {
        GridManager.Instance.FreeUnitReservation(this);
    }

    public Transform GetTransform()
    {
        return transform;
    }
}

