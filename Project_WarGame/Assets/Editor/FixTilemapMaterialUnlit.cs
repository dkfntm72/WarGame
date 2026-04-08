using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class FixTilemapMaterialUnlit
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.LogError("Grid not found"); return; }

        // TerrainTilemap이 실제로 사용하는 Material을 기준으로 맞춤
        // (lit vs unlit 관계없이 TerrainTilemap과 동일하게)
        TilemapRenderer terrainRenderer = null;
        foreach (Transform child in grid.transform)
            if (child.name == "TerrainTilemap")
                terrainRenderer = child.GetComponent<TilemapRenderer>();

        if (terrainRenderer == null) { Debug.LogError("TerrainTilemap not found"); return; }

        // 모든 타일맵 material 현황 출력
        foreach (Transform child in grid.transform)
        {
            var rend = child.GetComponent<TilemapRenderer>();
            if (rend == null) continue;
            Debug.Log($"Before | {child.name}: {rend.sharedMaterial?.name} | mode:{rend.mode}");
        }

        // WaterFoamTilemap, WaterBgEdgeTilemap → TerrainTilemap과 완전히 동일한 설정
        string[] targets = { "WaterFoamTilemap", "WaterBgEdgeTilemap" };
        foreach (Transform child in grid.transform)
        {
            if (System.Array.IndexOf(targets, child.name) < 0) continue;
            var rend = child.GetComponent<TilemapRenderer>();
            if (rend == null) continue;

            Undo.RecordObject(rend, $"Fix {child.name} Renderer");
            rend.sharedMaterial = terrainRenderer.sharedMaterial;
            rend.mode = terrainRenderer.mode;
            rend.maskInteraction = terrainRenderer.maskInteraction;
            EditorUtility.SetDirty(rend);
            Debug.Log($"Fixed | {child.name}: material={rend.sharedMaterial?.name}, mode={rend.mode}");
        }
    }
}
