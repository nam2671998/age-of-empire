using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Picks a nearby "standing cell" for a unit so multiple units can interact with the same target.
/// </summary>
/// <remarks>
/// The candidate cells are precomputed (either an outline from transforms or a ring around a center).
/// On request, we choose the closest candidate that is free in <see cref="GridManager"/>.
/// </remarks>
public class SlotReservationController
{
    // Candidate slot cells. These are potential standing positions around the target.
    private readonly List<Vector2Int> slots = new List<Vector2Int>();

    public SlotReservationController(Transform[] verticesTransforms)
    {
        InitializeSlots(verticesTransforms);
    }
    
    public SlotReservationController(Vector2Int center, int radius, int excludeRadius)
    {
        InitializeSlots(center, radius, excludeRadius);
    }

    public bool TryReservePosition(IGridEntity movementOwner, out Vector3 position)
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

        if (slots.Count == 0)
        {
            return false;
        }

        if (GridManager.Instance.TryGetReservedCell(movementOwner, out Vector2Int existingCell) && slots.Contains(existingCell))
        {
            position = GridManager.Instance.GridToWorld(existingCell);
            return true;
        }

        Vector2Int fromCell = GridManager.Instance.WorldToGrid(position);
        bool found = false;
        Vector2Int result = default;
        int shortestDistance = int.MaxValue;

        // Choose the closest available candidate slot to the unit.
        foreach (var cell in slots)
        {
            if (!GridManager.Instance.IsCellFree(cell))
            {
                IGridEntity occupant = GridManager.Instance.GetCellReservation(cell);
                if (occupant == null || occupant != movementOwner)
                {
                    continue;
                }
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

        // Reserve it in the grid so other systems (movement/pathing) also see it as occupied.
        if (!GridManager.Instance.ReserveCell(result, movementOwner))
        {
            return false;
        }
        position = GridManager.Instance.GridToWorld(result);
        return true;
    }

    public void ReleasePosition(IGridEntity movementOwner)
    {
        if (ReferenceEquals(movementOwner, null))
        {
            return;
        }

        if (GridManager.Instance == null)
        {
            return;
        }

        if (GridManager.Instance.TryGetReservedCell(movementOwner, out Vector2Int cell) && slots.Contains(cell))
        {
            // Only free the grid cell if we are still the owner (or it's already empty).
            IGridEntity occupant = GridManager.Instance.GetCellReservation(cell);
            if (occupant == null || occupant == movementOwner)
            {
                GridManager.Instance.FreeCell(movementOwner, cell);
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

        // Convert vertex transforms into grid cells and connect them as an outline.
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
            // Add the grid cells along each edge; exclude the end cell to avoid duplicates at corners.
            GridLineCells.AddLineCells(a, b, unique, slots, true);
        }
        ListPool<Vector2Int>.Release(vertices);
    }

    private void InitializeSlots(Vector2Int center, int radius, int excludeRadius)
    {
        slots.Clear();

        if (radius <= excludeRadius)
            return;

        // Produce a ring (or multiple rings) around a center point:
        // - r is the ring distance from center.
        // - excludeRadius leaves an empty area near the center so units don't overlap the target.
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

