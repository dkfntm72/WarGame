using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixSettingsPanel
{
    public static void Execute()
    {
        var uiGO   = GameObject.Find("UI");
        if (uiGO == null) { Debug.LogError("[WarGame] UI 오브젝트를 찾을 수 없습니다."); return; }

        var gameUI = uiGO.GetComponent<GameUI>();
        if (gameUI == null) { Debug.LogError("[WarGame] GameUI 컴포넌트를 찾을 수 없습니다."); return; }

        // ── SettingsPanel 씬 인스턴스로 재연결 ────────────────
        GameObject settingsPanel = null;
        foreach (var t in uiGO.GetComponentsInChildren<Transform>(true))
            if (t.name == "SettingsPanel" && t.parent == uiGO.transform)
            { settingsPanel = t.gameObject; break; }

        if (settingsPanel == null) { Debug.LogError("[WarGame] UI/SettingsPanel 을 찾을 수 없습니다."); return; }

        gameUI.settingsPanel = settingsPanel;
        Debug.Log("[WarGame] settingsPanel → UI/SettingsPanel 연결 완료");

        // ── VolumeSlider 연결 ──────────────────────────────────
        var slider = settingsPanel.GetComponentInChildren<Slider>(true);
        if (slider != null)
        {
            gameUI.volumeSlider = slider;
            Debug.Log("[WarGame] volumeSlider 연결 완료");
        }

        // ── 닫기(X) 버튼 추가 ─────────────────────────────────
        var existing = settingsPanel.transform.Find("CloseButton");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var closeIcon = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny Swords/UI Elements/Icons/Icon_09.png");

        var closeBtnGO = new GameObject("CloseButton");
        closeBtnGO.transform.SetParent(settingsPanel.transform, false);

        var rt             = closeBtnGO.AddComponent<RectTransform>();
        rt.anchorMin       = new Vector2(1f, 1f);
        rt.anchorMax       = new Vector2(1f, 1f);
        rt.pivot           = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-8f, -8f);
        rt.sizeDelta       = new Vector2(44f, 44f);

        var img = closeBtnGO.AddComponent<Image>();
        if (closeIcon != null)
        {
            img.sprite         = closeIcon;
            img.type           = Image.Type.Simple;
            img.preserveAspect = true;
            img.color          = Color.white;
        }
        else img.color = new Color(0.8f, 0.1f, 0.1f, 1f);

        var btn = closeBtnGO.AddComponent<Button>();
        var cb  = btn.colors;
        cb.highlightedColor = new Color(1f, 0.6f, 0.6f, 1f);
        cb.pressedColor     = new Color(0.5f, 0.1f, 0.1f, 1f);
        btn.colors = cb;

        Debug.Log("[WarGame] CloseButton(X) 추가 완료");

        // ── settingsPanel 비활성 유지 ──────────────────────────
        settingsPanel.SetActive(false);

        EditorUtility.SetDirty(gameUI);
        EditorUtility.SetDirty(settingsPanel);
        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
