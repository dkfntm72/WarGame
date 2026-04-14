using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AddSettingsButton
{
    public static void Execute()
    {
        // ── 기존 버튼 제거 ─────────────────────────────────────
        var existing = GameObject.Find("UI/HUD/SettingsButton");
        if (existing != null) Object.DestroyImmediate(existing);

        // ── 부모 찾기 ─────────────────────────────────────────
        var hudGO = GameObject.Find("UI/HUD");
        if (hudGO == null) { Debug.LogError("[WarGame] UI/HUD 를 찾을 수 없습니다."); return; }

        // ── 아이콘 스프라이트 로드 ─────────────────────────────
        var iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny Swords/UI Elements/Icons/Icon_10.png");

        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/경기천년제목OTF_Bold SDF.asset");

        var outlineMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Fonts/경기천년제목OTF_Bold SDF - Outline.mat");

        // ── 버튼 GameObject ────────────────────────────────────
        var btnGO = new GameObject("SettingsButton");
        btnGO.transform.SetParent(hudGO.transform, false);

        var rt             = btnGO.AddComponent<RectTransform>();
        rt.anchorMin       = new Vector2(1f, 0f);
        rt.anchorMax       = new Vector2(1f, 1f);
        rt.pivot           = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-10f, 0f);
        rt.sizeDelta       = new Vector2(65f, -10f);

        var img = btnGO.AddComponent<Image>();
        if (iconSprite != null)
        {
            img.sprite          = iconSprite;
            img.type            = Image.Type.Simple;
            img.preserveAspect  = true;
            img.color           = Color.white;
        }
        else
        {
            img.color = new Color(0.2f, 0.2f, 0.3f, 0.85f);

            // 아이콘 없으면 "설정" 텍스트 표시
            var lbl = new GameObject("Label");
            lbl.transform.SetParent(btnGO.transform, false);
            var lRT       = lbl.AddComponent<RectTransform>();
            lRT.anchorMin = Vector2.zero;
            lRT.anchorMax = Vector2.one;
            lRT.sizeDelta = Vector2.zero;
            var tmp       = lbl.AddComponent<TextMeshProUGUI>();
            tmp.text      = "설정";
            tmp.fontSize  = 20;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            if (fontAsset  != null) tmp.font               = fontAsset;
            if (outlineMat != null) tmp.fontSharedMaterial  = outlineMat;
        }

        var btn = btnGO.AddComponent<Button>();
        var cb  = btn.colors;
        cb.highlightedColor = new Color(1f, 1f, 0.7f, 1f);
        cb.pressedColor     = new Color(0.6f, 0.6f, 0.6f, 1f);
        btn.colors = cb;

        // ── GameUI.settingsToggleButton 연결 ──────────────────
        var gameUI = Object.FindFirstObjectByType<GameUI>();
        if (gameUI != null)
        {
            gameUI.settingsToggleButton = btn;
            EditorUtility.SetDirty(gameUI);
            Debug.Log("[WarGame] GameUI.settingsToggleButton 연결 완료");
        }
        else
        {
            Debug.LogWarning("[WarGame] GameUI 컴포넌트를 찾을 수 없습니다. settingsToggleButton 수동 연결 필요");
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[WarGame] SettingsButton 생성 완료 (UI/HUD/SettingsButton)");
    }
}
