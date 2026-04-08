using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class InspectTilemapTiles
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.LogError("Grid not found"); return; }

        string[] names = { "TerrainTilemap", "Grass2BgTilemap" };
        foreach (var n in names)
        {
            var go = grid.transform.Find(n)?.gameObject;
            if (go == null) { Debug.Log($"{n}: NOT FOUND"); continue; }
            var tm = go.GetComponent<Tilemap>();
            var renderer = go.GetComponent<TilemapRenderer>();
            tm.CompressBounds();
            var bounds = tm.cellBounds;
            int count = 0;
            var sample = new System.Text.StringBuilder();
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var tile = tm.GetTile(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    count++;
                    if (count <= 5)
                        sample.Append($"({x},{y})={tile.name} ");
                }
            }
            Debug.Log($"[{n}] sortLayer={renderer.sortingLayerName} orderInLayer={renderer.sortingOrder} tileCount={count} bounds={bounds.xMin}~{bounds.xMax},{bounds.yMin}~{bounds.yMax} | samples: {sample}");
        }
    }
}
