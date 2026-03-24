using UnityEngine;
using UnityEditor;
using TMPro;
using System.Linq;

/// <summary>
/// UI의 모든 TextMeshProUGUI를 한글 지원 폰트로 교체합니다.
/// Window > WarGame > Fix Korean Font
/// </summary>
public static class WarGameFontSetup
{
    [MenuItem("Window/WarGame/Fix Korean Font")]
    public static void FixKoreanFont()
    {
        // TMP에 내장된 NotoSans-Regular (한글 지원) 찾기
        var font = FindKoreanFont();
        if (font == null)
        {
            Debug.LogWarning("[WarGame] 한글 폰트를 찾을 수 없습니다. TMP Essential Resources를 임포트하세요.");
            return;
        }

        // 씬의 모든 TextMeshProUGUI 컴포넌트 교체
        var texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;
        foreach (var t in texts)
        {
            t.font = font;
            EditorUtility.SetDirty(t);
            count++;
        }

        // UnitButton 프리팹도 교체
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UnitButton.prefab");
        if (prefab != null)
        {
            var prefabTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in prefabTexts)
            {
                t.font = font;
                EditorUtility.SetDirty(t);
            }
            PrefabUtility.SavePrefabAsset(prefab);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[WarGame] 폰트 교체 완료: {count}개 TextMeshProUGUI → {font.name}");
    }

    static TMP_FontAsset FindKoreanFont()
    {
        // 1. 프로젝트에 이미 있는 한글 폰트 검색
        var guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        foreach (var guid in guids)
        {
            var path  = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (asset == null) continue;

            // NotoSans, 한국어, Korean 등 포함된 폰트 우선
            string lower = asset.name.ToLower();
            if (lower.Contains("noto") || lower.Contains("korean") || lower.Contains("nanum"))
                return asset;
        }

        // 2. TMP 기본 폰트 (LiberationSans) 반환 – 한글은 □로 표시되지만 에러는 없음
        foreach (var guid in guids)
        {
            var path  = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (asset != null) return asset;
        }

        return null;
    }
}
