using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class PlaceWaterFoamOnWater
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.LogError("Grid not found"); return; }

        Tilemap terrainTilemap = null;
        Tilemap foamTilemap = null;
        foreach (var tm in grid.GetComponentsInChildren<Tilemap>(true))
        {
            if (tm.name == "TerrainTilemap") terrainTilemap = tm;
            if (tm.name == "WaterFoamTilemap") foamTilemap = tm;
        }

        if (terrainTilemap == null || foamTilemap == null)
        {
            Debug.LogError("Tilemap not found");
            return;
        }

        var waterTile = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Background color.asset");
        var foamTile = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/GameData/WaterFoam.asset");

        if (waterTile == null) { Debug.LogError("Water Background color tile not found"); return; }
        if (foamTile == null)  { Debug.LogError("WaterFoam tile not found"); return; }

        Undo.RecordObject(foamTilemap, "Place WaterFoam on Water tiles");

        int count = 0;
        BoundsInt bounds = terrainTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            var pos = new Vector3Int(x, y, 0);
            if (terrainTilemap.GetTile(pos) == waterTile)
            {
                foamTilemap.SetTile(pos, foamTile);
                count++;
            }
        }

        EditorUtility.SetDirty(foamTilemap);
        Debug.Log($"[WaterFoam] Water 타일 위치 {count}개에 WaterFoam 타일 배치 완료");
    }
}
