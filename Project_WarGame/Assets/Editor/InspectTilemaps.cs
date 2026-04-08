using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;

public class InspectTilemaps
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.Log("RESULT: Grid not found"); return; }

        var tilemaps = grid.GetComponentsInChildren<Tilemap>(true);
        foreach (var tm in tilemaps)
        {
            Debug.Log($"=== Tilemap: {tm.name} | Order: {tm.GetComponent<TilemapRenderer>().sortingOrder} ===");
            BoundsInt bounds = tm.cellBounds;
            var tileSet = new Dictionary<string, int>();
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                var tile = tm.GetTile(pos);
                if (tile != null)
                {
                    string path = AssetDatabase.GetAssetPath(tile);
                    string key = $"{tile.name} | {path}";
                    tileSet.TryGetValue(key, out int count);
                    tileSet[key] = count + 1;
                }
            }
            foreach (var kv in tileSet)
                Debug.Log($"  [{kv.Value}x] {kv.Key}");
        }
    }
}
