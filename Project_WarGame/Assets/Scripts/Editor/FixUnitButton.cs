using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class FixUnitButton
{
    public static void Execute()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/경기천년제목OTF_Bold SDF.asset");
        if (fontAsset == null) { Debug.LogError("[FixUnitButton] 폰트를 찾을 수 없습니다."); return; }

        var outlineMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Fonts/경기천년제목OTF_Bold SDF - Outline.mat");
        if (outlineMat == null) { Debug.LogError("[FixUnitButton] Outline 머티리얼을 찾을 수 없습니다."); return; }

        using (var scope = new PrefabUtility.EditPrefabContentsScope("Assets/Prefabs/UnitButton.prefab"))
        {
            var root = scope.prefabContentsRoot;
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.font               = fontAsset;
                tmp.fontSharedMaterial = outlineMat;
                Debug.Log($"[FixUnitButton] {tmp.name} — 폰트+아웃라인 적용");
            }
        }

        Debug.Log("[FixUnitButton] 완료!");
    }
}
