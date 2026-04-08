using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Reference")]
    public Grid grid;

    [Header("Tilemaps")]
    public Tilemap terrainTilemap;
    public Tilemap highlightTilemap;

    public int Width  { get; private set; }
    public int Height { get; private set; }

    private TileNode[,] nodes;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(MapData mapData, GameSettings settings)
    {
        Width  = mapData.width;
        Height = mapData.height;
        nodes  = new TileNode[Width, Height];

        // 타일맵 시각은 에디터에서 배치된 상태 그대로 유지.
        // 런타임에 ClearAllTiles + RuleTile 재설정 시 엣지/코너 스프라이트가 뒤바뀌는 문제 방지.
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var type = mapData.GetTerrain(x, y);
                nodes[x, y] = new TileNode(x, y, type);
            }
        }
    }

    public TileNode GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
        return nodes[x, y];
    }

    public TileNode GetTileAtWorld(Vector3 worldPos)
    {
        var cell = grid.WorldToCell(worldPos);
        return GetTile(cell.x, cell.y);
    }

    public Vector3 GetWorldCenter(int x, int y) =>
        grid.GetCellCenterWorld(new Vector3Int(x, y, 0));

    // BFS movement range – returns tiles a unit can land on
    public List<TileNode> GetMovementRange(TileNode start, int moveRange, Faction faction)
    {
        var result  = new List<TileNode>();
        var visited = new Dictionary<TileNode, int> { [start] = 0 };
        var queue   = new Queue<(TileNode node, int steps)>();
        queue.Enqueue((start, 0));

        while (queue.Count > 0)
        {
            var (current, steps) = queue.Dequeue();

            if (current != start && current.IsWalkable && current.TerrainType != TerrainType.Slope)
            {
                // 적대 유닛이 없으면 착지 가능 (입력 핸들러에서 !HasUnit 로 재필터링)
                if (!current.HasUnit || !FactionHelper.IsHostileTo(faction, current.OccupyingUnit.faction))
                    result.Add(current);
            }

            if (steps >= moveRange) continue;

            foreach (var neighbor in GetNeighbors(current))
            {
                // 적대 유닛이 점령한 타일은 통과 불가 (우호 유닛은 통과 가능)
                bool blockedByEnemy = neighbor.HasUnit && FactionHelper.IsHostileTo(faction, neighbor.OccupyingUnit.faction);
                if (!visited.ContainsKey(neighbor) && neighbor.IsWalkable && CanTransition(current, neighbor) && !blockedByEnemy)
                {
                    visited[neighbor] = steps + 1;
                    queue.Enqueue((neighbor, steps + 1));
                }
            }
        }

        return result;
    }

    // Manhattan distance attack range (no line-of-sight check for ranged)
    public List<TileNode> GetAttackRange(TileNode center, int range)
    {
        var result = new List<TileNode>();
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                if (Mathf.Abs(dx) + Mathf.Abs(dy) > range) continue;
                var tile = GetTile(center.X + dx, center.Y + dy);
                if (tile != null) result.Add(tile);
            }
        }
        return result;
    }

    // 이동(파란색) + 공격 가능 적 칸(빨간색) 동시 표시
    public void ShowBothHighlights(List<TileNode> moveTiles, List<TileNode> attackTiles, GameSettings settings)
    {
        highlightTilemap.ClearAllTiles();
        foreach (var tile in moveTiles)
            highlightTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), settings.moveHighlightTile);
        foreach (var tile in attackTiles)
            highlightTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), settings.attackHighlightTile);
    }

    public void ShowMoveHighlight(List<TileNode> tiles, GameSettings settings)
    {
        highlightTilemap.ClearAllTiles();
        foreach (var tile in tiles)
            highlightTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), settings.moveHighlightTile);
    }

    public void ShowAttackHighlight(List<TileNode> tiles, GameSettings settings)
    {
        highlightTilemap.ClearAllTiles();
        foreach (var tile in tiles)
            highlightTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), settings.attackHighlightTile);
    }

    public void ClearHighlights() => highlightTilemap.ClearAllTiles();

    private List<TileNode> GetNeighbors(TileNode tile)
    {
        var result = new List<TileNode>(4);
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };
        for (int i = 0; i < 4; i++)
        {
            var n = GetTile(tile.X + dx[i], tile.Y + dy[i]);
            if (n != null) result.Add(n);
        }
        return result;
    }

    // Grass2는 Slope 위칸(좌우 방향)을 통해서만 진입/이탈 가능
    // 위칸 정의: 해당 Slope의 y+1이 Slope가 아닌 칸
    public static bool CanTransition(TileNode from, TileNode to)
    {
        // Grass2 진입: Slope 위칸에서 좌우(같은 Y)로만 허용
        if (to.TerrainType == TerrainType.Grass2 && from.TerrainType != TerrainType.Grass2)
        {
            if (from.TerrainType != TerrainType.Slope || to.Y != from.Y) return false;
            var above = Instance?.GetTile(from.X, from.Y + 1);
            if (above != null && above.TerrainType == TerrainType.Slope) return false;
        }

        // Grass2 이탈: Slope 위칸으로 좌우(같은 Y)로만 허용
        if (from.TerrainType == TerrainType.Grass2 && to.TerrainType != TerrainType.Grass2)
        {
            if (to.TerrainType != TerrainType.Slope || to.Y != from.Y) return false;
            var above = Instance?.GetTile(to.X, to.Y + 1);
            if (above != null && above.TerrainType == TerrainType.Slope) return false;
        }

        return true;
    }
}
