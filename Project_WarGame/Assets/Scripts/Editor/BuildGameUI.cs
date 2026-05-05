using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BuildGameUI
{
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
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── HUD (상단바) ──────────────────────────────────────
        var hud   = MakePanel(canvasGO, "HUD",
            new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f,0.5f),
            new Vector2(0,-40), new Vector2(0, 80));
        var hudImg = hud.GetComponent<Image>();
        if (hudSprite != null) { hudImg.sprite = hudSprite; hudImg.type = Image.Type.Sliced; hudImg.color = Color.white; }
        else                   { hudImg.color  = new Color(0.08f, 0.06f, 0.12f, 0.92f); }

        // TurnText (좌 절반, 수직 중앙)
        var turnText = MakeTMP(hud, "TurnText", "1턴 - 플레이어", fontAsset, 28,
            new Vector2(0,0.5f), new Vector2(0.5f,0.5f), new Vector2(20,0), new Vector2(-20,30));

        // GoldText (우 절반, 수직 중앙)
        var goldText = MakeTMP(hud, "GoldText", "골드: 100", fontAsset, 28,
            new Vector2(0.5f,0.5f), new Vector2(1f,0.5f), new Vector2(-20,0), new Vector2(-20,30));
        goldText.alignment = TextAlignmentOptions.Center;

        // 설정 버튼 (우측 세로 stretch)
        var settingsBtnGO = new GameObject("SettingsButton");
        settingsBtnGO.transform.SetParent(hud.transform, false);
        var sbRt2 = settingsBtnGO.AddComponent<RectTransform>();
        sbRt2.anchorMin        = new Vector2(1f, 0f);
        sbRt2.anchorMax        = new Vector2(1f, 1f);
        sbRt2.pivot            = new Vector2(1f, 0.5f);
        sbRt2.anchoredPosition = new Vector2(-10f, 0f);
        sbRt2.sizeDelta        = new Vector2(65f, -10f);
        var settingsBtn = settingsBtnGO.AddComponent<Button>();
        var sbImg = settingsBtnGO.AddComponent<Image>();
        if (settingsIconSprite != null) { sbImg.sprite = settingsIconSprite; sbImg.type = Image.Type.Simple; sbImg.preserveAspect = true; }
        else sbImg.color = Color.white;

        // ── End Turn 버튼 (우하단) ────────────────────────────
        var endTurnBtn = MakeButtonBox(canvasGO, "EndTurnButton", "턴 종료", fontAsset, 22,
            new Vector2(1,0), new Vector2(1,0), new Vector2(0.5f,0.5f),
            new Vector2(-160, 80), new Vector2(150, 60),
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
            new Vector2(20, 20), new Vector2(280, 130));
        SetImgSliced(unitPanel, regularPaperSprite);
        unitPanel.SetActive(false);

        var unitName   = MakeTMP(unitPanel, "UnitName",   "Unit Name", fontAsset, 22,
            new Vector2(0,1), new Vector2(1,1), new Vector2(10,-8),  new Vector2(-20, 28));
        unitName.color = Color.black;
        var unitHp     = MakeTMP(unitPanel, "UnitHp",     "HP: -",     fontAsset, 18,
            new Vector2(0,1), new Vector2(1,1), new Vector2(10,-40), new Vector2(-20, 24));
        unitHp.color = Color.black;
        var unitStats  = MakeTMP(unitPanel, "UnitStats",  "",          fontAsset, 15,
            new Vector2(0,1), new Vector2(1,1), new Vector2(10,-68), new Vector2(-20, 44));
        unitStats.enableWordWrapping = false;
        unitStats.color = Color.black;
        var unitStatus = MakeTMP(unitPanel, "UnitStatus", "",          fontAsset, 16,
            new Vector2(0,1), new Vector2(1,1), new Vector2(10,-116), new Vector2(-20, 24));

        // ── 건물 패널 (hidden) ────────────────────────────────
        var buildingPanel = MakePanel(canvasGO, "BuildingPanel",
            new Vector2(0,0), new Vector2(0,0), new Vector2(0,0),
            new Vector2(-180, 20), new Vector2(320, 180));
        SetImgSliced(buildingPanel, hudSprite);
        buildingPanel.SetActive(false);

        MakeTMP(buildingPanel, "BuildingTitle", "유닛 훈련", fontAsset, 18,
            new Vector2(0,1), new Vector2(1,1), new Vector2(8,-6), new Vector2(-16, 24));

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
            Vector2.zero, new Vector2(500, 300));
        SetImgSliced(victoryPanel, hudSprite);
        var vt = MakeTMP(victoryPanel, "VictoryText", "승리!", fontAsset, 64,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        vt.alignment = TextAlignmentOptions.Center;
        victoryPanel.SetActive(false);

        var defeatPanel = MakePanel(canvasGO, "DefeatPanel",
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            Vector2.zero, new Vector2(500, 300));
        SetImgSliced(defeatPanel, hudSprite);
        var dt = MakeTMP(defeatPanel, "DefeatText", "패배...", fontAsset, 64,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        dt.alignment = TextAlignmentOptions.Center;
        defeatPanel.SetActive(false);

        // ── 설정 패널 (프리팹 인스턴스화) ────────────────────────
        var settingsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SettingsPanel.prefab");
        if (settingsPrefab == null)
        {
            Debug.LogWarning("[BuildGameUI] Assets/Prefabs/SettingsPanel.prefab 없음 — 설정 패널 생략");
            return;
        }
        var settingsPanel = (GameObject)PrefabUtility.InstantiatePrefab(settingsPrefab, canvasGO.transform);
        settingsPanel.SetActive(false);

        var slider = settingsPanel.GetComponentInChildren<UnityEngine.UI.Slider>();

        // ── 대화창 (하단 고정 바) ─────────────────────────────
        var eventPanel = new GameObject("EventPanel");
        eventPanel.transform.SetParent(canvasGO.transform, false);
        var epRt = eventPanel.AddComponent<RectTransform>();
        epRt.anchorMin        = new Vector2(0.03f, 0f);
        epRt.anchorMax        = new Vector2(0.97f, 0f);
        epRt.pivot            = new Vector2(0.5f, 0f);
        epRt.anchoredPosition = new Vector2(0f, 15f);
        epRt.sizeDelta        = new Vector2(0f, 185f);
        var epCanvas = eventPanel.AddComponent<Canvas>();
        epCanvas.overrideSorting = true;
        epCanvas.sortingOrder   = 999;
        eventPanel.AddComponent<GraphicRaycaster>();
        eventPanel.AddComponent<Image>().color = new Color(0.08f, 0.06f, 0.03f, 0.96f);

        // 상단 황금 테두리
        var borderTop = new GameObject("BorderTop");
        borderTop.transform.SetParent(eventPanel.transform, false);
        var btRt = borderTop.AddComponent<RectTransform>();
        btRt.anchorMin        = new Vector2(0f, 1f);
        btRt.anchorMax        = new Vector2(1f, 1f);
        btRt.pivot            = new Vector2(0.5f, 1f);
        btRt.anchoredPosition = Vector2.zero;
        btRt.sizeDelta        = new Vector2(0f, 3f);
        borderTop.AddComponent<Image>().color = new Color(0.9f, 0.75f, 0.2f, 1f);

        // 화자 탭 (SpeakerBox)
        var speakerBox = new GameObject("SpeakerBox");
        speakerBox.transform.SetParent(eventPanel.transform, false);
        var sbRt = speakerBox.AddComponent<RectTransform>();
        sbRt.anchorMin        = new Vector2(0f, 1f);
        sbRt.anchorMax        = new Vector2(0f, 1f);
        sbRt.pivot            = new Vector2(0f, 0f);
        sbRt.anchoredPosition = new Vector2(20f, 0f);
        sbRt.sizeDelta        = new Vector2(180f, 40f);
        speakerBox.AddComponent<Image>().color = new Color(0.08f, 0.06f, 0.03f, 1f);
        var speakerTmp = MakeTMP(speakerBox, "SpeakerName", "", fontAsset, 22,
            Vector2.zero, Vector2.one, new Vector2(8f, -4f), new Vector2(-16f, -8f),
            TextAlignmentOptions.Center);
        speakerTmp.color = new Color(0.95f, 0.85f, 0.5f, 1f);
        speakerBox.SetActive(false);

        // 대사 본문
        var eventText = MakeTMP(eventPanel, "EventText", "", fontAsset, 28,
            Vector2.zero, Vector2.one, new Vector2(24f, -35f), new Vector2(-48f, -55f));
        eventText.enableWordWrapping = true;

        // 힌트 텍스트 (▼ 클릭하여 계속)
        var hintGO = new GameObject("HintText");
        hintGO.transform.SetParent(eventPanel.transform, false);
        var hintRt = hintGO.AddComponent<RectTransform>();
        hintRt.anchorMin        = new Vector2(1f, 0f);
        hintRt.anchorMax        = new Vector2(1f, 0f);
        hintRt.pivot            = new Vector2(1f, 0f);
        hintRt.anchoredPosition = new Vector2(-16f, 12f);
        hintRt.sizeDelta        = new Vector2(200f, 28f);
        var hintTmp = hintGO.AddComponent<TextMeshProUGUI>();
        hintTmp.text      = "▼ 클릭하여 계속";
        hintTmp.fontSize  = 18;
        hintTmp.color     = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        hintTmp.alignment = TextAlignmentOptions.Right;
        if (fontAsset != null) hintTmp.font = fontAsset;

        eventPanel.SetActive(false);

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
        gameUI.dialogueSpeakerText  = speakerTmp;

        // SettingsPanel 내 승패조건 텍스트 연결
        var covTmp = settingsPanel
            .transform.Find("ConditionsOfVictoryText/COV")
            ?.GetComponent<TextMeshProUGUI>();
        gameUI.conditionsOfVictoryText = covTmp;

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
