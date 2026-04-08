using System.Collections.Generic;

public static class Pathfinding
{
    // BFS path from start to end. Returns ordered list of nodes (not including start).
    // faction: 이동하는 유닛의 진영. 적 진영 유닛이 있는 타일은 통과 불가.
    public static List<TileNode> FindPath(GridManager grid, TileNode start, TileNode end, Faction faction = Faction.Neutral)
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
                // 적대 유닛이 점령한 타일은 통과 불가 (우호 유닛은 통과 가능, 목적지 자체는 허용)
                bool blockedByEnemy = faction != Faction.Neutral
                    && neighbor != end
                    && neighbor.HasUnit
                    && FactionHelper.IsHostileTo(faction, neighbor.OccupyingUnit.faction);
                if (!visited.Contains(neighbor) && neighbor.IsWalkable && GridManager.CanTransition(current, neighbor) && !blockedByEnemy)
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
