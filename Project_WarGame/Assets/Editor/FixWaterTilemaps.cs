using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class FixWaterTilemaps
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.LogError("Grid not found"); return; }

        Tilemap terrainTilemap = null, foamTilemap = null, waterBgEdgeTilemap = null;
        TilemapRenderer terrainRenderer = null;
        foreach (Transform child in grid.transform)
        {
            var tm = child.GetComponent<Tilemap>();
            var rend = child.GetComponent<TilemapRenderer>();
            if (child.name == "TerrainTilemap")      { terrainTilemap = tm; terrainRenderer = rend; }
            if (child.name == "WaterFoamTilemap")     foamTilemap = tm;
            if (child.name == "WaterBgEdgeTilemap")   waterBgEdgeTilemap = tm;
        }

        // --- 1. WaterBgEdgeTilemap Material 동기화 ---
        if (waterBgEdgeTilemap != null && terrainRenderer != null)
        {
            var edgeRenderer = waterBgEdgeTilemap.GetComponent<TilemapRenderer>();
            if (edgeRenderer != null && terrainRenderer.sharedMaterial != null)
            {
                Undo.RecordObject(edgeRenderer, "Fix WaterBgEdge Material");
                edgeRenderer.sharedMaterial = terrainRenderer.sharedMaterial;
                EditorUtility.SetDirty(edgeRenderer);
                Debug.Log($"[Fix] WaterBgEdgeTilemap Material → {terrainRenderer.sharedMaterial.name}");
            }
        }

        // --- 2. WaterFoam 타일 재배치 (에셋 교체 후 레퍼런스 복구) ---
        var foamTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/GameData/WaterFoam.asset");
        var waterBgTile = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Background color.asset");

        if (foamTilemap != null && foamTile != null)
        {
            Undo.RecordObject(foamTilemap, "Re-place WaterFoam tiles");
            BoundsInt bounds = foamTilemap.cellBounds;
            int count = 0;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                // 이미 타일이 있던 자리(레퍼런스가 끊겨 null로 보이는 셀 포함)에 재배치
                if (foamTilemap.GetTile(pos) != null)
                {
                    foamTilemap.SetTile(pos, foamTile);
                    count++;
                }
            }
            // 레퍼런스가 null인 셀도 커버하기 위해 WaterBgEdge 기준으로 재배치
            if (waterBgEdgeTilemap != null)
            {
                BoundsInt eb = waterBgEdgeTilemap.cellBounds;
                for (int x = eb.xMin; x < eb.xMax; x++)
                for (int y = eb.yMin; y < eb.yMax; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    if (waterBgEdgeTilemap.GetTile(pos) != null && foamTilemap.GetTile(pos) == null)
                    {
                        foamTilemap.SetTile(pos, foamTile);
                        count++;
                    }
                }
            }
            EditorUtility.SetDirty(foamTilemap);
            Debug.Log($"[Fix] WaterFoam 타일 재배치 완료: {count}개");
        }

        Debug.Log("[Fix] 완료");
    }
}
