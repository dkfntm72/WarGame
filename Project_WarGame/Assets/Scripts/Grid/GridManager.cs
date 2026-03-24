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

        terrainTilemap.ClearAllTiles();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var type = mapData.GetTerrain(x, y);
                nodes[x, y] = new TileNode(x, y, type);
                terrainTilemap.SetTile(new Vector3Int(x, y, 0), settings.GetTerrainTile(type));
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

            if (current != start && current.IsWalkable)
            {
                // Can land unless occupied by same faction
                if (!current.HasUnit || current.OccupyingUnit.faction != faction)
                    result.Add(current);
            }

            if (steps >= moveRange) continue;

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!visited.ContainsKey(neighbor) && neighbor.IsWalkable)
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
}
