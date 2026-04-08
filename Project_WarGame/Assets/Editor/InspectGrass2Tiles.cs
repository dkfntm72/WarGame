using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;

public class InspectGrass2Tiles
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        var terrainTm = grid.transform.Find("TerrainTilemap").GetComponent<Tilemap>();

        // GameSettings에서 grassTile, grass2Tile 가져오기
        var gsGuids = AssetDatabase.FindAssets("t:GameSettings");
        if (gsGuids.Length == 0) { Debug.LogError("GameSettings not found"); return; }
        var gs = AssetDatabase.LoadAssetAtPath<GameSettings>(AssetDatabase.GUIDToAssetPath(gsGuids[0]));
        Debug.Log($"grassTile={gs.grassTile?.name} grass2Tile={gs.grass2Tile?.name}");

        // TerrainTilemap의 모든 타일 이름 수집
        terrainTm.CompressBounds();
        var bounds = terrainTm.cellBounds;
        var tileCounts = new Dictionary<string, int>();
        var grass2Positions = new List<Vector3Int>();
        var grassPositions = new HashSet<Vector3Int>();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            var pos = new Vector3Int(x, y, 0);
            var tile = terrainTm.GetTile(pos);
            if (tile == null) continue;
            tileCounts.TryGetValue(tile.name, out int c);
            tileCounts[tile.name] = c + 1;

            if (gs.grass2Tile != null && tile == gs.grass2Tile)
                grass2Positions.Add(pos);
            else if (gs.grassTile != null && tile == gs.grassTile)
                grassPositions.Add(pos);
        }

        foreach (var kv in tileCounts)
            Debug.Log($"  tile={kv.Key} count={kv.Value}");

        Debug.Log($"grass2Tile positions ({grass2Positions.Count}): " +
            string.Join(" ", grass2Positions.ConvertAll(p => $"({p.x},{p.y})")));
        Debug.Log($"grassTile positions ({grassPositions.Count})");
    }
}
