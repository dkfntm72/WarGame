using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CheckHealthBar
{
    public static void Execute()
    {
        const string prefabPath = "Assets/Prefabs/Unit.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogError("Unit prefab not found!"); return; }

        var hb = prefab.GetComponent<HealthBar>();
        if (hb == null) { Debug.LogError("No HealthBar component on Unit root!"); return; }

        Debug.Log($"HealthBar found on: {hb.gameObject.name}");

        var canvasRoot = prefab.transform.Find("HealthBarRoot");
        if (canvasRoot == null) { Debug.LogError("HealthBarRoot child not found!"); return; }

        Debug.Log($"HealthBarRoot found. ActiveSelf={canvasRoot.gameObject.activeSelf}");

        var canvas = canvasRoot.GetComponent<Canvas>();
        Debug.Log($"Canvas renderMode={canvas?.renderMode}, sortingOrder={canvas?.sortingOrder}");

        var rt = canvasRoot.GetComponent<RectTransform>();
        Debug.Log($"RectTransform sizeDelta={rt?.sizeDelta}, localPos={canvasRoot.localPosition}, localScale={canvasRoot.localScale}");

        var fill = canvasRoot.Find("Fill")?.GetComponent<Image>();
        Debug.Log($"Fill image fillAmount={fill?.fillAmount}, type={fill?.type}");
    }
}
