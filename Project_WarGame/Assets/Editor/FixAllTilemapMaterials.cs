using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class FixAllTilemapMaterials
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.LogError("Grid not found"); return; }

        // 모든 TilemapRenderer 및 Material 현황 출력
        foreach (Transform child in grid.transform)
        {
            var rend = child.GetComponent<TilemapRenderer>();
            if (rend == null) continue;
            Debug.Log($"[Material] {child.name} | Order:{rend.sortingOrder} | Material:{rend.sharedMaterial?.name ?? "NULL"}");
        }

        // TerrainTilemap의 Material을 기준으로 나머지 동기화
        TilemapRenderer terrainRenderer = null;
        foreach (Transform child in grid.transform)
        {
            if (child.name == "TerrainTilemap")
            {
                terrainRenderer = child.GetComponent<TilemapRenderer>();
                break;
            }
        }

        if (terrainRenderer == null || terrainRenderer.sharedMaterial == null)
        {
            Debug.LogError("TerrainTilemap renderer or material not found");
            return;
        }

        string[] targets = { "WaterFoamTilemap", "WaterBgEdgeTilemap" };
        foreach (Transform child in grid.transform)
        {
            foreach (var target in targets)
            {
                if (child.name == target)
                {
                    var rend = child.GetComponent<TilemapRenderer>();
                    if (rend != null && rend.sharedMaterial != terrainRenderer.sharedMaterial)
                    {
                        Undo.RecordObject(rend, $"Fix {target} Material");
                        rend.sharedMaterial = terrainRenderer.sharedMaterial;
                        EditorUtility.SetDirty(rend);
                        Debug.Log($"[Fix] {target} Material → {terrainRenderer.sharedMaterial.name}");
                    }
                    else if (rend != null)
                    {
                        Debug.Log($"[OK] {target} Material 이미 동일: {rend.sharedMaterial?.name}");
                    }
                }
            }
        }
    }
}
