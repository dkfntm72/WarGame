using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Tilemaps;
using System.Linq;

/// <summary>
/// Window > WarGame > Setup Stage01
/// ScriptableObject, 프리팹, Stage01 씬 전체를 한 번에 생성/갱신합니다.
/// </summary>
public static class WarGameStageSetup
{
    private const string UnitRoot     = "Assets/Tiny Swords/Units/";
    private const string BuildingRoot = "Assets/Tiny Swords/Buildings/";
    private const string TileRoot     = "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Sliced Tiles/";

    [MenuItem("Window/WarGame/Setup Stage01")]
    public static void Run()
    {
        EnsureFolders();

        // ── ScriptableObjects ─────────────────────────────────
        var warrior      = MakeUnitData(UnitType.Warrior,     "검사",    false, 100, 15, 5, 3, 1, 6, 50,
            "Blue Units/Warrior/Warrior_Idle.png",
            "Blue Units/Warrior/Warrior Blue Animations/Warrior_Blue.controller",
            "Red Units/Warrior/Warrior_Idle.png",
            "Red Units/Warrior/Warrior Red Animations/Warrior.controller");

        var archer       = MakeUnitData(UnitType.Archer,      "궁수",    false, 60,  20, 0, 2, 3, 6, 60,
            "Blue Units/Archer/Archer_Idle.png",
            "Blue Units/Archer/Archer Blue Animations/Archer_Blue.controller",
            "Red Units/Archer/Archer_Idle.png",
            "Red Units/Archer/Archer Animations Red/Archer_Red.controller");

        var lancer       = MakeUnitData(UnitType.Lancer,      "창병",    false, 80,  10, 3, 3, 1, 6, 40,
            "Blue Units/Lancer/Lancer_Idle.png",
            "Blue Units/Lancer/Lancer Blue Animations/Lancer_Blue.controller",
            "Red Units/Lancer/Lancer_Idle.png",
            "Red Units/Lancer/Lancer Red Animations/Lancer_Red.controller");

        var monk         = MakeUnitData(UnitType.Monk,        "회복사",  false, 40,  10, 0, 2, 2, 6, 30,
            "Blue Units/Monk/Idle.png",
            "Blue Units/Monk/Monk Blue Animations/Monk_Blue.controller",
            "Red Units/Monk/Idle.png",
            "Red Units/Monk/Monk Red Animations/Monk_Red.controller");

        // EliteWarrior: 플레이어 전용, 생산 불가 — Warrior 스프라이트/애니메이터 공유
        var elite        = MakeUnitData(UnitType.EliteWarrior, "정예 전사", true, 150, 23, 8, 4, 1, 6, 0,
            "Blue Units/Warrior/Warrior_Idle.png",
            "Blue Units/Warrior/Warrior Blue Animations/Warrior_Blue.controller",
            null, null);  // 적 진영 없음

        // Tower: 이동 불가(moveRange=0), 사거리 3, 생산 불가 — 스프라이트는 수동 연결
        var tower        = MakeUnitData(UnitType.Tower,        "타워",    true, 200, 20, 5, 0, 3, 6, 0,
            null, null, null, null);

        var castle       = MakeBuildingData(BuildingType.Castle,      "성",          10, "Castle.png");
        var archery      = MakeBuildingData(BuildingType.ArcheryRange, "궁수 양성소", 0,  "Archery.png");
        var cathedral    = MakeBuildingData(BuildingType.Cathedral,    "성당",        0,  "Monastery.png");
        var house        = MakeBuildingData(BuildingType.House,        "집",          5,  "House1.png");

        var unitPrefab     = EnsureUnitPrefab();
        var buildingPrefab = EnsureBuildingPrefab();

        var settings = EnsureGameSettings(
            new[] { warrior, archer, lancer, monk, elite, tower },
            new[] { castle, archery, cathedral, house },
            unitPrefab, buildingPrefab);

        EnsureStage01MapData(settings);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── 씬 셋업 ───────────────────────────────────────────
        SetupScene(settings);

        Debug.Log("[WarGame] Stage01 셋업 완료!");
    }

