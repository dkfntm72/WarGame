using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class SetKoreanFont
{
    private const string FontPath = "Assets/Fonts/경기천년제목OTF_Bold SDF.asset";

    public static void Execute()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
        {
            Debug.LogError($"[SetKoreanFont] 폰트를 찾을 수 없음: {FontPath}");
            return;
        }

        // 1. TMP 기본 폰트 설정
        var settings = TMP_Settings.instance;
        if (settings != null)
        {
            var so = new SerializedObject(settings);
            var prop = so.FindProperty("m_defaultFontAsset");
            if (prop != null)
            {
                prop.objectReferenceValue = font;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
                Debug.Log("[SetKoreanFont] TMP 기본 폰트 설정 완료");
            }
        }

        // 2. 현재 열린 씬의 모든 TextMeshProUGUI 교체
        int count = 0;
        var allTmp = GameObject.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tmp in allTmp)
        {
            Undo.RecordObject(tmp, "Set Korean Font");
            tmp.font = font;
            EditorUtility.SetDirty(tmp);
            count++;
        }

        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log($"[SetKoreanFont] 완료 — TextMeshProUGUI {count}개 폰트 교체");
    }
}
