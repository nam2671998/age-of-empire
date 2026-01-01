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

    private Dictionary<Vector2Int, IMovementCapability> reservedCells = new();

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
        if (reservedCells.ContainsKey(gridPos) && (reservedCells[gridPos] == null || reservedCells[gridPos].GetTransform() == null))
        {
            reservedCells.Remove(gridPos);
        }
        return !reservedCells.ContainsKey(gridPos);
    }

    /// <summary>
    /// Finds the nearest free cell to the target position
    /// </summary>
    public Vector2Int FindNearestFreeCell(Vector3 targetWorldPosition, IMovementCapability requestingUnit = null)
    {
        Vector2Int targetCell = WorldToGrid(targetWorldPosition);

        // If target cell is free, use it
        if (IsCellFree(targetCell))
            return targetCell;

        // If the requesting unit already has this cell reserved, use it
        if (requestingUnit != null && reservedCells.TryGetValue(targetCell, out IMovementCapability occupant))
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
                    if (requestingUnit != null && reservedCells.TryGetValue(candidateCell, out IMovementCapability owner))
                    {
                        if (owner == requestingUnit)
                            return candidateCell;
                    }
                }
            }
        }

        // Fallback: return target cell anyway
        Debug.LogWarning($"GridManager: Could not find free cell within {maxSearchRadius} radius. Using target cell.");
        return targetCell;
    }

    /// <summary>
    /// Reserves a cell for a specific unit
    /// </summary>
    public bool ReserveCell(Vector2Int gridPos, IMovementCapability unit)
    {
        if (unit == null)
            return false;

        // Free any previous reservation by this unit
        FreeUnitReservation(unit);

        // Reserve the new cell
        reservedCells[gridPos] = unit;
        return true;
    }

    /// <summary>
    /// Frees a specific cell
    /// </summary>
    public void FreeCell(Vector2Int gridPos)
    {
        reservedCells.Remove(gridPos);
    }

    /// <summary>
    /// Frees any cell reserved by a specific unit
    /// </summary>
    public void FreeUnitReservation(IMovementCapability unit)
    {
        if (unit == null)
            return;

        // Find and remove all cells reserved by this unit
        List<Vector2Int> cellsToFree = new List<Vector2Int>();
        foreach (var kvp in reservedCells)
        {
            if (kvp.Value == unit)
                cellsToFree.Add(kvp.Key);
        }

        foreach (var cell in cellsToFree)
        {
            reservedCells.Remove(cell);
        }
    }

    /// <summary>
    /// Gets the unit that has reserved a specific cell
    /// </summary>
    public IMovementCapability GetCellReservation(Vector2Int gridPos)
    {
        return reservedCells.GetValueOrDefault(gridPos);
    }
}
