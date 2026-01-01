using System.Collections.Generic;
using UnityEngine;

public sealed class ReservationController
{
    private readonly Dictionary<CommandExecutor, Vector2Int> reservedCellByExecutor = new Dictionary<CommandExecutor, Vector2Int>();
    private readonly HashSet<Vector2Int> reservedCells = new HashSet<Vector2Int>();
    private readonly List<Vector2Int> edgeSlots = new List<Vector2Int>();

    private Transform[] buildPositionTransforms;

    public ReservationController(Transform[] buildPositionTransforms)
    {
        this.buildPositionTransforms = buildPositionTransforms;
    }

    public void SetBuildPositionTransforms(Transform[] buildPositionTransforms)
    {
        this.buildPositionTransforms = buildPositionTransforms;
        edgeSlots.Clear();
    }

    public bool TryReservePosition(CommandExecutor executor, out Vector3 position)
    {
        position = Vector3.zero;
        if (ReferenceEquals(executor, null) || executor == null)
        {
            return false;
        }

        if (GridManager.Instance == null)
        {
            position = executor.transform.position;
            return false;
        }

        CleanupDestroyedReservations();
        EnsureEdgeSlots();

        if (reservedCellByExecutor.TryGetValue(executor, out Vector2Int existingCell))
        {
            position = GridManager.Instance.GridToWorld(existingCell);
            return true;
        }

        if (edgeSlots.Count == 0)
        {
            position = executor.transform.position;
            return false;
        }

        Vector2Int fromCell = GridManager.Instance.WorldToGrid(executor.transform.position);
        bool found = false;
        Vector2Int result = default;
        int shortestDistance = int.MaxValue;

        for (int i = 0; i < edgeSlots.Count; i++)
        {
            Vector2Int cell = edgeSlots[i];
            if (reservedCells.Contains(cell))
            {
                continue;
            }

            if (!GridManager.Instance.IsCellFree(cell))
            {
                continue;
            }

            Vector2Int delta = cell - fromCell;
            int distance = delta.sqrMagnitude;
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                result = cell;
                found = true;
            }
        }

        if (!found)
        {
            position = executor.transform.position;
            return false;
        }

        GridManager.Instance.ReserveCell(result, executor);
        reservedCells.Add(result);
        reservedCellByExecutor[executor] = result;
        position = GridManager.Instance.GridToWorld(result);
        return true;
    }

    public void ReleasePosition(CommandExecutor executor)
    {
        if (ReferenceEquals(executor, null))
        {
            return;
        }

        if (GridManager.Instance == null)
        {
            if (reservedCellByExecutor.TryGetValue(executor, out Vector2Int cellToRemove))
            {
                reservedCellByExecutor.Remove(executor);
                reservedCells.Remove(cellToRemove);
            }
            return;
        }

        if (reservedCellByExecutor.TryGetValue(executor, out Vector2Int cell))
        {
            reservedCellByExecutor.Remove(executor);
            reservedCells.Remove(cell);

            CommandExecutor occupant = GridManager.Instance.GetCellReservation(cell);
            if (occupant == null || occupant == executor)
            {
                GridManager.Instance.FreeCell(cell);
            }
        }
    }

    private void CleanupDestroyedReservations()
    {
        if (reservedCellByExecutor.Count == 0)
        {
            return;
        }

        var cellsToFree = new List<Vector2Int>();
        var keysToRemove = new List<CommandExecutor>();
        foreach (var kvp in reservedCellByExecutor)
        {
            if (kvp.Key == null)
            {
                keysToRemove.Add(kvp.Key);
                cellsToFree.Add(kvp.Value);
            }
        }

        for (int i = 0; i < keysToRemove.Count; i++)
        {
            reservedCellByExecutor.Remove(keysToRemove[i]);
        }

        for (int i = 0; i < cellsToFree.Count; i++)
        {
            reservedCells.Remove(cellsToFree[i]);
            if (GridManager.Instance != null)
            {
                GridManager.Instance.FreeCell(cellsToFree[i]);
            }
        }
    }

    private void EnsureEdgeSlots()
    {
        edgeSlots.Clear();

        if (buildPositionTransforms == null || GridManager.Instance == null)
        {
            return;
        }

        var vertices = new List<Vector2Int>(buildPositionTransforms.Length);
        for (int i = 0; i < buildPositionTransforms.Length; i++)
        {
            var t = buildPositionTransforms[i];
            if (t != null)
            {
                vertices.Add(GridManager.Instance.WorldToGrid(t.position));
            }
        }

        if (vertices.Count < 2)
        {
            return;
        }

        var unique = new HashSet<Vector2Int>();
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2Int a = vertices[i];
            Vector2Int b = vertices[(i + 1) % vertices.Count];
            GridLineCells.AddLineCells(a, b, unique, edgeSlots, true);
        }
    }
}

