using System.Collections.Generic;
using UnityEngine;

public static class GridLineCells
{
    public static void AddLineCells(Vector2Int start, Vector2Int end, HashSet<Vector2Int> unique, List<Vector2Int> output, bool excludeEnd)
    {
        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            var cell = new Vector2Int(x0, y0);
            if (cell != end || !excludeEnd)
            {
                if (unique.Add(cell))
                {
                    output.Add(cell);
                }
            }

            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}

