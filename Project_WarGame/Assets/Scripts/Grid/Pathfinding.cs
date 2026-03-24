using System.Collections.Generic;

public static class Pathfinding
{
    // BFS path from start to end. Returns ordered list of nodes (not including start).
    public static List<TileNode> FindPath(GridManager grid, TileNode start, TileNode end)
    {
        var parent  = new Dictionary<TileNode, TileNode>();
        var visited = new HashSet<TileNode> { start };
        var queue   = new Queue<TileNode>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == end) break;

            foreach (var neighbor in GetNeighbors(grid, current))
            {
                if (!visited.Contains(neighbor) && neighbor.IsWalkable)
                {
                    visited.Add(neighbor);
                    parent[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        var path = new List<TileNode>();
        if (!parent.ContainsKey(end)) return path;

        var node = end;
        while (node != start)
        {
            path.Add(node);
            if (!parent.TryGetValue(node, out node)) break;
        }
        path.Reverse();
        return path;
    }

    private static List<TileNode> GetNeighbors(GridManager grid, TileNode tile)
    {
        var result = new List<TileNode>(4);
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };
        for (int i = 0; i < 4; i++)
        {
            var n = grid.GetTile(tile.X + dx[i], tile.Y + dy[i]);
            if (n != null) result.Add(n);
        }
        return result;
    }
}
