using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class AddHealthBar
{
    public static void Execute()
    {
        const string prefabPath = "Assets/Prefabs/Unit.prefab";

        using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
        var root = scope.prefabContentsRoot;

        // Remove ALL existing HealthBar components and HealthBarRoot
        foreach (var h in root.GetComponents<HealthBar>())
            Object.DestroyImmediate(h);
        var existing = root.transform.Find("HealthBarRoot");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // ── World-space Canvas ───────────────────────────────────
        var canvasGO = new GameObject("HealthBarRoot");
        canvasGO.transform.SetParent(root.transform, false);
        canvasGO.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        canvasGO.transform.localScale    = new Vector3(0.01f, 0.01f, 1f);

        var canvas        = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        var rt       = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80f, 10f);

        // ── Background ───────────────────────────────────────────
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRT         = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin   = Vector2.zero;
        bgRT.anchorMax   = Vector2.one;
        bgRT.offsetMin   = Vector2.zero;
        bgRT.offsetMax   = Vector2.zero;
        var bgImg        = bgGO.AddComponent<Image>();
        bgImg.color      = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // ── Fill (anchor-based width) ────────────────────────────
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(canvasGO.transform, false);
        var fillRT       = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;   // starts full; SetHP shrinks anchorMax.x
        fillRT.offsetMin = new Vector2(1f, 1f);
        fillRT.offsetMax = new Vector2(-1f, -1f);
        var fillImg      = fillGO.AddComponent<Image>();
        fillImg.color    = new Color(0.2f, 0.85f, 0.2f, 1f);

        // ── HealthBar component on Unit root ─────────────────────
        root.AddComponent<HealthBar>();

        Debug.Log("[WarGame] HealthBar rebuilt on Unit prefab.");
    }
}