    // ─────────────────────────────────────────────────────────
    // 폴더
    // ─────────────────────────────────────────────────────────
    static void EnsureFolders()
    {
        MakeFolder("Assets",          "GameData");
        MakeFolder("Assets/GameData", "Units");
        MakeFolder("Assets/GameData", "Buildings");
        MakeFolder("Assets/GameData", "Maps");
        MakeFolder("Assets",          "Prefabs");
        MakeFolder("Assets",          "Scenes");
    }

    static void MakeFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            AssetDatabase.CreateFolder(parent, child);
    }

    // ─────────────────────────────────────────────────────────
    // ScriptableObject 헬퍼
    // ─────────────────────────────────────────────────────────
    static T GetOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var obj = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(obj, path);
        return obj;
    }

    static Sprite LoadSprite(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return null;
        var subs = AssetDatabase.LoadAllAssetsAtPath(fullPath).OfType<Sprite>().ToArray();
        return subs.Length > 0 ? subs[0] : AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
    }

    static RuntimeAnimatorController LoadCtrl(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return null;
        return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(fullPath);
    }

    // ─────────────────────────────────────────────────────────
    // UnitData
    // ─────────────────────────────────────────────────────────
    static UnitData MakeUnitData(
        UnitType type, string unitName, bool isElite,
        int hp, int atk, int def, int mov, int range, int detect, int cost,
        string pSpr, string pCtrl, string eSpr, string eCtrl)
    {
        var data = GetOrCreate<UnitData>($"Assets/GameData/Units/{type}.asset");
        data.unitType    = type;
        data.unitName    = unitName;
        data.isElite     = isElite;
        data.maxHp       = hp;
        data.attack      = atk;
        data.defense     = def;
        data.moveRange   = mov;
        data.attackRange = range;
        data.detectRange = detect;
        data.goldCost    = cost;

        // 경로가 있을 때만 덮어씀 — null이면 기존 수동 연결값 보존
        if (!string.IsNullOrEmpty(pSpr))  data.playerIdleSprite     = LoadSprite(UnitRoot + pSpr);
        if (!string.IsNullOrEmpty(pCtrl)) data.playerAnimController = LoadCtrl(UnitRoot + pCtrl);
        if (!string.IsNullOrEmpty(eSpr))  data.enemyIdleSprite      = LoadSprite(UnitRoot + eSpr);
        if (!string.IsNullOrEmpty(eCtrl)) data.enemyAnimController  = LoadCtrl(UnitRoot + eCtrl);

        // EliteWarrior: ally 스프라이트도 player와 동일하게
        if (isElite && data.playerIdleSprite != null && data.allyIdleSprite == null)
        {
            data.allyIdleSprite      = data.playerIdleSprite;
            data.allyAnimController  = data.playerAnimController;
        }

        EditorUtility.SetDirty(data);
        return data;
    }

    // ─────────────────────────────────────────────────────────
    // BuildingData
    // ─────────────────────────────────────────────────────────
    static BuildingData MakeBuildingData(BuildingType type, string bName, int gold, string png)
    {
        var data = GetOrCreate<BuildingData>($"Assets/GameData/Buildings/{type}.asset");
        data.buildingType   = type;
        data.buildingName   = bName;
        data.goldProduction = gold;
        data.playerSprite   = LoadSprite(BuildingRoot + "Blue Buildings/"   + png);
        data.enemySprite    = LoadSprite(BuildingRoot + "Red Buildings/"    + png);
        data.neutralSprite  = LoadSprite(BuildingRoot + "Yellow Buildings/" + png);
        EditorUtility.SetDirty(data);
        return data;
    }

    // ─────────────────────────────────────────────────────────
    // 프리팹
    // ─────────────────────────────────────────────────────────
    static GameObject EnsureUnitPrefab()
    {
        const string path = "Assets/Prefabs/Unit.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("Unit");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        go.AddComponent<Animator>();
        go.AddComponent<Unit>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject EnsureBuildingPrefab()
    {
        const string path = "Assets/Prefabs/Building.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("Building");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        go.AddComponent<Building>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ─────────────────────────────────────────────────────────
    // GameSettings
    // ─────────────────────────────────────────────────────────
    static GameSettings EnsureGameSettings(UnitData[] units, BuildingData[] buildings,
        GameObject unitPrefab, GameObject buildingPrefab)
    {
        var s = GetOrCreate<GameSettings>("Assets/GameData/GameSettings.asset");
        s.unitDataList     = units;
        s.buildingDataList = buildings;
        s.unitPrefab       = unitPrefab;
        s.buildingPrefab   = buildingPrefab;

        s.grassTile  = AssetDatabase.LoadAssetAtPath<TileBase>(TileRoot + "Tilemap_color1_22.asset");
        s.slopeTile  = AssetDatabase.LoadAssetAtPath<TileBase>(TileRoot + "Tilemap_color2_22.asset");
        s.wallTile   = AssetDatabase.LoadAssetAtPath<TileBase>(TileRoot + "Tilemap_color4_22.asset");
        s.waterTile  = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Tile animated.asset");

        s.moveHighlightTile   = EnsureHighlightTile("MoveHighlight",   new Color(0.1f, 0.9f, 0.1f, 0.4f));
        s.attackHighlightTile = EnsureHighlightTile("AttackHighlight", new Color(0.9f, 0.1f, 0.1f, 0.4f));

        // grass2Tile, wallOnWaterTile 은 에셋 준비 후 수동 연결
        EditorUtility.SetDirty(s);
        return s;
    }

    static TileBase EnsureHighlightTile(string name, Color color)
    {
        string tilePath = $"Assets/GameData/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (existing != null) return existing;

        string texPath = $"Assets/GameData/{name}_tex.png";
        if (!System.IO.File.Exists(texPath))
        {
            var tex    = new Texture2D(16, 16);
            var pixels = new Color[256];
            for (int i = 0; i < 256; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            System.IO.File.WriteAllBytes(texPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(texPath);

            var imp = (TextureImporter)AssetImporter.GetAtPath(texPath);
            imp.textureType         = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 16;
            imp.filterMode          = FilterMode.Point;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
        }

        var tile   = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = LoadSprite(texPath);
        tile.color  = Color.white;
        AssetDatabase.CreateAsset(tile, tilePath);
        return tile;
    }

    // ─────────────────────────────────────────────────────────
    // Stage01 MapData
    // ─────────────────────────────────────────────────────────
    static void EnsureStage01MapData(GameSettings settings)
    {
        const string path = "Assets/GameData/Maps/Stage01.asset";
        var map = GetOrCreate<MapData>(path);

        map.scenarioName    = "제1장: 국경의 전투";
        map.scenarioStory   = "적군이 국경을 넘었다. 성을 지키고 적의 성을 점령하라!\n\n" +
                              "힌트: 중앙 강을 우회해 적의 측면을 노려라.";
        map.width           = 10;
        map.height          = 8;
        map.playerStartGold = 100;
        map.enemyStartGold  = 80;

        map.InitializeTerrain();

        // 강 (중앙 대각선)
        map.SetTerrain(4, 4, TerrainType.Water);
        map.SetTerrain(5, 4, TerrainType.Water);
        map.SetTerrain(4, 3, TerrainType.Water);
        map.SetTerrain(5, 3, TerrainType.Water);
        map.SetTerrain(5, 2, TerrainType.Water);
        map.SetTerrain(6, 2, TerrainType.Water);

        map.buildings = new[]
        {
            new BuildingPlacement { x=1, y=4, buildingType=BuildingType.Castle,      faction=Faction.Player },
            new BuildingPlacement { x=2, y=6, buildingType=BuildingType.House,        faction=Faction.Player },
            new BuildingPlacement { x=7, y=5, buildingType=BuildingType.ArcheryRange, faction=Faction.Enemy  },
            new BuildingPlacement { x=8, y=4, buildingType=BuildingType.Castle,       faction=Faction.Enemy  },
        };

        map.units = new[]
        {
            new UnitPlacement { x=2, y=4, unitType=UnitType.Warrior, faction=Faction.Player },
            new UnitPlacement { x=1, y=5, unitType=UnitType.Lancer,  faction=Faction.Player },
            new UnitPlacement { x=2, y=3, unitType=UnitType.Warrior, faction=Faction.Player },

            new UnitPlacement { x=7, y=4, unitType=UnitType.Warrior, faction=Faction.Enemy  },
            new UnitPlacement { x=8, y=5, unitType=UnitType.Lancer,  faction=Faction.Enemy  },
            new UnitPlacement { x=7, y=3, unitType=UnitType.Archer,  faction=Faction.Enemy  },
        };

        EditorUtility.SetDirty(map);
    }

    // ─────────────────────────────────────────────────────────
    // 씬 셋업
    // ─────────────────────────────────────────────────────────
    static void SetupScene(GameSettings settings)
    {
        const string scenePath = "Assets/Scenes/Stage01.unity";

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // ── 카메라 ────────────────────────────────────────────
        var cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.gameObject.tag = "MainCamera";
        }
        cam.orthographic      = true;
        cam.orthographicSize  = 5f;
        cam.transform.position = new Vector3(4.5f, 3.5f, -10f);
        cam.backgroundColor   = new Color(0.1f, 0.1f, 0.15f);
        cam.gameObject.AddComponent<CameraDragController>();

        // ── Grid / Tilemap ────────────────────────────────────
        var gridGO   = new GameObject("Grid");
        var grid     = gridGO.AddComponent<Grid>();
        grid.cellSize = Vector3.one;

        var terrainGO  = new GameObject("TerrainTilemap");
        terrainGO.transform.SetParent(gridGO.transform);
        var terrainTM  = terrainGO.AddComponent<Tilemap>();
        var terrainTMR = terrainGO.AddComponent<TilemapRenderer>();
        terrainTMR.sortingOrder = 0;

        var highlightGO  = new GameObject("HighlightTilemap");
        highlightGO.transform.SetParent(gridGO.transform);
        var highlightTM  = highlightGO.AddComponent<Tilemap>();
        var highlightTMR = highlightGO.AddComponent<TilemapRenderer>();
        highlightTMR.sortingOrder = 3;

        // ── GridManager ───────────────────────────────────────
        var gmGO = new GameObject("GridManager");
        var gm   = gmGO.AddComponent<GridManager>();
        gm.grid             = grid;
        gm.terrainTilemap   = terrainTM;
        gm.highlightTilemap = highlightTM;

        // ── 매니저 ────────────────────────────────────────────
        var managers = new GameObject("Managers");

        var turnMgr  = AddManager<TurnManager>(managers,     "TurnManager");
        var resMgr   = AddManager<ResourceManager>(managers, "ResourceManager");
        AddManager<EnemyAI>(managers,          "EnemyAI");
        AddManager<AllyAI>(managers,           "AllyAI");
        AddManager<PlayerInputHandler>(managers, "PlayerInputHandler");
        AddManager<EventTriggerManager>(managers, "EventTriggerManager");

        var gameMgrGO = new GameObject("GameManager");
        gameMgrGO.transform.SetParent(managers.transform);
        var gameMgr = gameMgrGO.AddComponent<GameManager>();

        var mapData = AssetDatabase.LoadAssetAtPath<MapData>("Assets/GameData/Maps/Stage01.asset");
        gameMgr.settings        = settings;
        gameMgr.currentMap      = mapData;
        gameMgr.gridManager     = gm;
        gameMgr.resourceManager = resMgr;
        gameMgr.turnManager     = turnMgr;

        // ── Canvas UI ─────────────────────────────────────────
        BuildGameUI.Execute();

        // ── 씬 저장 ───────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();
        AddSceneToBuild(scenePath);

        Debug.Log("[WarGame] Stage01 씬 저장 완료: " + scenePath);
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(scenePath));
    }

    static T AddManager<T>(GameObject parent, string name) where T : Component
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        return go.AddComponent<T>();
    }

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
