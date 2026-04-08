using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class CreateSettingsPanel
{
    public static void Execute()
    {
        var uiRoot = GameObject.Find("UI");
        if (uiRoot == null) { Debug.LogError("[Settings] UI 오브젝트를 찾을 수 없습니다."); return; }

        var canvas = uiRoot.GetComponent<Canvas>();
        var gameUI = uiRoot.GetComponent<GameUI>();

        // 폰트 로드
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/경기천년제목OTF_Bold SDF.asset");

        // ── 기존 오브젝트 정리 ─────────────────────────────────
        DestroyIfExists(uiRoot, "SettingsButton");
        DestroyIfExists(uiRoot, "SettingsPanel");

        // ── 색상 팔레트 ────────────────────────────────────────
        var panelBg      = new Color(0.08f, 0.10f, 0.14f, 0.95f);
        var accentColor  = new Color(0.90f, 0.75f, 0.30f, 1f);   // 골드
        var btnNormal    = new Color(0.20f, 0.24f, 0.32f, 1f);
        var btnExit      = new Color(0.65f, 0.18f, 0.18f, 1f);
        var textWhite    = Color.white;

        // ══════════════════════════════════════════════════════
        // 1. 설정 버튼 (톱니 ⚙, 우상단)
        // ══════════════════════════════════════════════════════
        var settingsBtnGO = MakeRect("SettingsButton", uiRoot.transform);
        SetAnchors(settingsBtnGO, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        SetSizeDelta(settingsBtnGO, 56, 56);
        SetAnchoredPos(settingsBtnGO, -12, -12);

        // 배경 이미지
        var btnImg = settingsBtnGO.AddComponent<Image>();
        btnImg.color = new Color(0.12f, 0.15f, 0.20f, 0.90f);

        // 버튼 컴포넌트
        var settingsBtn = settingsBtnGO.AddComponent<Button>();
        var btnColors = settingsBtn.colors;
        btnColors.normalColor      = new Color(0.12f, 0.15f, 0.20f, 0.90f);
        btnColors.highlightedColor = new Color(0.25f, 0.30f, 0.40f, 1f);
        btnColors.pressedColor     = new Color(0.08f, 0.10f, 0.14f, 1f);
        settingsBtn.colors = btnColors;

        // ⚙ 텍스트
        var gearTmp = MakeTMP("GearIcon", settingsBtnGO.transform, "⚙", 28, font);
        SetAnchors(gearTmp.gameObject, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(gearTmp.gameObject, 0, 0, 0, 0);
        gearTmp.alignment = TextAlignmentOptions.Center;
        gearTmp.color     = accentColor;

        // ══════════════════════════════════════════════════════
        // 2. 설정 패널
        // ══════════════════════════════════════════════════════
        var panelGO = MakeRect("SettingsPanel", uiRoot.transform);
        SetAnchors(panelGO, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        SetSizeDelta(panelGO, 280, 220);
        SetAnchoredPos(panelGO, -12, -76);   // 버튼 바로 아래

        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = panelBg;

        // ── 상단 바 (타이틀 + 닫기 버튼) ──────────────────────
        var topBar = MakeRect("TopBar", panelGO.transform);
        SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetSizeDelta(topBar, 0, 44);
        SetAnchoredPos(topBar, 0, 0);

        var topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = new Color(0.05f, 0.07f, 0.10f, 1f);

        // 타이틀 "설정"
        var titleTmp = MakeTMP("TitleText", topBar.transform, "설정", 20, font);
        SetAnchors(titleTmp.gameObject, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(titleTmp.gameObject, 16, 0, 48, 0);
        titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
        titleTmp.color     = accentColor;

        // ✕ 닫기 버튼
        var closeBtnGO = MakeRect("CloseButton", topBar.transform);
        SetAnchors(closeBtnGO, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
        SetSizeDelta(closeBtnGO, 40, 0);
        SetAnchoredPos(closeBtnGO, 0, 0);
        var closeImg = closeBtnGO.AddComponent<Image>();
        closeImg.color = Color.clear;
        var closeBtn = closeBtnGO.AddComponent<Button>();
        var closeTmp = MakeTMP("CloseText", closeBtnGO.transform, "✕", 18, font);
        SetAnchors(closeTmp.gameObject, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(closeTmp.gameObject, 0, 0, 0, 0);
        closeTmp.alignment = TextAlignmentOptions.Center;
        closeTmp.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        var closeBtnColors = closeBtn.colors;
        closeBtnColors.highlightedColor = new Color(1f, 0.3f, 0.3f, 0.3f);
        closeBtn.colors = closeBtnColors;

        // ── 음량 레이블 + 슬라이더 ────────────────────────────
        var volumeLabel = MakeTMP("VolumeLabel", panelGO.transform, "음량", 16, font);
        SetAnchors(volumeLabel.gameObject, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetSizeDelta(volumeLabel.gameObject, 0, 30);
        SetAnchoredPos(volumeLabel.gameObject, 0, -56);
        var volLabelRT = volumeLabel.GetComponent<RectTransform>();
        volLabelRT.offsetMin = new Vector2(16, volLabelRT.offsetMin.y);
        volLabelRT.offsetMax = new Vector2(-16, volLabelRT.offsetMax.y);
        volumeLabel.alignment = TextAlignmentOptions.MidlineLeft;
        volumeLabel.color = textWhite;

        var sliderGO = MakeRect("VolumeSlider", panelGO.transform);
        SetAnchors(sliderGO, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        SetSizeDelta(sliderGO, 0, 28);
        SetAnchoredPos(sliderGO, 0, -90);
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.offsetMin = new Vector2(16, sliderRT.offsetMin.y);
        sliderRT.offsetMax = new Vector2(-16, sliderRT.offsetMax.y);

        var slider = BuildSlider(sliderGO, accentColor);

        // ── 나가기 버튼 ────────────────────────────────────────
        var exitBtnGO = MakeRect("ExitButton", panelGO.transform);
        SetAnchors(exitBtnGO, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
        SetSizeDelta(exitBtnGO, 0, 44);
        SetAnchoredPos(exitBtnGO, 0, 16);
        var exitRT = exitBtnGO.GetComponent<RectTransform>();
        exitRT.offsetMin = new Vector2(16, exitRT.offsetMin.y);
        exitRT.offsetMax = new Vector2(-16, exitRT.offsetMax.y);

        var exitImg = exitBtnGO.AddComponent<Image>();
        exitImg.color = btnExit;
        var exitBtn = exitBtnGO.AddComponent<Button>();
        var exitColors = exitBtn.colors;
        exitColors.normalColor      = btnExit;
        exitColors.highlightedColor = new Color(0.80f, 0.25f, 0.25f, 1f);
        exitColors.pressedColor     = new Color(0.50f, 0.10f, 0.10f, 1f);
        exitBtn.colors = exitColors;

        var exitTmp = MakeTMP("ExitText", exitBtnGO.transform, "스테이지 선택으로", 17, font);
        SetAnchors(exitTmp.gameObject, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(exitTmp.gameObject, 0, 0, 0, 0);
        exitTmp.alignment = TextAlignmentOptions.Center;
        exitTmp.color = textWhite;

        // ══════════════════════════════════════════════════════
        // 3. GameUI 필드 연결
        // ══════════════════════════════════════════════════════
        if (gameUI != null)
        {
            var so = new SerializedObject(gameUI);
            so.FindProperty("settingsToggleButton").objectReferenceValue = settingsBtn;
            so.FindProperty("settingsPanel").objectReferenceValue        = panelGO;
            so.FindProperty("volumeSlider").objectReferenceValue         = slider;
            so.ApplyModifiedProperties();
        }

        // ══════════════════════════════════════════════════════
        // 4. 버튼 이벤트 연결
        // ══════════════════════════════════════════════════════
        // 설정 열기/닫기
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            settingsBtn.onClick,
            new UnityEngine.Events.UnityAction(gameUI.ToggleSettings));

        // 닫기(✕)
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            closeBtn.onClick,
            new UnityEngine.Events.UnityAction(gameUI.ToggleSettings));

        // 나가기
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            exitBtn.onClick,
            new UnityEngine.Events.UnityAction(gameUI.ExitToStageSelect));

        // 패널 기본 비활성
        panelGO.SetActive(false);

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Settings] 설정 패널 생성 완료. 씬을 저장하세요.");
    }

    // ── 헬퍼 ──────────────────────────────────────────────────
    static void DestroyIfExists(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }

    static GameObject MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static TextMeshProUGUI MakeTMP(string name, Transform parent, string text,
                                    float size, TMP_FontAsset font)
    {
        var go  = MakeRect(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = size;
        if (font != null) tmp.font = font;
        return tmp;
    }

    static void SetAnchors(GameObject go, Vector2 min, Vector2 max, Vector2 pivot)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max; rt.pivot = pivot;
    }

    static void SetSizeDelta(GameObject go, float w, float h) =>
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);

    static void SetAnchoredPos(GameObject go, float x, float y) =>
        go.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

    static void SetOffsets(GameObject go, float left, float bottom, float right, float top)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    static Slider BuildSlider(GameObject go, Color fillColor)
    {
        // Background
        var bgGO = MakeRect("Background", go.transform);
        SetAnchors(bgGO, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(bgGO, 0, 4, 0, 4);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Fill Area
        var fillArea = MakeRect("Fill Area", go.transform);
        SetAnchors(fillArea, new Vector2(0, 0.25f), new Vector2(1, 0.75f), new Vector2(0.5f, 0.5f));
        fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(5, 0);
        fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(-15, 0);

        var fillGO = MakeRect("Fill", fillArea.transform);
        SetAnchors(fillGO, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(fillGO, 0, 0, 0, 0);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = fillColor;

        // Handle Slide Area
        var handleArea = MakeRect("Handle Slide Area", go.transform);
        SetAnchors(handleArea, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        SetOffsets(handleArea, 10, 0, 10, 0);

        var handleGO = MakeRect("Handle", handleArea.transform);
        SetAnchors(handleGO, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0.5f, 0.5f));
        SetSizeDelta(handleGO, 20, 0);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.white;

        // Slider component
        var slider         = go.AddComponent<Slider>();
        slider.fillRect    = fillGO.GetComponent<RectTransform>();
        slider.handleRect  = handleGO.GetComponent<RectTransform>();
        slider.targetGraphic = handleImg;
        slider.minValue    = 0f;
        slider.maxValue    = 1f;
        slider.value       = 1f;
        slider.wholeNumbers = false;
        slider.direction   = Slider.Direction.LeftToRight;

        return slider;
    }
}
