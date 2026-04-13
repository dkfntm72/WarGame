using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BuildGameUI
{
    [UnityEditor.MenuItem("Window/WarGame/Build Stage01 UI")]
    public static void Execute()
    {
        // ── 기존 UI 제거 ──────────────────────────────────────
        var existing = GameObject.Find("UI");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

        // ── 에셋 로드 ─────────────────────────────────────────
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/경기천년제목OTF_Bold SDF.asset");

        // ── 아웃라인 머티리얼 생성/로드 ──────────────────────────
        Material outlineMat = null;
        if (fontAsset != null)
        {
            const string outlineMatPath = "Assets/Fonts/경기천년제목OTF_Bold SDF - Outline.mat";
            outlineMat = AssetDatabase.LoadAssetAtPath<Material>(outlineMatPath);
            if (outlineMat == null)
            {
                outlineMat = new Material(fontAsset.material);
                AssetDatabase.CreateAsset(outlineMat, outlineMatPath);
            }
            outlineMat.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
            outlineMat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.3f);
            AssetDatabase.SaveAssets();
        }

        Sprite hudSprite = null;
        foreach (var s in AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Tiny Swords/UI Elements/Papers/SpecialPaper.png"))
        {
            if (s is Sprite sp && sp.name == "SpecialPaper_0") { hudSprite = sp; break; }
        }

        Sprite btnSprite = null;
        foreach (var s in AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Tiny Swords/UI Elements/Buttons/SmallBlueSquareButton_Regular.png"))
        {
            if (s is Sprite sp && sp.name == "SmallBlueSquareButton_Regular_0") { btnSprite = sp; break; }
        }

        Sprite regularPaperSprite = null;
        foreach (var s in AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Tiny Swords/UI Elements/Papers/RegularPaper.png"))
        {
            if (s is Sprite sp && sp.name == "RegularPaper_0") { regularPaperSprite = sp; break; }
        }

        Sprite settingsIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Tiny Swords/UI Elements/Icons/Icon_10.png");

        // ── Canvas ────────────────────────────────────────────
        var canvasGO = new GameObject("UI");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── HUD (상단바) ──────────────────────────────────────
        var hud   = MakePanel(canvasGO, "HUD",
            new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f,1f),
            Vector2.zero, new Vector2(0, 120));
        var hudImg = hud.GetComponent<Image>();
        if (hudSprite != null) { hudImg.sprite = hudSprite; hudImg.type = Image.Type.Sliced; hudImg.color = Color.white; }
        else                   { hudImg.color  = new Color(0.08f, 0.06f, 0.12f, 0.92f); }

        // TurnText (좌)
        var turnText = MakeTMP(hud, "TurnText", "1턴 - 플레이어", fontAsset, 36,
            new Vector2(0,0), new Vector2(0.45f,1), new Vector2(20,-5), new Vector2(-10,-10));

        // GoldText (중앙)
        var goldText = MakeTMP(hud, "GoldText", "골드: 100", fontAsset, 36,
            new Vector2(0.3f,0), new Vector2(0.7f,1), new Vector2(0,-5), new Vector2(0,-10));
        goldText.alignment = TextAlignmentOptions.Center;

        // 설정 버튼 (우상단, 아이콘)
        var settingsBtn = MakeButtonBox(hud, "SettingsButton", "", fontAsset, 26,
            new Vector2(1,0.5f), new Vector2(1,0.5f), new Vector2(1,0.5f),
            new Vector2(-50, 0), new Vector2(80, 80),
            Color.white);
        if (settingsIconSprite != null)
        {
            var sImg = settingsBtn.GetComponent<Image>();
            sImg.sprite = settingsIconSprite;
            sImg.type   = Image.Type.Simple;
            sImg.preserveAspect = true;
        }

        // ── End Turn 버튼 (우하단) ────────────────────────────
        var endTurnBtn = MakeButtonBox(canvasGO, "EndTurnButton", "턴 종료", fontAsset, 28,
            new Vector2(1,0), new Vector2(1,0), new Vector2(0.5f,0),
            new Vector2(-110, 60), new Vector2(160, 160),
            Color.white);
        if (btnSprite != null)
        {
            var img = endTurnBtn.GetComponent<Image>();
            img.sprite = btnSprite;
            img.type   = Image.Type.Sliced;
        }

        // ── 유닛 정보 패널 (좌하단) ───────────────────────────
        var unitPanel = MakePanel(canvasGO, "UnitInfoPanel",
            new Vector2(0,0), new Vector2(0,0), new Vector2(0,0),
            new Vector2(15, 15), new Vector2(320, 170));
        SetImgSliced(unitPanel, regularPaperSprite);
        unitPanel.SetActive(false);

        var unitName   = MakeTMP(unitPanel, "UnitName",   "Unit Name", fontAsset, 26,
            new Vector2(0,1), new Vector2(1,1), new Vector2(10,-10), new Vector2(-20, 32));
        unitName.color = Color.black;
        var unitHp     = MakeTMP(unitPanel, "UnitHp",     "HP: -",     fontAsset, 20,
            new Vector2(0,1), new Vector2(1,1), new Vector2(10,-48), new Vector2(-20, 26));
        unitHp.color = Color.black;
        var unitStats  = MakeTMP(unitPanel, "UnitStats",  "",          fontAsset, 17,
            new Vector2(0,1), new Vector2(1,1), new Vector2(10,-80), new Vector2(-20, 52));
        unitStats.enableWordWrapping = false;
        unitStats.color = Color.black;
        var unitStatus = MakeTMP(unitPanel, "UnitStatus", "",          fontAsset, 18,
            new Vector2(0,1), new Vector2(1,1), new Vector2(10,-138), new Vector2(-20, 26));

        // ── 건물 패널 (hidden, 화면 중앙 약간 위) ────────────────
        var buildingPanel = MakePanel(canvasGO, "BuildingPanel",
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(0, -300), new Vector2(770, 280));
        SetImgSliced(buildingPanel, hudSprite);
        buildingPanel.SetActive(false);

        MakeTMP(buildingPanel, "BuildingTitle", "유닛 훈련", fontAsset, 22,
            new Vector2(0,1), new Vector2(1,1), new Vector2(10,-8), new Vector2(-20, 28));

        var containerGO = MakePanel(buildingPanel, "UnitButtonContainer",
            new Vector2(0,0), new Vector2(1,1), new Vector2(0,0),
            new Vector2(0, 0), new Vector2(0, 0));
        containerGO.GetComponent<Image>().color = Color.clear;
        var hlg = containerGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8; hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(8,8,4,4);
        hlg.childAlignment = TextAnchor.MiddleLeft;

        // ── 승리/패배 패널 ────────────────────────────────────
        var victoryPanel = MakePanel(canvasGO, "VictoryPanel",
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            Vector2.zero, new Vector2(600, 320));
        SetImgSliced(victoryPanel, hudSprite);
        var vt = MakeTMP(victoryPanel, "VictoryText", "승리!", fontAsset, 72,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        vt.alignment = TextAlignmentOptions.Center;
        victoryPanel.SetActive(false);

        var defeatPanel = MakePanel(canvasGO, "DefeatPanel",
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            Vector2.zero, new Vector2(600, 320));
        SetImgSliced(defeatPanel, hudSprite);
        var dt = MakeTMP(defeatPanel, "DefeatText", "패배...", fontAsset, 72,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        dt.alignment = TextAlignmentOptions.Center;
        defeatPanel.SetActive(false);

        // ── 설정 패널 ─────────────────────────────────────────
        var settingsPanel = MakePanel(canvasGO, "SettingsPanel",
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            Vector2.zero, new Vector2(500, 300));
        SetImgSliced(settingsPanel, hudSprite);
        settingsPanel.SetActive(false);

        MakeTMP(settingsPanel, "SettingsTitle", "설정", fontAsset, 36,
            new Vector2(0,1), new Vector2(1,1), new Vector2(0,-15), new Vector2(0, 50));

        // 볼륨 슬라이더
        var sliderGO = new GameObject("VolumeSlider");
        sliderGO.transform.SetParent(settingsPanel.transform, false);
        var sliderRT = sliderGO.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.1f,0.5f); sliderRT.anchorMax = new Vector2(0.9f,0.5f);
        sliderRT.anchoredPosition = new Vector2(0, 10); sliderRT.sizeDelta = new Vector2(0, 30);
        var slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0; slider.maxValue = 1; slider.value = 1;

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.sizeDelta = Vector2.zero;
        bgGO.AddComponent<Image>().color = new Color(0.3f,0.3f,0.3f,1f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        var faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
        faRT.sizeDelta = new Vector2(-20, 0); faRT.anchoredPosition = new Vector2(-5,0);
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillArea.transform, false);
        var fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(1,1); fillRT.sizeDelta = new Vector2(10,0);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.9f, 0.7f, 0.2f, 1f);
        slider.fillRect = fillRT;

        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGO.transform, false);
        var haRT = handleArea.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one;
        haRT.sizeDelta = new Vector2(-20,0);
        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(handleArea.transform, false);
        var handleRT = handleGO.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(30,0);
        handleGO.AddComponent<Image>().color = Color.white;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleGO.GetComponent<Image>();

        // 볼륨 라벨
        MakeTMP(settingsPanel, "VolumeLabel", "음량", fontAsset, 24,
            new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.5f),
            new Vector2(0, 50), new Vector2(0, 30));

        // 나가기 버튼
        var exitBtn = MakeButtonBox(settingsPanel, "ExitButton", "스테이지 선택", fontAsset, 24,
            new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0),
            new Vector2(0, 25), new Vector2(220, 55),
            new Color(0.55f, 0.15f, 0.15f, 1f));
        exitBtn.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("StageSelect"));

        // ── 이벤트 팝업 패널 ──────────────────────────────────
        var eventPanel = MakePanel(canvasGO, "EventPanel",
            Vector2.zero, Vector2.one, new Vector2(0.5f,0.5f),
            Vector2.zero, Vector2.zero);
        SetImgColor(eventPanel, new Color(0,0,0,0.55f));
        eventPanel.SetActive(false);

        var eventTextGO = MakePanel(eventPanel, "EventTextBox",
            new Vector2(0.1f,0.3f), new Vector2(0.9f,0.7f), new Vector2(0.5f,0.5f),
            Vector2.zero, Vector2.zero);
        SetImgColor(eventTextGO, new Color(0.06f, 0.05f, 0.10f, 0.95f));
        var eventText = MakeTMP(eventTextGO, "EventText", "", fontAsset, 32,
            Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-30,-30));
        eventText.alignment = TextAlignmentOptions.Center;
        eventText.enableWordWrapping = true;

        // ── GameUI 컴포넌트 연결 ──────────────────────────────
        var gameUI = canvasGO.AddComponent<GameUI>();
        gameUI.turnText             = turnText;
        gameUI.goldText             = goldText;
        gameUI.endTurnButton        = endTurnBtn;
        gameUI.unitInfoPanel        = unitPanel;
        gameUI.unitNameText         = unitName;
        gameUI.unitHpText           = unitHp;
        gameUI.unitStatsText        = unitStats;
        gameUI.unitStatusText       = unitStatus;
        gameUI.buildingPanel        = buildingPanel;
        gameUI.unitButtonContainer  = containerGO.transform;
        gameUI.victoryPanel         = victoryPanel;
        gameUI.defeatPanel          = defeatPanel;
        gameUI.settingsToggleButton = settingsBtn;
        gameUI.settingsPanel        = settingsPanel;
        gameUI.volumeSlider         = slider;
        gameUI.eventPanel           = eventPanel;
        gameUI.eventText            = eventText;

        // UnitButton 프리팹 연결
        var unitBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UnitButton.prefab");
        gameUI.unitButtonPrefab = unitBtnPrefab;

        // ── 아웃라인 머티리얼 일괄 적용 ──────────────────────────
        if (outlineMat != null)
        {
            foreach (var tmp in canvasGO.GetComponentsInChildren<TextMeshProUGUI>(true))
                tmp.fontSharedMaterial = outlineMat;
        }

        // ── 씬 저장 ───────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[BuildGameUI] Canvas UI 생성 완료!");
    }

    // ── 헬퍼 ──────────────────────────────────────────────────
    static GameObject MakePanel(GameObject parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 pivot, Vector2 ancPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax; rt.pivot = pivot;
        rt.anchoredPosition = ancPos; rt.sizeDelta = sizeDelta;
        go.AddComponent<Image>().color = Color.clear;
        return go;
    }

    static void SetImgColor(GameObject go, Color c)
    {
        var img = go.GetComponent<Image>(); if (img == null) img = go.AddComponent<Image>();
        img.color = c;
    }

    static void SetImgSliced(GameObject go, Sprite sprite)
    {
        var img = go.GetComponent<Image>(); if (img == null) img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type   = Image.Type.Sliced;
        img.color  = Color.white;
    }

    static TextMeshProUGUI MakeTMP(GameObject parent, string name, string text,
        TMP_FontAsset font, int fontSize,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancPos, Vector2 sizeDelta,
        TextAlignmentOptions align = TextAlignmentOptions.Left)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.anchoredPosition = ancPos; rt.sizeDelta = sizeDelta;
        rt.pivot = new Vector2(0, 1);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.color = Color.white; tmp.alignment = align;
        if (font != null) tmp.font = font;
        return tmp;
    }

    static Button MakeButton(GameObject parent, string name, string label, TMP_FontAsset font, int fontSize,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancMin2, Vector2 ancMax2, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = ancMin2; rt.offsetMax = ancMax2;
        var img = go.AddComponent<Image>(); img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var lbl = new GameObject("Label"); lbl.transform.SetParent(go.transform, false);
        var lrt = lbl.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.sizeDelta = Vector2.zero;
        var tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        return btn;
    }

    static Button MakeButtonBox(GameObject parent, string name, string label, TMP_FontAsset font, int fontSize,
        Vector2 ancMin, Vector2 ancMax, Vector2 pivot, Vector2 ancPos, Vector2 size, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax; rt.pivot = pivot;
        rt.anchoredPosition = ancPos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var lbl = new GameObject("Label"); lbl.transform.SetParent(go.transform, false);
        var lrt = lbl.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.sizeDelta = Vector2.zero;
        var tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        return btn;
    }
}
