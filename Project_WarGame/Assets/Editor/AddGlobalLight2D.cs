using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor;

public class AddGlobalLight2D
{
    public static void Execute()
    {
        // 이미 있으면 중복 생성 안 함
        var existing = Object.FindFirstObjectByType<Light2D>();
        if (existing != null && existing.lightType == Light2D.LightType.Global)
        {
            Debug.Log($"[Light] Global Light2D 이미 존재: {existing.name}");
            return;
        }

        var go = new GameObject("Global Light 2D");
        Undo.RegisterCreatedObjectUndo(go, "Add Global Light2D");

        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.color = Color.white;
        light.intensity = 1f;

        EditorUtility.SetDirty(go);
        Debug.Log("[Light] Global Light2D 추가 완료 (white, intensity 1)");
    }
}
