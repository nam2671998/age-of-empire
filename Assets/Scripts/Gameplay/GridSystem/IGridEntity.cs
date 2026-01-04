using UnityEngine;

public interface IGridEntity
{
    Transform GetTransform();
    Vector2Int GetSize();
}