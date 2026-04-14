using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// <summary>
/// Window > WarGame > Create Stage 01 로 실행
/// GameData 폴더에 모든 ScriptableObject, 프리팹, Stage01 맵을 자동 생성합니다.
/// </summary>
public static class WarGameStageCreator
{
    private const string UnitRoot     = "Assets/Tiny Swords/Units/";
    private const string BuildingRoot = "Assets/Tiny Swords/Buildings/";
    private const string TileRoot     = "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Sliced Tiles/";

    // [MenuItem("Window/WarGame/Create Stage 01")] — WarGameStageSetup으로 통합됨
    public static void CreateStage01()
    {
        EnsureFolders();

        // ── Unit Data ────────────────────────────────────────
        var warrior = MakeUnitData(UnitType.Warrior, "Warrior", 100, 15, 5, 3, 1, 50,
            "Blue Units/Warrior/Warrior_Idle.png",
            "Blue Units/Warrior/Warrior Blue Animations/Warrior_Blue.controller",
            "Red Units/Warrior/Warrior_Idle.png",
            "Red Units/Warrior/Warrior Red Animations/Warrior.controller");

        var archer = MakeUnitData(UnitType.Archer, "Archer", 60, 20, 0, 2, 3, 60,
            "Blue Units/Archer/Archer_Idle.png",
            "Blue Units/Archer/Archer Blue Animations/Archer_Blue.controller",
            "Red Units/Archer/Archer_Idle.png",
            "Red Units/Archer/Archer Animations Red/Archer_Red.controller");

        var lancer = MakeUnitData(UnitType.Lancer, "Lancer", 80, 10, 3, 3, 1, 40,
            "Blue Units/Lancer/Lancer_Idle.png",
            "Blue Units/Lancer/Lancer Blue Animations/Lancer_Blue.controller",
            "Red Units/Lancer/Lancer_Idle.png",
            "Red Units/Lancer/Lancer Red Animations/Lancer_Red.controller");

        var monk = MakeUnitData(UnitType.Monk, "Monk", 40, 10, 0, 2, 2, 30,
            "Blue Units/Monk/Idle.png",
            "Blue Units/Monk/Monk Blue Animations/Monk_Blue.controller",
            "Red Units/Monk/Idle.png",
            "Red Units/Monk/Monk Red Animations/Monk_Red.controller");

        // ── Building Data ────────────────────────────────────
        var castle      = MakeBuildingData(BuildingType.Castle,       "Castle",        10, "Castle.png");
        var archery     = MakeBuildingData(BuildingType.ArcheryRange,  "Archery Range", 0, "Archery.png");
        var cathedral   = MakeBuildingData(BuildingType.Cathedral,     "Cathedral",     0, "Monastery.png");
        var house       = MakeBuildingData(BuildingType.House,         "House",         5, "House1.png");

        // ── Prefabs ──────────────────────────────────────────
        var unitPrefab     = EnsureUnitPrefab();
        var buildingPrefab = EnsureBuildingPrefab();

        // ── Game Settings ────────────────────────────────────
        var settings = EnsureGameSettings(
            new[] { warrior, archer, lancer, monk },
            new[] { castle, archery, cathedral, house },
            unitPrefab, buildingPrefab);

        // ── Stage 01 MapData ─────────────────────────────────
        EnsureStage01(settings);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── Canvas UI 생성 ────────────────────────────────────
        BuildGameUI.Execute();

        Debug.Log("[WarGame] Stage 01 생성 완료! GameData 폴더를 확인하세요.");
    }

    // ── Folders ───────────────────────────────────────────────
    static void EnsureFolders()
    {
        MakeFolder("Assets",          "GameData");
        MakeFolder("Assets/GameData", "Units");
        MakeFolder("Assets/GameData", "Buildings");
        MakeFolder("Assets/GameData", "Maps");
        MakeFolder("Assets",          "Prefabs");
    }

