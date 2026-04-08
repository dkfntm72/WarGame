using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ToggleEventPanel
{
    // 미리보기 전용 — 씬 저장 안 함, 에디터에서만 일시적으로 표시
    public static void Preview()
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "EventPanel" && go.scene.IsValid())
            {
                go.SetActive(true);
                var tmp = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmp != null && tmp.gameObject.name == "EventText")
                    tmp.text = "제1장: 국경의 전투\n\n우리 왕국의 국경이 위협받고 있습니다.\n용감한 병사들이여, 적을 물리쳐라!";
                break;
            }
        }
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        Debug.Log("[ToggleEventPanel] 미리보기 (저장 안 됨)");
    }

    // EventPanel 비활성화 후 씬 저장
    public static void Execute()
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "EventPanel" && go.scene.IsValid())
            {
                go.SetActive(false);
                EditorUtility.SetDirty(go);
                break;
            }
        }
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[ToggleEventPanel] EventPanel 비활성화 및 씬 저장 완료");
    }
}
