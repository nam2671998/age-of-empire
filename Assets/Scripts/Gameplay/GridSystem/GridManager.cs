using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages a grid system for unit movement, handling cell reservations
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;

    private readonly Dictionary<Vector2Int, IGridEntity> reservedCells = new();
    private readonly Dictionary<IGridEntity, List<Vector2Int>> reserveCellsByEntity = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Converts a world position to grid coordinates
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        float half = cellSize * 0.5f;
        int x = Mathf.RoundToInt((worldPosition.x - half) / cellSize);
        int z = Mathf.RoundToInt((worldPosition.z - half) / cellSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Converts grid coordinates to world position (center of cell)
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        float half = cellSize * 0.5f;
        return new Vector3((gridPosition.x * cellSize) + half, 0, (gridPosition.y * cellSize) + half);
    }

    /// <summary>
    /// Checks if a cell is free (not reserved by any unit)
    /// </summary>
    public bool IsCellFree(Vector2Int gridPos)
    {
        if (reservedCells.TryGetValue(gridPos, out IGridEntity owner) && owner != null && owner.GetTransform() == null)
        {
            reservedCells.Remove(gridPos);
            if (reserveCellsByEntity.TryGetValue(owner, out List<Vector2Int> cells))
            {
                cells.Remove(gridPos);
            }
        }
        return !reservedCells.ContainsKey(gridPos);
    }

    /// <summary>
    /// Finds the nearest free cell to the target position
    /// </summary>
    public Vector2Int FindNearestFreeCell(Vector3 targetWorldPosition, IGridEntity requestingUnit = null)
    {
        Vector2Int targetCell = WorldToGrid(targetWorldPosition);

        // If target cell is free, use it
        if (IsCellFree(targetCell))
            return targetCell;

        // If the requesting unit already has this cell reserved, use it
        if (requestingUnit != null && reservedCells.TryGetValue(targetCell, out IGridEntity occupant))
        {
            if (occupant == requestingUnit)
                return targetCell;
        }

        // Spiral search for nearest free cell
        int maxSearchRadius = 50;
        for (int radius = 1; radius <= maxSearchRadius; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    // Only check cells on the current radius perimeter
                    if (Mathf.Abs(dx) != radius && Mathf.Abs(dz) != radius)
                        continue;

                    Vector2Int candidateCell = new Vector2Int(targetCell.x + dx, targetCell.y + dz);
                    
                    if (IsCellFree(candidateCell))
                        return candidateCell;

                    // Check if this cell is reserved by the requesting unit
                    if (requestingUnit != null && reservedCells.TryGetValue(candidateCell, out IGridEntity owner))
                    {
                        if (owner == requestingUnit)
                            return candidateCell;
                    }
                }
            }
        }

        // Fallback: return target cell anyway
        return targetCell;
    }

    /// <summary>
    /// Reserves a cell for a specific unit
    /// </summary>
    public bool ReserveCell(Vector2Int gridPos, IGridEntity entity)
    {
        if (entity == null)
            return false;

        // Free any previous reservation by this unit
        FreeUnitReservation(entity);

        // Reserve the new cell
        reservedCells[gridPos] = entity;
        if (!reserveCellsByEntity.ContainsKey(entity))
        {
            Vector2Int size = entity.GetSize();
            reserveCellsByEntity.Add(entity, new List<Vector2Int>(size.x * size.y));
        }
        reserveCellsByEntity[entity].Add(gridPos);
        return true;
    }

    public void ReserveArea(Vector3 worldPosition, Vector2Int size, IGridEntity entity)
    {
        if (entity == null)
            return;

        FreeUnitReservation(entity);
        if (size.x == 0 || size.y == 0)
            return;

        int width = Mathf.Max(1, size.x);
        int height = Mathf.Max(1, size.y);

        Vector2Int origin = WorldToGrid(worldPosition);

        // Ensure list exists
        if (!reserveCellsByEntity.TryGetValue(entity, out var cellList))
        {
            cellList = new List<Vector2Int>(width * height);
            reserveCellsByEntity.Add(entity, cellList);
        }
        else
        {
            cellList.Clear();
        }
        
        // Prioritize Top and Right in case the Height/Width is even number
        int left = (width  - 1) / 2;
        int right = width  / 2;
        int bottom = (height - 1) / 2;
        int top = height / 2;

        for (int x = origin.x - left; x <= origin.x + right; x++)
        {
            for (int z = origin.y - bottom; z <= origin.y + top; z++)
            {
                Vector2Int cell = new Vector2Int(x, z);
                reservedCells[cell] = entity;
                cellList.Add(cell);
            }
        }
    }


    /// <summary>
    /// Frees a specific cell
    /// </summary>
    public void FreeCell(IGridEntity movementOwner, Vector2Int gridPos)
    {
        reservedCells.Remove(gridPos);
        if (movementOwner != null && reserveCellsByEntity.TryGetValue(movementOwner, out List<Vector2Int> cells))
        {
            cells.Remove(gridPos);
        }
    }

    /// <summary>
    /// Frees any cell reserved by a specific unit
    /// </summary>
    public void FreeUnitReservation(IGridEntity unit)
    {
        if (unit == null)
            return;
        if (reserveCellsByEntity.TryGetValue(unit, out List<Vector2Int> cells))
        {
            foreach (var cell in cells)
            {
                reservedCells.Remove(cell);
            }
        }
    }
    
    public void FreeFullyUnitReservation(IGridEntity unit)
    {
        if (unit == null)
            return;
        if (reserveCellsByEntity.TryGetValue(unit, out List<Vector2Int> cells))
        {
            foreach (var cell in cells)
            {
                reservedCells.Remove(cell);
            }
            reserveCellsByEntity.Remove(unit);
        }
    }

    public bool TryGetReservedCell(IGridEntity entity, out Vector2Int cell)
    {
        cell = default;
        if (entity == null)
        {
            return false;
        }

        if (!reserveCellsByEntity.TryGetValue(entity, out List<Vector2Int> cells) || cells.Count == 0)
        {
            return false;
        }

        cell = cells[0];
        return true;
    }

    /// <summary>
    /// Gets the unit that has reserved a specific cell
    /// </summary>
    public IGridEntity GetCellReservation(Vector2Int gridPos)
    {
        return reservedCells.GetValueOrDefault(gridPos);
    }
}
