using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class SetUnitButtonSprite
{
    public static void Execute()
    {
        var outlineMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Fonts/경기천년제목OTF_Bold SDF - Outline.mat");
        if (outlineMat == null) { Debug.LogError("[SetUnitButtonSprite] Outline 머티리얼을 찾을 수 없습니다."); return; }

        using (var scope = new PrefabUtility.EditPrefabContentsScope("Assets/Prefabs/UnitButton.prefab"))
        {
            var root = scope.prefabContentsRoot;
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                tmp.fontSharedMaterial = outlineMat;
                Debug.Log($"[SetUnitButtonSprite] {tmp.name} 아웃라인 머티리얼 적용");
            }
        }

        Debug.Log("[SetUnitButtonSprite] UnitButton 프리팹 아웃라인 적용 완료!");
    }
}
