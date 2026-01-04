using UnityEngine;

public class GridEntity : MonoBehaviour, IGridEntity
{
    [SerializeField] private Vector2Int gridSize = Vector2Int.one;
    public Vector2Int GetSize()
    {
        return gridSize;
    }

    private void OnEnable()
    {
        GridManager.Instance.ReserveArea(transform.position, gridSize, this);
    }

    private void OnDisable()
    {
        GridManager.Instance.FreeFullyUnitReservation(this);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void OverrideSize(Vector3 center, Vector2Int newSize)
    {
        GridManager.Instance.FreeUnitReservation(this);
        gridSize = newSize;
        GridManager.Instance.ReserveArea(center, gridSize, this);
    }
}
