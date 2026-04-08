using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class ResetTileCellColors
{
    public static void Execute()
    {
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.LogError("Grid not found"); return; }

        string[] targets = { "WaterFoamTilemap", "WaterBgEdgeTilemap" };

        foreach (Transform child in grid.transform)
        {
            if (System.Array.IndexOf(targets, child.name) < 0) continue;
            var tm = child.GetComponent<Tilemap>();
            if (tm == null) continue;

            Undo.RecordObject(tm, $"Reset {child.name} cell colors");

            BoundsInt bounds = tm.cellBounds;
            int count = 0;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (!tm.HasTile(pos)) continue;
                var col = tm.GetColor(pos);
                if (col != Color.white)
                {
                    tm.SetColor(pos, Color.white);
                    count++;
                }
            }

            EditorUtility.SetDirty(tm);
            Debug.Log($"[ResetColors] {child.name}: {count}개 셀 색상 white로 초기화 완료");
        }
    }
}
