using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;

public class InspectGrass2Config
{
    public static void Execute()
    {
        // Grass2 TerrainTileConfig 찾기
        foreach (var guid in AssetDatabase.FindAssets("t:TerrainTileConfig"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var cfg = AssetDatabase.LoadAssetAtPath<TerrainTileConfig>(path);
            if (cfg == null) continue;
            Debug.Log($"TerrainTileConfig: {path} type={cfg.terrainType} defaultTile={cfg.defaultTile?.name} rules={cfg.rules.Count}");
        }

        // TileTerrainMapping 확인
        foreach (var guid in AssetDatabase.FindAssets("t:TileTerrainMapping"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mapping = AssetDatabase.LoadAssetAtPath<TileTerrainMapping>(path);
            if (mapping == null) continue;
            Debug.Log($"TileTerrainMapping: {path}");
            foreach (var entry in mapping.entries)
                Debug.Log($"  tile={entry.tile?.name} → {entry.terrainType}");
        }

        // TerrainTilemap에서 Tilemap_color1 계열 타일 종류 파악
        var grid = GameObject.Find("Grid");
        var terrainTm = grid?.transform.Find("TerrainTilemap")?.GetComponent<Tilemap>();
        if (terrainTm == null) return;

        terrainTm.CompressBounds();
        var bounds = terrainTm.cellBounds;
        var tilePositions = new Dictionary<string, List<string>>();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            var pos = new Vector3Int(x, y, 0);
            var tile = terrainTm.GetTile(pos);
            if (tile == null) continue;
            if (!tilePositions.ContainsKey(tile.name))
                tilePositions[tile.name] = new List<string>();
            if (tilePositions[tile.name].Count < 3)
                tilePositions[tile.name].Add($"({x},{y})");
        }
        foreach (var kv in tilePositions)
            Debug.Log($"  TerrainTilemap tile={kv.Key} sample={string.Join(",", kv.Value)}");
    }
}
