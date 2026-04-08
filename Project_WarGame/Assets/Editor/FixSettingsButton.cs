using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.Events;

public class FixSettingsButton
{
    public static void Execute()
    {
        var uiRoot = GameObject.Find("UI");
        if (uiRoot == null) { Debug.LogError("[Fix] UI를 찾을 수 없습니다."); return; }

        var gameUI = uiRoot.GetComponent<GameUI>();

        // ── SettingsButton targetGraphic 연결 ──────────────────
        var settingsBtnGO = uiRoot.transform.Find("SettingsButton")?.gameObject;
        if (settingsBtnGO != null)
        {
            var btn = settingsBtnGO.GetComponent<Button>();
            var img = settingsBtnGO.GetComponent<Image>();
            if (btn != null && img != null)
            {
                var so = new SerializedObject(btn);
                so.FindProperty("m_TargetGraphic").objectReferenceValue = img;
                so.ApplyModifiedProperties();
                Debug.Log("[Fix] SettingsButton targetGraphic 연결 완료");
            }
        }

        // ── CloseButton targetGraphic 연결 ────────────────────
        var closeBtnGO = uiRoot.transform.Find("SettingsPanel/TopBar/CloseButton")?.gameObject;
        if (closeBtnGO != null)
        {
            var btn = closeBtnGO.GetComponent<Button>();
            var img = closeBtnGO.GetComponent<Image>();
            if (btn != null && img != null)
            {
                var so = new SerializedObject(btn);
                so.FindProperty("m_TargetGraphic").objectReferenceValue = img;
                so.ApplyModifiedProperties();
            }
        }

        // ── ExitButton targetGraphic 연결 ─────────────────────
        var exitBtnGO = uiRoot.transform.Find("SettingsPanel/ExitButton")?.gameObject;
        if (exitBtnGO != null)
        {
            var btn = exitBtnGO.GetComponent<Button>();
            var img = exitBtnGO.GetComponent<Image>();
            if (btn != null && img != null)
            {
                var so = new SerializedObject(btn);
                so.FindProperty("m_TargetGraphic").objectReferenceValue = img;
                so.ApplyModifiedProperties();
            }
        }

        // ── 모든 버튼 onClick Persistent Listener 재설정 ──────
        if (gameUI != null)
        {
            // SettingsButton
            if (settingsBtnGO != null)
            {
                var btn = settingsBtnGO.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                UnityEventTools.AddVoidPersistentListener(btn.onClick,
                    new UnityAction(gameUI.ToggleSettings));
            }

            // CloseButton
            if (closeBtnGO != null)
            {
                var btn = closeBtnGO.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                UnityEventTools.AddVoidPersistentListener(btn.onClick,
                    new UnityAction(gameUI.ToggleSettings));
            }

            // ExitButton
            if (exitBtnGO != null)
            {
                var btn = exitBtnGO.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                UnityEventTools.AddVoidPersistentListener(btn.onClick,
                    new UnityAction(gameUI.ExitToStageSelect));
            }

            Debug.Log("[Fix] 모든 버튼 onClick 연결 완료");
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
