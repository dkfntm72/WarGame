using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class PlaceWaterBgOnEdges
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.LogError("Grid not found"); return; }

        Tilemap foamTilemap = null;
        foreach (var tm in grid.GetComponentsInChildren<Tilemap>(true))
            if (tm.name == "WaterFoamTilemap") foamTilemap = tm;

        if (foamTilemap == null) { Debug.LogError("WaterFoamTilemap not found"); return; }

        // WaterBgEdgeTilemap이 없으면 생성
        Tilemap edgeBgTilemap = null;
        foreach (var tm in grid.GetComponentsInChildren<Tilemap>(true))
            if (tm.name == "WaterBgEdgeTilemap") edgeBgTilemap = tm;

        if (edgeBgTilemap == null)
        {
            var go = new GameObject("WaterBgEdgeTilemap");
            go.transform.SetParent(grid.transform, false);
            edgeBgTilemap = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = -2; // WaterFoamTilemap(-1) 보다 아래
            Undo.RegisterCreatedObjectUndo(go, "Create WaterBgEdgeTilemap");
            Debug.Log("[WaterBgEdge] WaterBgEdgeTilemap 생성 완료 (Order: -2)");
        }

        var waterTile = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Background color.asset");
        if (waterTile == null) { Debug.LogError("Water Background color tile not found"); return; }

        Undo.RecordObject(edgeBgTilemap, "Place WaterBg on Edge tiles");

        int count = 0;
        BoundsInt bounds = foamTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            var pos = new Vector3Int(x, y, 0);
            if (foamTilemap.GetTile(pos) != null)
            {
                edgeBgTilemap.SetTile(pos, waterTile);
                count++;
            }
        }

        EditorUtility.SetDirty(edgeBgTilemap);
        Debug.Log($"[WaterBgEdge] 가장자리 타일 {count}개에 Water Background color 배치 완료");
    }
}
