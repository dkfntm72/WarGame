using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Window > WarGame > Setup Stage01 Scene
/// Stage01 씬을 생성하고 모든 오브젝트/컴포넌트/레퍼런스를 자동으로 세팅합니다.
/// </summary>
public static class WarGameSceneSetup
{
    [MenuItem("Window/WarGame/Setup Stage01 Scene")]
    public static void SetupScene()
    {
        // ── 1. 새 씬 생성 ─────────────────────────────────────
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "Stage01";

        // ── 2. Main Camera 세팅 ───────────────────────────────
        var cam = GameObject.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.gameObject.tag = "MainCamera";
        }
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(4.5f, 3.5f, -10f); // 10x8 맵 중앙
        cam.backgroundColor  = new Color(0.1f, 0.1f, 0.15f);

        // ── 3. Grid / Tilemap 구조 생성 ───────────────────────
        var gridGO = new GameObject("Grid");
        var grid   = gridGO.AddComponent<Grid>();
        grid.cellSize = Vector3.one;

        // TerrainTilemap
        var terrainGO  = new GameObject("TerrainTilemap");
        terrainGO.transform.SetParent(gridGO.transform);
        var terrainTM  = terrainGO.AddComponent<Tilemap>();
        var terrainTMR = terrainGO.AddComponent<TilemapRenderer>();
        terrainTMR.sortingOrder = 0;

        // HighlightTilemap (이동/공격 범위 표시)
        var highlightGO  = new GameObject("HighlightTilemap");
        highlightGO.transform.SetParent(gridGO.transform);
        var highlightTM  = highlightGO.AddComponent<Tilemap>();
        var highlightTMR = highlightGO.AddComponent<TilemapRenderer>();
        highlightTMR.sortingOrder = 3;

        // GridManager
        var gmGO = new GameObject("GridManager");
        var gm   = gmGO.AddComponent<GridManager>();
        gm.grid              = grid;
        gm.terrainTilemap    = terrainTM;
        gm.highlightTilemap  = highlightTM;

        // ── 4. 매니저 오브젝트 생성 ───────────────────────────
        var managers = new GameObject("Managers");

        var turnMgrGO = new GameObject("TurnManager");
        turnMgrGO.transform.SetParent(managers.transform);
        turnMgrGO.AddComponent<TurnManager>();

        var resMgrGO = new GameObject("ResourceManager");
        resMgrGO.transform.SetParent(managers.transform);
        resMgrGO.AddComponent<ResourceManager>();

        var aiGO = new GameObject("EnemyAI");
        aiGO.transform.SetParent(managers.transform);
        aiGO.AddComponent<EnemyAI>();

        var inputGO = new GameObject("PlayerInputHandler");
        inputGO.transform.SetParent(managers.transform);
        inputGO.AddComponent<PlayerInputHandler>();

        // GameManager (마지막 생성 - 다른 싱글턴 먼저 있어야 함)
        var gameMgrGO = new GameObject("GameManager");
        gameMgrGO.transform.SetParent(managers.transform);
        var gameMgr = gameMgrGO.AddComponent<GameManager>();

        // GameManager 레퍼런스 연결
        var settings   = AssetDatabase.LoadAssetAtPath<GameSettings>("Assets/GameData/GameSettings.asset");
        var mapData    = AssetDatabase.LoadAssetAtPath<MapData>("Assets/GameData/Maps/Stage01.asset");

        gameMgr.settings        = settings;
        gameMgr.currentMap      = mapData;
        gameMgr.gridManager     = gm;
        gameMgr.resourceManager = resMgrGO.GetComponent<ResourceManager>();
        gameMgr.turnManager     = turnMgrGO.GetComponent<TurnManager>();

        // ── 5. UI 구성 ────────────────────────────────────────
        var uiGO = BuildUI(cam);

