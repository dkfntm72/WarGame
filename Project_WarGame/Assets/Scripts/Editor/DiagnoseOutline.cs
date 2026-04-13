using UnityEngine;
using UnityEditor;
using TMPro;

public class DiagnoseOutline
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UI");
        if (canvas == null) { Debug.LogError("UI 캔버스를 찾을 수 없음"); return; }

        foreach (var tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            var mat = tmp.fontSharedMaterial;
            float outlineW = mat != null ? mat.GetFloat(ShaderUtilities.ID_OutlineWidth) : -1f;
            Debug.Log($"[TMP] {tmp.name} | material={mat?.name} | outlineWidth={outlineW}");
        }
    }
}
