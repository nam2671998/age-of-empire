using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed class SlotReservationController
{
    private readonly Dictionary<IMovementCapability, Vector2Int> reservedSlotByExecutor = new Dictionary<IMovementCapability, Vector2Int>();
    private readonly HashSet<Vector2Int> reservedSlots = new HashSet<Vector2Int>();
    private readonly List<Vector2Int> slots = new List<Vector2Int>();

    public SlotReservationController(Transform[] verticesTransforms)
    {
        InitializeSlots(verticesTransforms);
    }
    
    public SlotReservationController(Vector2Int center, int radius, int excludeRadius)
    {
        InitializeSlots(center, radius, excludeRadius);
    }

    public bool TryReservePosition(IMovementCapability movementOwner, out Vector3 position)
    {
        position = Vector3.zero;
        if (ReferenceEquals(movementOwner, null))
        {
            return false;
        }
        Transform movementOwnerTransform = movementOwner.GetTransform();
        if (movementOwnerTransform == null)
        {
            return false;
        }
        position = movementOwnerTransform.position;

        if (GridManager.Instance == null)
        {
            return false;
        }

        CleanupDestroyedReservations();

        if (reservedSlotByExecutor.TryGetValue(movementOwner, out Vector2Int existingCell))
        {
            position = GridManager.Instance.GridToWorld(existingCell);
            return true;
        }

        if (slots.Count == 0)
        {
            return false;
        }

        Vector2Int fromCell = GridManager.Instance.WorldToGrid(position);
        bool found = false;
        Vector2Int result = default;
        int shortestDistance = int.MaxValue;

        foreach (var cell in slots)
        {
            if (reservedSlots.Contains(cell))
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
            return false;
        }

        GridManager.Instance.ReserveCell(result, movementOwner);
        reservedSlots.Add(result);
        reservedSlotByExecutor[movementOwner] = result;
        position = GridManager.Instance.GridToWorld(result);
        return true;
    }

    public void ReleasePosition(IMovementCapability movementOwner)
    {
        if (ReferenceEquals(movementOwner, null))
        {
            return;
        }

        if (GridManager.Instance == null)
        {
            if (reservedSlotByExecutor.Remove(movementOwner, out Vector2Int cellToRemove))
            {
                reservedSlots.Remove(cellToRemove);
            }
            return;
        }

        if (reservedSlotByExecutor.Remove(movementOwner, out Vector2Int cell))
        {
            reservedSlots.Remove(cell);

            IMovementCapability occupant = GridManager.Instance.GetCellReservation(cell);
            if (occupant == null || occupant == movementOwner)
            {
                GridManager.Instance.FreeCell(cell);
            }
        }
    }

    private void CleanupDestroyedReservations()
    {
        if (reservedSlotByExecutor.Count == 0)
        {
            return;
        }

        var cellsToFree = new List<Vector2Int>();
        var keysToRemove = new List<IMovementCapability>();
        foreach (var kvp in reservedSlotByExecutor)
        {
            if (kvp.Key == null)
            {
                keysToRemove.Add(kvp.Key);
                cellsToFree.Add(kvp.Value);
            }
        }

        foreach (var commandExecutor in keysToRemove)
        {
            reservedSlotByExecutor.Remove(commandExecutor);
        }

        foreach (var cell in cellsToFree)
        {
            reservedSlots.Remove(cell);
            if (GridManager.Instance != null)
            {
                GridManager.Instance.FreeCell(cell);
            }
        }
    }

    public void InitializeSlots(Transform[] verticesTransforms)
    {
        slots.Clear();

        if (verticesTransforms == null || GridManager.Instance == null)
        {
            return;
        }

        List<Vector2Int> vertices = ListPool<Vector2Int>.Get();
        foreach (var transform in verticesTransforms)
        {
            if (transform != null)
            {
                vertices.Add(GridManager.Instance.WorldToGrid(transform.position));
            }
        }

        if (vertices.Count < 2)
        {
            ListPool<Vector2Int>.Release(vertices);
            return;
        }

        var unique = new HashSet<Vector2Int>();
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2Int a = vertices[i];
            Vector2Int b = vertices[(i + 1) % vertices.Count];
            GridLineCells.AddLineCells(a, b, unique, slots, true);
        }
        ListPool<Vector2Int>.Release(vertices);
    }

    private void InitializeSlots(Vector2Int center, int radius, int excludeRadius)
    {
        slots.Clear();

        if (radius <= excludeRadius)
            return;

        for (int r = excludeRadius + 1; r <= radius; r++)
        {
            // Top & bottom edges
            for (int x = -r; x <= r; x++)
            {
                slots.Add(new Vector2Int(center.x + x, center.y + r));
                slots.Add(new Vector2Int(center.x + x, center.y - r));
            }

            // Left & right edges (skip corners)
            for (int y = -r + 1; y <= r - 1; y++)
            {
                slots.Add(new Vector2Int(center.x + r, center.y + y));
                slots.Add(new Vector2Int(center.x - r, center.y + y));
            }
        }
    }
}