        // ── 6. 씬 저장 ───────────────────────────────────────
        string scenePath = "Assets/Scenes/Stage01.unity";
        System.IO.Directory.CreateDirectory(Application.dataPath + "/../Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        // Build Settings에 추가
        AddSceneToBuild(scenePath);

        Debug.Log("[WarGame] Stage01 씬 셋업 완료! Assets/Scenes/Stage01.unity");
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(scenePath));
    }

    // ── UI 구성 ───────────────────────────────────────────────
    static GameObject BuildUI(Camera cam)
    {
        // Canvas
        var canvasGO = new GameObject("UI");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── HUD 패널 (상단) ───────────────────────────────────
        var hudPanel = MakePanel(canvasGO, "HUD",
            new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0, -80), new Vector2(0, 0), 80);
        SetPanelColor(hudPanel, new Color(0, 0, 0, 0.65f));

        var turnText = MakeTMPText(hudPanel, "TurnText", "Turn 1 - Player",
            new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-20, 0), 28);
        var goldText = MakeTMPText(hudPanel, "GoldText", "Gold: 100",
            new Vector2(0.5f, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, 0), 28);

        // ── End Turn 버튼 (우하단) ────────────────────────────
        var endTurnBtn = MakeButton(canvasGO, "EndTurnButton", "End Turn",
            new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-160, 80), new Vector2(150, 60));

        // ── 유닛 정보 패널 (좌하단) ───────────────────────────
        var unitPanel = MakePanel(canvasGO, "UnitInfoPanel",
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(10, 10), new Vector2(280, 130), 0);
        SetPanelColor(unitPanel, new Color(0.05f, 0.05f, 0.1f, 0.85f));
        unitPanel.SetActive(false);

        var unitName  = MakeTMPText(unitPanel, "UnitName",  "Unit Name",  new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -18), 22, TextAlignmentOptions.Left);
        var unitHp    = MakeTMPText(unitPanel, "UnitHp",    "HP: -",   new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -50), 18, TextAlignmentOptions.Left);
        var unitStats = MakeTMPText(unitPanel, "UnitStats", "",        new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), 16, TextAlignmentOptions.Left);

        // ── 건물 패널 (좌하단, 유닛과 같은 위치) ─────────────
        var buildingPanel = MakePanel(canvasGO, "BuildingPanel",
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(10, 10), new Vector2(320, 180), 0);
        SetPanelColor(buildingPanel, new Color(0.05f, 0.1f, 0.05f, 0.85f));
        buildingPanel.SetActive(false);

        MakeTMPText(buildingPanel, "BuildingTitle", "Train Unit", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -18), 22);
        var unitBtnContainer = MakePanel(buildingPanel, "UnitButtonContainer",
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(10, 10), new Vector2(-20, -50), 0);
        var hlg = unitBtnContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing  = 8;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        // 유닛 버튼 프리팹 생성
        var unitBtnPrefab = CreateUnitButtonPrefab();

        // ── 승/패 패널 ────────────────────────────────────────
        var victoryPanel = MakePanel(canvasGO, "VictoryPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(500, 300), 0);
        SetPanelColor(victoryPanel, new Color(0.1f, 0.3f, 0.1f, 0.95f));
        MakeTMPText(victoryPanel, "VictoryText", "Victory!", new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 0), 60, TextAlignmentOptions.Center);
        victoryPanel.SetActive(false);

        var defeatPanel = MakePanel(canvasGO, "DefeatPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(500, 300), 0);
        SetPanelColor(defeatPanel, new Color(0.3f, 0.05f, 0.05f, 0.95f));
        MakeTMPText(defeatPanel, "DefeatText", "Defeat...", new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 0), 60, TextAlignmentOptions.Center);
        defeatPanel.SetActive(false);

        // ── GameUI 컴포넌트 연결 ──────────────────────────────
        var gameUI = canvasGO.AddComponent<GameUI>();
        gameUI.turnText           = turnText;
        gameUI.goldText           = goldText;
        gameUI.endTurnButton      = endTurnBtn;
        gameUI.unitInfoPanel      = unitPanel;
        gameUI.unitNameText       = unitName;
        gameUI.unitHpText         = unitHp;
        gameUI.unitStatsText      = unitStats;
        gameUI.buildingPanel      = buildingPanel;
        gameUI.unitButtonContainer = unitBtnContainer.transform;
        gameUI.unitButtonPrefab   = unitBtnPrefab;
        gameUI.victoryPanel       = victoryPanel;
        gameUI.defeatPanel        = defeatPanel;

        return canvasGO;
    }

    // ── UI 헬퍼 ───────────────────────────────────────────────
    static GameObject MakePanel(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin     = anchorMin;
        rt.anchorMax     = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta     = sizeDelta;
        if (height > 0) rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        go.AddComponent<Image>().color = Color.clear;
        return go;
    }

    static void SetPanelColor(GameObject panel, Color color)
    {
        var img = panel.GetComponent<Image>();
        if (img == null) img = panel.AddComponent<Image>();
        img.color = color;
    }

    static TextMeshProUGUI MakeTMPText(GameObject parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, int fontSize,
        TextAlignmentOptions align = TextAlignmentOptions.Left)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = new Vector2(-20, 30);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        tmp.alignment = align;
        return tmp;
    }

    static Button MakeButton(GameObject parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.35f, 0.6f);

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.highlightedColor = new Color(0.25f, 0.50f, 0.85f);
        cb.pressedColor     = new Color(0.08f, 0.20f, 0.40f);
        btn.colors = cb;

        // Label
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin        = Vector2.zero;
        lblRT.anchorMax        = Vector2.one;
        lblRT.anchoredPosition = Vector2.zero;
        lblRT.sizeDelta        = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 22;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    static GameObject CreateUnitButtonPrefab()
    {
        const string path = "Assets/Prefabs/UnitButton.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("UnitButton");
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 70);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.2f);
        go.AddComponent<Button>();

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin        = Vector2.zero;
        lblRT.anchorMax        = Vector2.one;
        lblRT.anchoredPosition = Vector2.zero;
        lblRT.sizeDelta        = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize  = 16;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ── Build Settings ────────────────────────────────────────
    static void AddSceneToBuild(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);

        if (!scenes.Exists(s => s.path == scenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