    static void MakeFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            AssetDatabase.CreateFolder(parent, child);
    }

    // ── Helpers ───────────────────────────────────────────────
    static Sprite LoadSprite(string fullPath)
    {
        var subs = AssetDatabase.LoadAllAssetsAtPath(fullPath).OfType<Sprite>().ToArray();
        return subs.Length > 0 ? subs[0] : AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
    }

    static RuntimeAnimatorController LoadCtrl(string fullPath) =>
        AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(fullPath);

    static T GetOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var obj = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(obj, path);
        return obj;
    }

    // ── Unit Data ─────────────────────────────────────────────
    static UnitData MakeUnitData(UnitType type, string unitName,
        int hp, int atk, int def, int mov, int range, int cost,
        string pSpr, string pCtrl, string eSpr, string eCtrl)
    {
        var data = GetOrCreate<UnitData>($"Assets/GameData/Units/{type}.asset");
        data.unitType    = type;
        data.unitName    = unitName;
        data.maxHp       = hp;
        data.attack      = atk;
        data.defense     = def;
        data.moveRange   = mov;
        data.attackRange = range;
        data.goldCost    = cost;

        data.playerIdleSprite     = LoadSprite(UnitRoot + pSpr);
        data.playerAnimController = LoadCtrl(UnitRoot + pCtrl);
        data.enemyIdleSprite      = LoadSprite(UnitRoot + eSpr);
        data.enemyAnimController  = LoadCtrl(UnitRoot + eCtrl);

        EditorUtility.SetDirty(data);
        return data;
    }

    // ── Building Data ─────────────────────────────────────────
    static BuildingData MakeBuildingData(BuildingType type, string bName, int gold, string png)
    {
        var data = GetOrCreate<BuildingData>($"Assets/GameData/Buildings/{type}.asset");
        data.buildingType   = type;
        data.buildingName   = bName;
        data.goldProduction = gold;

        data.playerSprite  = LoadSprite(BuildingRoot + "Blue Buildings/" + png);
        data.enemySprite   = LoadSprite(BuildingRoot + "Red Buildings/"  + png);
        data.neutralSprite = LoadSprite(BuildingRoot + "Yellow Buildings/" + png);

        EditorUtility.SetDirty(data);
        return data;
    }

    // ── Prefabs ───────────────────────────────────────────────
    static GameObject EnsureUnitPrefab()
    {
        const string path = "Assets/Prefabs/Unit.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("Unit");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
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

    // ── Game Settings ─────────────────────────────────────────
    static GameSettings EnsureGameSettings(UnitData[] units, BuildingData[] buildings,
        GameObject unitPrefab, GameObject buildingPrefab)
    {
        var s = GetOrCreate<GameSettings>("Assets/GameData/GameSettings.asset");
        s.unitDataList     = units;
        s.buildingDataList = buildings;
        s.unitPrefab       = unitPrefab;
        s.buildingPrefab   = buildingPrefab;

        // Grass – color1 center tile (index 22 = typically the solid center in a 44-tile wang set)
        s.grassTile = AssetDatabase.LoadAssetAtPath<TileBase>(TileRoot + "Tilemap_color1_22.asset");
        // Sand/slope – color2
        s.slopeTile = AssetDatabase.LoadAssetAtPath<TileBase>(TileRoot + "Tilemap_color2_22.asset");
        // Stone/wall – color4
        s.wallTile  = AssetDatabase.LoadAssetAtPath<TileBase>(TileRoot + "Tilemap_color4_22.asset");
        // Water
        s.waterTile = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Tile animated.asset");

        // Highlight tiles (semi-transparent colored squares)
        s.moveHighlightTile   = EnsureHighlightTile("MoveHighlight",   new Color(0.1f, 0.9f, 0.1f, 0.4f));
        s.attackHighlightTile = EnsureHighlightTile("AttackHighlight", new Color(0.9f, 0.1f, 0.1f, 0.4f));

        EditorUtility.SetDirty(s);
        return s;
    }

    static TileBase EnsureHighlightTile(string name, Color color)
    {
        string tilePath = $"Assets/GameData/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (existing != null) return existing;

        // 16×16 semi-transparent texture
        string texPath = $"Assets/GameData/{name}_tex.png";
        if (!System.IO.File.Exists(texPath))
        {
            var tex = new Texture2D(16, 16);
            var pixels = new Color[256];
            for (int i = 0; i < 256; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            System.IO.File.WriteAllBytes(texPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(texPath);

            var imp = (TextureImporter)AssetImporter.GetAtPath(texPath);
            imp.textureType        = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 16;
            imp.filterMode         = FilterMode.Point;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
        }

        var tile   = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = LoadSprite(texPath);
        tile.color  = Color.white;
        AssetDatabase.CreateAsset(tile, tilePath);
        return tile;
    }

    // ── Stage 01 MapData ──────────────────────────────────────
    static void EnsureStage01(GameSettings settings)
    {
        const string path = "Assets/GameData/Maps/Stage01.asset";
        var map = GetOrCreate<MapData>(path);

        map.scenarioName    = "Chapter 1: Battle at the Border";
        map.scenarioStory   = "Enemy forces have crossed the border. Defend your castle and capture theirs!\n\n" +
                              "Hint: Flank the enemy by crossing around the river in the center.";
        map.width           = 10;
        map.height          = 8;
        map.playerStartGold = 100;
        map.enemyStartGold  = 80;

        map.InitializeTerrain();

        // ── Terrain ──────────────────────────────────────────
        // y=7: _ _ T _ _ _ _ T _ _
        // y=6: _ _ _ _ _ _ _ _ _ _
        // y=5: _ _ _ _ _ _ _ _ _ _
        // y=4: _ _ _ _ W W _ _ _ _   ← 강
        // y=3: _ _ _ _ W W _ _ _ _   ← 강
        // y=2: _ _ _ _ _ W W _ _ _   ← 강 (대각선)
        // y=1: _ _ _ _ _ _ _ _ _ _
        // y=0: _ _ T _ _ _ _ T _ _

        foreach (int tx in new[] { 2, 7 })
        {
            map.SetTerrain(tx, 0, TerrainType.Grass);
            map.SetTerrain(tx, 7, TerrainType.Grass);
        }

        // 강 (대각선 방향으로 중앙을 가로지름)
        map.SetTerrain(4, 4, TerrainType.Water);
        map.SetTerrain(5, 4, TerrainType.Water);
        map.SetTerrain(4, 3, TerrainType.Water);
        map.SetTerrain(5, 3, TerrainType.Water);
        map.SetTerrain(5, 2, TerrainType.Water);
        map.SetTerrain(6, 2, TerrainType.Water);

        // ── Buildings ─────────────────────────────────────────
        //  Player: 성(1,4), 집(2,6)
        //  Enemy:  성(8,4), 궁수양성소(7,5)
        map.buildings = new[]
        {
            new BuildingPlacement { x=1, y=4, buildingType=BuildingType.Castle,      faction=Faction.Player },
            new BuildingPlacement { x=2, y=6, buildingType=BuildingType.House,        faction=Faction.Player },
            new BuildingPlacement { x=7, y=5, buildingType=BuildingType.ArcheryRange, faction=Faction.Enemy  },
            new BuildingPlacement { x=8, y=4, buildingType=BuildingType.Castle,       faction=Faction.Enemy  },
        };

        // ── Units ─────────────────────────────────────────────
        //  Player: 검사×2, 창병×1  (성 주변 왼쪽)
        //  Enemy:  검사×1, 창병×1, 궁수×1  (성 주변 오른쪽)
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
        Debug.Log($"[WarGame] Stage01 맵 저장: {path}");
    }
}
