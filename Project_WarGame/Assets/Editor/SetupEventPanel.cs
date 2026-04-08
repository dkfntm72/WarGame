using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class SetupEventPanel
{
    // 비활성 오브젝트를 포함해 이름으로 찾기
    static GameObject FindInactive(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.name == name && go.scene.IsValid()) return go;
        return null;
    }

    public static void Execute()
    {
        var eventPanelGO  = FindInactive("EventPanel");
        var eventTextGO   = FindInactive("EventText");
        var btnGO         = FindInactive("EventConfirmButton");
        var uiGO          = GameObject.Find("UI") ?? FindInactive("UI");

        if (eventPanelGO == null) { Debug.LogError("[Setup] EventPanel을 찾을 수 없습니다."); return; }
        if (eventTextGO  == null) { Debug.LogError("[Setup] EventText를 찾을 수 없습니다."); return; }
        if (btnGO        == null) { Debug.LogError("[Setup] EventConfirmButton을 찾을 수 없습니다."); return; }

        // EventText: 기존 Text 제거 후 TMP 추가
        var legacyText = eventTextGO.GetComponent<Text>();
        if (legacyText != null) Object.DestroyImmediate(legacyText);

        var tmp = eventTextGO.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = eventTextGO.AddComponent<TextMeshProUGUI>();
        tmp.text             = "";
        tmp.fontSize         = 20;
        tmp.alignment        = TextAlignmentOptions.Center;
        tmp.color            = Color.white;
        tmp.enableWordWrapping = true;
        EditorUtility.SetDirty(eventTextGO);

        // EventPanel 배경색 + 기본 비활성화
        var img = eventPanelGO.GetComponent<Image>();
        if (img != null) img.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        eventPanelGO.SetActive(false);
        EditorUtility.SetDirty(eventPanelGO);

        // GameUI에 필드 연결
        if (uiGO != null)
        {
            var gameUI = uiGO.GetComponent<GameUI>();
            if (gameUI != null)
            {
                gameUI.eventPanel         = eventPanelGO;
                gameUI.eventText          = tmp;
                gameUI.eventConfirmButton = btnGO.GetComponent<Button>();
                EditorUtility.SetDirty(uiGO);
            }
            else Debug.LogError("[Setup] UI 오브젝트에 GameUI 컴포넌트가 없습니다.");
        }
        else Debug.LogError("[Setup] UI 오브젝트를 찾을 수 없습니다.");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        var gameUI2 = uiGO?.GetComponent<GameUI>();
        Debug.Log($"[Setup] 완료 — eventPanel={gameUI2?.eventPanel != null}, " +
                  $"eventText={gameUI2?.eventText != null}, " +
                  $"eventConfirmButton={gameUI2?.eventConfirmButton != null}");
    }
}
