using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEditor;

public class CheckLightsAndLayers
{
    public static void Execute()
    {
        // 씬의 2D Light 확인
        var lights = Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        Debug.Log($"[Light] 씬 내 Light2D 수: {lights.Length}");
        foreach (var l in lights)
            Debug.Log($"  {l.name} | type:{l.lightType} | color:{l.color} | intensity:{l.intensity} | layers:{l.targetSortingLayers?.Length}");

        // TilemapRenderer Sorting Layer 확인
        var grid = GameObject.Find("Grid");
        if (grid == null) return;
        foreach (Transform child in grid.transform)
        {
            var rend = child.GetComponent<TilemapRenderer>();
            if (rend == null) continue;
            Debug.Log($"[Rend] {child.name} | sortingLayer:{rend.sortingLayerName}(id:{rend.sortingLayerID}) | order:{rend.sortingOrder} | material:{rend.sharedMaterial?.name}");
        }
    }
}
