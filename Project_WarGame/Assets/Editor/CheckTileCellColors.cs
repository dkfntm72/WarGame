using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class CheckTileCellColors
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        foreach (Transform child in grid.transform)
        {
            var tm = child.GetComponent<Tilemap>();
            if (tm == null) continue;
            if (child.name != "WaterFoamTilemap" && child.name != "WaterBgEdgeTilemap") continue;

            BoundsInt bounds = tm.cellBounds;
            bool hasNonWhite = false;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (!tm.HasTile(pos)) continue;
                var col = tm.GetColor(pos);
                if (col != Color.white)
                {
                    Debug.Log($"[CellColor] {child.name} ({x},{y}): {col}");
                    hasNonWhite = true;
                }
            }
            if (!hasNonWhite)
                Debug.Log($"[CellColor] {child.name}: 모든 셀 색상 = white (정상)");
        }

        // URP Renderer 설정 경로 확인
        string[] guids = AssetDatabase.FindAssets("t:UniversalRendererData", new[]{"Assets"});
        foreach (var g in guids)
            Debug.Log($"[URP] Renderer: {AssetDatabase.GUIDToAssetPath(g)}");

        string[] guids2 = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset", new[]{"Assets"});
        foreach (var g in guids2)
            Debug.Log($"[URP] Pipeline: {AssetDatabase.GUIDToAssetPath(g)}");
    }
}
