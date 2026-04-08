using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class ForceReplaceTiles
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.LogError("Grid not found"); return; }

        Tilemap foamTilemap = null, waterBgEdgeTilemap = null;
        foreach (Transform child in grid.transform)
        {
            if (child.name == "WaterFoamTilemap")   foamTilemap = child.GetComponent<Tilemap>();
            if (child.name == "WaterBgEdgeTilemap") waterBgEdgeTilemap = child.GetComponent<Tilemap>();
        }

        var foamTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/GameData/WaterFoam.asset");
        var waterBgTile = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Background color.asset");

        if (foamTile == null)  { Debug.LogError("WaterFoam.asset not found"); return; }
        if (waterBgTile == null) { Debug.LogError("Water Background color.asset not found"); return; }

        // WaterFoamTilemap: 기존 타일 전부 새 AnimatedTile로 교체 (null 포함 강제)
        if (foamTilemap != null)
        {
            Undo.RecordObject(foamTilemap, "Force Replace WaterFoam tiles");
            // 전체 범위를 한번 순회해서 기존 타일(null 포함)을 강제로 새 타일로 덮어씀
            BoundsInt bounds = foamTilemap.cellBounds;
            int count = 0;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                // HasTile은 missing reference도 true 반환
                if (foamTilemap.HasTile(pos))
                {
                    foamTilemap.SetTile(pos, foamTile);
                    count++;
                }
            }
            EditorUtility.SetDirty(foamTilemap);
            Debug.Log($"[ForceReplace] WaterFoamTilemap: {count}개 교체 완료");
        }

        // WaterBgEdgeTilemap: 동일하게 처리
        if (waterBgEdgeTilemap != null)
        {
            Undo.RecordObject(waterBgEdgeTilemap, "Force Replace WaterBgEdge tiles");
            BoundsInt bounds = waterBgEdgeTilemap.cellBounds;
            int count = 0;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (waterBgEdgeTilemap.HasTile(pos))
                {
                    waterBgEdgeTilemap.SetTile(pos, waterBgTile);
                    count++;
                }
            }
            EditorUtility.SetDirty(waterBgEdgeTilemap);
            Debug.Log($"[ForceReplace] WaterBgEdgeTilemap: {count}개 교체 완료");
        }
    }
}
