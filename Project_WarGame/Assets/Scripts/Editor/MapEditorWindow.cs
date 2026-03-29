using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// WarGame 맵 에디터 - Window > WarGame > Map Editor 로 열기
/// </summary>
public class MapEditorWindow : EditorWindow
{
    // ── Tool modes ────────────────────────────────────────────
    private enum Tool { Terrain, Building, Unit, Erase }

    // ── State ─────────────────────────────────────────────────
    private MapData  mapData;
    private Tool     currentTool  = Tool.Terrain;
    private Vector2  scrollPos;
    private float    cellSize     = 40f;

    // Cached styles (avoid allocating new GUIStyle every frame)
    private GUIStyle _buildingLabelStyle;
    private GUIStyle _unitLabelStyle;
    private GUIStyle _coordLabelStyle;
    private GUIStyle BuildingLabelStyle => _buildingLabelStyle ??= new GUIStyle(EditorStyles.miniLabel)
        { alignment = TextAnchor.MiddleCenter, fontSize = 8 };
    private GUIStyle UnitLabelStyle => _unitLabelStyle ??= new GUIStyle(EditorStyles.miniLabel)
        { alignment = TextAnchor.MiddleCenter, fontSize = 8 };
    private GUIStyle CoordLabelStyle => _coordLabelStyle ??= new GUIStyle(EditorStyles.miniLabel)
        { alignment = TextAnchor.LowerRight, fontSize = 7, normal = { textColor = new Color(0,0,0,0.4f) } };

    // Palette selections
    private TerrainType  selectedTerrain  = TerrainType.Grass;
    private BuildingType selectedBuilding = BuildingType.Castle;
    private UnitType     selectedUnit     = UnitType.Warrior;
    private Faction      selectedFaction  = Faction.Neutral;

    // Building/Unit lists for editing
    private List<BuildingPlacement> buildings = new();
    private List<UnitPlacement>     units     = new();

    // ── Menu item ─────────────────────────────────────────────
    [MenuItem("Window/WarGame/Map Editor")]
    public static void Open() => GetWindow<MapEditorWindow>("Map Editor");

    // ── GUI ───────────────────────────────────────────────────
    private void OnGUI()
    {
        DrawTopBar();

        if (mapData == null)
        {
            EditorGUILayout.HelpBox("Select a MapData asset or create a new one.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(4);
        DrawToolbar();
        EditorGUILayout.Space(4);
        DrawPalette();
        EditorGUILayout.Space(4);

        using var scroll = new EditorGUILayout.ScrollViewScope(scrollPos);
        scrollPos = scroll.scrollPosition;
        DrawGrid();
    }

    // ── Top bar: load / create / resize ──────────────────────
    private void DrawTopBar()
    {
        using var row = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar);

        EditorGUI.BeginChangeCheck();
        mapData = (MapData)EditorGUILayout.ObjectField(mapData, typeof(MapData), false, GUILayout.Width(200));
        if (EditorGUI.EndChangeCheck() && mapData != null) LoadFromAsset();

        if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50))) CreateNew();
        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50))) SaveToAsset();
        if (GUILayout.Button("From Tilemap", EditorStyles.toolbarButton, GUILayout.Width(90))) ImportFromTilemap();
        if (GUILayout.Button("Apply to Scene", EditorStyles.toolbarButton, GUILayout.Width(100))) ApplyToScene();

        GUILayout.Space(10);

        // Zoom
        GUILayout.Label("Zoom", GUILayout.Width(40));
        cellSize = EditorGUILayout.Slider(cellSize, 20f, 80f, GUILayout.Width(120));

        GUILayout.FlexibleSpace();

        if (mapData != null)
        {
            EditorGUILayout.LabelField($"Map: {mapData.scenarioName}  ({mapData.width}x{mapData.height})", EditorStyles.miniLabel);
            GUILayout.Space(8);

            GUILayout.Label("W", GUILayout.Width(14));
            int newW = EditorGUILayout.IntField(mapData.width, GUILayout.Width(36));
            GUILayout.Label("H", GUILayout.Width(14));
            int newH = EditorGUILayout.IntField(mapData.height, GUILayout.Width(36));
            if (GUILayout.Button("Resize", EditorStyles.toolbarButton, GUILayout.Width(55)))
                ResizeMap(newW, newH);
        }
    }

    // ── Toolbar ───────────────────────────────────────────────
    private void DrawToolbar()
    {
        using var row = new EditorGUILayout.HorizontalScope();
        DrawToolButton(Tool.Terrain,  "Paint Terrain");
        DrawToolButton(Tool.Building, "Place Building");
        DrawToolButton(Tool.Unit,     "Place Unit");
        DrawToolButton(Tool.Erase,    "Erase");
    }

    private void DrawToolButton(Tool tool, string label)
    {
        var style = currentTool == tool ? GUI.skin.button : EditorStyles.miniButton;
        if (GUILayout.Button(label, style, GUILayout.Height(28)))
            currentTool = tool;
    }

    // ── Palette (changes per tool) ────────────────────────────
    private void DrawPalette()
    {
        using var box = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);

        switch (currentTool)
        {
            case Tool.Terrain:
                DrawTerrainPalette();
                break;
            case Tool.Building:
                DrawBuildingPalette();
                break;
            case Tool.Unit:
                DrawUnitPalette();
                break;
            case Tool.Erase:
                EditorGUILayout.LabelField("Click to erase buildings/units (terrain resets to Grass).");
                break;
        }
    }

    private void DrawTerrainPalette()
    {
        using var row = new EditorGUILayout.HorizontalScope();
        GUILayout.Label("Terrain:", GUILayout.Width(60));
        foreach (TerrainType t in System.Enum.GetValues(typeof(TerrainType)))
        {
            var color = TerrainColor(t);
            var old   = GUI.backgroundColor;
            GUI.backgroundColor = selectedTerrain == t ? Color.yellow : color;
            if (GUILayout.Button(t.ToString(), GUILayout.Width(60), GUILayout.Height(24)))
                selectedTerrain = t;
            GUI.backgroundColor = old;
        }
    }

    private void DrawBuildingPalette()
    {
        using var row = new EditorGUILayout.HorizontalScope();
        GUILayout.Label("Building:", GUILayout.Width(60));
        foreach (BuildingType b in System.Enum.GetValues(typeof(BuildingType)))
        {
            bool sel = selectedBuilding == b;
            if (GUILayout.Toggle(sel, b.ToString(), EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(24)) && !sel)
                selectedBuilding = b;
        }
        GUILayout.Space(12);
        GUILayout.Label("Faction:", GUILayout.Width(55));
        DrawFactionToggle();
    }

    private void DrawUnitPalette()
    {
        using var row = new EditorGUILayout.HorizontalScope();
        GUILayout.Label("Unit:", GUILayout.Width(60));
        foreach (UnitType u in System.Enum.GetValues(typeof(UnitType)))
        {
            bool sel = selectedUnit == u;
            if (GUILayout.Toggle(sel, u.ToString(), EditorStyles.miniButton, GUILayout.Width(70), GUILayout.Height(24)) && !sel)
                selectedUnit = u;
        }
        GUILayout.Space(12);
        GUILayout.Label("Faction:", GUILayout.Width(55));
        DrawFactionToggle(false); // units can't be Neutral
    }

    private void DrawFactionToggle(bool allowNeutral = true)
    {
        if (allowNeutral)
        {
            if (GUILayout.Toggle(selectedFaction == Faction.Neutral, "Neutral", EditorStyles.miniButton, GUILayout.Width(60), GUILayout.Height(24)))
                selectedFaction = Faction.Neutral;
        }
        if (GUILayout.Toggle(selectedFaction == Faction.Player, "Player", EditorStyles.miniButton, GUILayout.Width(55), GUILayout.Height(24)))
            selectedFaction = Faction.Player;
        if (GUILayout.Toggle(selectedFaction == Faction.Enemy, "Enemy", EditorStyles.miniButton, GUILayout.Width(55), GUILayout.Height(24)))
            selectedFaction = Faction.Enemy;
    }

    // ── Grid display & input ──────────────────────────────────
    private void DrawGrid()
    {
        float totalW = mapData.width  * cellSize;
        float totalH = mapData.height * cellSize;

        var rect = GUILayoutUtility.GetRect(totalW + 2, totalH + 2,
            GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

        // Draw cells bottom-to-top (Unity tilemap convention: y=0 is bottom)
        for (int y = 0; y < mapData.height; y++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                var cellRect = GetCellRect(rect, x, y);
                DrawCell(cellRect, x, y);
            }
        }

        // Handle mouse click
        var e = Event.current;
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0
            && rect.Contains(e.mousePosition))
        {
            int cx = Mathf.FloorToInt((e.mousePosition.x - rect.x) / cellSize);
            int cy = mapData.height - 1 - Mathf.FloorToInt((e.mousePosition.y - rect.y) / cellSize);
            if (cx >= 0 && cx < mapData.width && cy >= 0 && cy < mapData.height)
                HandleCellClick(cx, cy);
            e.Use();
            Repaint();
        }
    }

    private Rect GetCellRect(Rect gridRect, int x, int y)
    {
        // y=0 draws at the bottom of the grid
        float px = gridRect.x + x * cellSize;
        float py = gridRect.y + (mapData.height - 1 - y) * cellSize;
        return new Rect(px, py, cellSize - 1, cellSize - 1);
    }

    private void DrawCell(Rect r, int x, int y)
    {
        // Background = terrain color
        EditorGUI.DrawRect(r, TerrainColor(mapData.GetTerrain(x, y)));

        // Border
        DrawBorder(r, new Color(0, 0, 0, 0.3f));

        // Building marker
        var bp = buildings.Find(b => b.x == x && b.y == y);
        if (bp != null)
        {
            EditorGUI.DrawRect(Shrink(r, 6), FactionColor(bp.faction));
            EditorGUI.LabelField(r, BuildingChar(bp.buildingType), BuildingLabelStyle);
        }

        // Unit marker
        var up = units.Find(u => u.x == x && u.y == y);
        if (up != null)
        {
            var unitRect = Shrink(r, 10);
            EditorGUI.DrawRect(unitRect, FactionColor(up.faction));
            EditorGUI.LabelField(r, UnitChar(up.unitType), UnitLabelStyle);
        }

        // Coordinates (small)
        if (cellSize >= 36)
        {
            EditorGUI.LabelField(r, $"{x},{y}", CoordLabelStyle);
        }
    }

    private void HandleCellClick(int x, int y)
    {
        Undo.RecordObject(mapData, "Edit Map");
        EditorUtility.SetDirty(mapData);

        switch (currentTool)
        {
            case Tool.Terrain:
                mapData.SetTerrain(x, y, selectedTerrain);
                break;

            case Tool.Building:
                buildings.RemoveAll(b => b.x == x && b.y == y);
                buildings.Add(new BuildingPlacement { x = x, y = y, buildingType = selectedBuilding, faction = selectedFaction });
                break;

            case Tool.Unit:
                var faction = selectedFaction == Faction.Neutral ? Faction.Player : selectedFaction;
                units.RemoveAll(u => u.x == x && u.y == y);
                units.Add(new UnitPlacement { x = x, y = y, unitType = selectedUnit, faction = faction });
                break;

            case Tool.Erase:
                mapData.SetTerrain(x, y, TerrainType.Grass);
                buildings.RemoveAll(b => b.x == x && b.y == y);
                units.RemoveAll(u => u.x == x && u.y == y);
                break;
        }
    }

    // ── Asset I/O ─────────────────────────────────────────────
    private void CreateNew()
    {
        var path = EditorUtility.SaveFilePanelInProject(
            "Create New Map", "NewMap", "asset", "Choose where to save the map asset");
        if (string.IsNullOrEmpty(path)) return;

        var asset = ScriptableObject.CreateInstance<MapData>();
        asset.InitializeTerrain();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();

        mapData   = asset;
        buildings = new List<BuildingPlacement>();
        units     = new List<UnitPlacement>();
    }

    private void LoadFromAsset()
    {
        buildings = mapData.buildings != null
            ? new List<BuildingPlacement>(mapData.buildings)
            : new List<BuildingPlacement>();

        units = mapData.units != null
            ? new List<UnitPlacement>(mapData.units)
            : new List<UnitPlacement>();

        if (mapData.terrain == null || mapData.terrain.Length != mapData.width * mapData.height)
            mapData.InitializeTerrain();
    }

    private void SaveToAsset()
    {
        if (mapData == null) return;

        Undo.RecordObject(mapData, "Save Map");
        mapData.buildings = buildings.ToArray();
        mapData.units     = units.ToArray();
        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();
        Debug.Log($"[MapEditor] '{mapData.scenarioName}' saved ({mapData.width}x{mapData.height})");
    }

    private void ApplyToScene()
    {
        if (mapData == null) { EditorUtility.DisplayDialog("Apply", "MapData를 먼저 선택하세요.", "OK"); return; }

        var guids = AssetDatabase.FindAssets("t:GameSettings");
        if (guids.Length == 0) { EditorUtility.DisplayDialog("Apply", "GameSettings를 찾을 수 없습니다.", "OK"); return; }
        var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));

        var gm = Object.FindFirstObjectByType<GridManager>();
        if (gm == null) { EditorUtility.DisplayDialog("Apply", "씬에 GridManager가 없습니다.", "OK"); return; }
        var tilemap = gm.terrainTilemap;
        if (tilemap == null) { EditorUtility.DisplayDialog("Apply", "GridManager에 terrainTilemap이 없습니다.", "OK"); return; }

        // TerrainTileConfig 로드 (terrain type 별 — 같은 타입이 여러 개면 규칙 수가 많은 것을 사용)
        var configs = new Dictionary<TerrainType, TerrainTileConfig>();
        foreach (var guid in AssetDatabase.FindAssets("t:TerrainTileConfig"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var cfg  = AssetDatabase.LoadAssetAtPath<TerrainTileConfig>(path);
            if (cfg == null) continue;

            if (configs.TryGetValue(cfg.terrainType, out var existing))
            {
                if (cfg.rules.Count > existing.rules.Count)
                {
                    Debug.LogWarning($"[MapEditor] TerrainTileConfig 중복 — 규칙 수가 더 많은 '{path}' ({cfg.rules.Count}개)를 사용합니다 (기존 {existing.rules.Count}개 버림).");
                    configs[cfg.terrainType] = cfg;
                }
                else
                {
                    Debug.LogWarning($"[MapEditor] TerrainTileConfig 중복 — '{path}' ({cfg.rules.Count}개)는 기존 것({existing.rules.Count}개)보다 규칙이 적으므로 무시합니다.");
                }
            }
            else
            {
                configs[cfg.terrainType] = cfg;
            }
            Debug.Log($"[MapEditor] Config 로드: {cfg.terrainType} ({cfg.rules.Count}개 규칙) ← {path}");
        }

        Undo.RecordObject(tilemap, "Apply MapData to Scene");
        tilemap.ClearAllTiles();

        int ruleHit = 0, defaultHit = 0, fallbackHit = 0;

        for (int y = 0; y < mapData.height; y++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                var terrain = mapData.GetTerrain(x, y);
                TileBase tile;

                if (configs.TryGetValue(terrain, out var cfg))
                {
                    var resolved = cfg.Resolve(GetNeighbors8(x, y));
                    if (resolved != cfg.defaultTile) ruleHit++;
                    else defaultHit++;
                    tile = resolved;
                }
                else
                {
                    // 폴백: GameSettings 기본 타일
                    tile = settings.GetTerrainTile(terrain);
                    fallbackHit++;
                }

                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        tilemap.RefreshAllTiles();

        // ── Water Foam 레이어 적용 ────────────────────────────────
        ApplyWaterFoamLayer(gm.grid, mapData);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[MapEditor] 적용 완료 — 규칙 적중: {ruleHit}, 기본 타일: {defaultHit}, Config 없음(폴백): {fallbackHit}");
    }

    static void ApplyWaterFoamLayer(Grid grid, MapData mapData)
    {
        // WaterFoam 타일 에셋 로드/생성
        const string foamAssetPath = "Assets/GameData/WaterFoam.asset";
        var foamTile = AssetDatabase.LoadAssetAtPath<TileBase>(foamAssetPath);
        if (foamTile == null)
        {
            var allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Tiny Swords/Terrain/Tileset/Water Foam.png");
            UnityEngine.Sprite foamSprite = null;
            foreach (var obj in allAssets)
                if (obj is UnityEngine.Sprite s && s.name == "Water Foam_0") { foamSprite = s; break; }

            if (foamSprite == null) { Debug.LogWarning("[MapEditor] Water Foam_0 sprite not found — foam layer skipped"); return; }

            var t = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            t.sprite = foamSprite;
            t.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None;
            AssetDatabase.CreateAsset(t, foamAssetPath);
            AssetDatabase.SaveAssets();
            foamTile = t;
        }

        // WaterFoamTilemap 찾기/생성
        UnityEngine.Tilemaps.Tilemap foamTilemap = null;
        foreach (Transform child in grid.transform)
        {
            if (child.name == "WaterFoamTilemap")
            {
                foamTilemap = child.GetComponent<UnityEngine.Tilemaps.Tilemap>();
                break;
            }
        }
        if (foamTilemap == null)
        {
            var go = new GameObject("WaterFoamTilemap");
            go.transform.SetParent(grid.transform, false);
            foamTilemap = go.AddComponent<UnityEngine.Tilemaps.Tilemap>();
            var renderer = go.AddComponent<UnityEngine.Tilemaps.TilemapRenderer>();
            renderer.sortingOrder = -1;
            Undo.RegisterCreatedObjectUndo(go, "Create WaterFoamTilemap");
        }

        foamTilemap.ClearAllTiles();

        // 물과 인접한 잔디 타일에 foam 배치
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0,  0, 1,-1 };
        for (int y = 0; y < mapData.height; y++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                if (mapData.GetTerrain(x, y) == TerrainType.Water) continue;

                bool nextToWater = false;
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d], ny = y + dy[d];
                    if (nx < 0 || nx >= mapData.width || ny < 0 || ny >= mapData.height) continue;
                    if (mapData.GetTerrain(nx, ny) == TerrainType.Water) { nextToWater = true; break; }
                }
                if (nextToWater)
                    foamTilemap.SetTile(new Vector3Int(x, y, 0), foamTile);
            }
        }

        Debug.Log("[MapEditor] Water Foam 레이어 적용 완료");
    }

    // N, NE, E, SE, S, SW, W, NW 순서로 이웃 8칸의 TerrainType 반환
    private TerrainType[] GetNeighbors8(int x, int y)
    {
        // dx, dy 쌍: N, NE, E, SE, S, SW, W, NW
        (int dx, int dy)[] dirs = {
            (0,1),(1,1),(1,0),(1,-1),(0,-1),(-1,-1),(-1,0),(-1,1)
        };
        var result = new TerrainType[8];
        for (int i = 0; i < 8; i++)
        {
            int nx = x + dirs[i].dx;
            int ny = y + dirs[i].dy;
            result[i] = (nx >= 0 && nx < mapData.width && ny >= 0 && ny < mapData.height)
                ? mapData.GetTerrain(nx, ny)
                : TerrainType.Water; // 맵 밖은 Water로 간주
        }
        return result;
    }

    private void ImportFromTilemap()
    {
        if (mapData == null) { EditorUtility.DisplayDialog("Import", "MapData를 먼저 선택하세요.", "OK"); return; }

        // GameSettings 찾기
        var guids = AssetDatabase.FindAssets("t:GameSettings");
        if (guids.Length == 0) { EditorUtility.DisplayDialog("Import", "GameSettings asset을 찾을 수 없습니다.", "OK"); return; }
        var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));

        // 씬의 GridManager → terrainTilemap
        var gm = Object.FindFirstObjectByType<GridManager>();
        if (gm == null) { EditorUtility.DisplayDialog("Import", "씬에 GridManager가 없습니다.", "OK"); return; }
        var tilemap = gm.terrainTilemap;
        if (tilemap == null) { EditorUtility.DisplayDialog("Import", "GridManager에 terrainTilemap이 연결되어 있지 않습니다.", "OK"); return; }

        // TileBase → TerrainType 딕셔너리
        var tileMap = new Dictionary<TileBase, TerrainType>();
        void Register(TileBase t, TerrainType type) { if (t != null && !tileMap.ContainsKey(t)) tileMap[t] = type; }

        // TileTerrainMapping 우선 적용 (있을 경우)
        var mappingGuids = AssetDatabase.FindAssets("t:TileTerrainMapping");
        if (mappingGuids.Length > 0)
        {
            var mapping = AssetDatabase.LoadAssetAtPath<TileTerrainMapping>(AssetDatabase.GUIDToAssetPath(mappingGuids[0]));
            if (mapping.entries != null)
                foreach (var e in mapping.entries)
                    Register(e.tile, e.terrainType);
        }

        // GameSettings의 5개 기본 타일을 폴백으로 등록
        Register(settings.grassTile,  TerrainType.Grass);
        Register(settings.wallTile,   TerrainType.Wall);
        Register(settings.slopeTile,  TerrainType.Slope);
        Register(settings.waterTile,  TerrainType.Water);

        Undo.RecordObject(mapData, "Import Terrain from Tilemap");
        mapData.InitializeTerrain();

        int matched = 0, unmatched = 0;
        for (int y = 0; y < mapData.height; y++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile == null) { mapData.SetTerrain(x, y, TerrainType.Grass); continue; }

                if (tileMap.TryGetValue(tile, out var terrainType))
                {
                    mapData.SetTerrain(x, y, terrainType);
                    matched++;
                }
                else
                {
                    mapData.SetTerrain(x, y, TerrainType.Grass);
                    unmatched++;
                }
            }
        }

        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();
        Debug.Log($"[MapEditor] Tilemap → MapData 동기화 완료. 매칭: {matched}, 미인식(Grass 처리): {unmatched}");
        EditorUtility.DisplayDialog("Import 완료", $"동기화 완료!\n매칭된 타일: {matched}\n미인식 타일(Grass 처리): {unmatched}", "OK");
    }

    private void ResizeMap(int newW, int newH)
    {
        newW = Mathf.Clamp(newW, 4, 30);
        newH = Mathf.Clamp(newH, 4, 20);

        var newTerrain = new TerrainType[newW * newH];
        for (int y = 0; y < newH; y++)
            for (int x = 0; x < newW; x++)
                newTerrain[y * newW + x] = mapData.GetTerrain(x, y);

        Undo.RecordObject(mapData, "Resize Map");
        mapData.width   = newW;
        mapData.height  = newH;
        mapData.terrain = newTerrain;

        buildings.RemoveAll(b => b.x >= newW || b.y >= newH);
        units.RemoveAll(u => u.x >= newW || u.y >= newH);

        EditorUtility.SetDirty(mapData);
    }

    // ── Visuals ───────────────────────────────────────────────
    private static Color TerrainColor(TerrainType t) => t switch
    {
        TerrainType.Grass => new Color(0.48f, 0.78f, 0.39f),
        TerrainType.Wall  => new Color(0.55f, 0.50f, 0.45f),
        TerrainType.Slope => new Color(0.72f, 0.65f, 0.48f),
        TerrainType.Water => new Color(0.30f, 0.55f, 0.90f),
        _                 => Color.white
    };

    private static Color FactionColor(Faction f) => f switch
    {
        Faction.Player  => new Color(0.2f, 0.4f, 0.9f, 0.85f),
        Faction.Enemy   => new Color(0.9f, 0.2f, 0.2f, 0.85f),
        _               => new Color(0.7f, 0.7f, 0.7f, 0.85f)
    };

    private static string BuildingChar(BuildingType b) => b switch
    {
        BuildingType.Castle      => "Ca",
        BuildingType.ArcheryRange => "Ar",
        BuildingType.Cathedral   => "Ch",
        BuildingType.House       => "Ho",
        _                        => "?"
    };

    private static string UnitChar(UnitType u) => u switch
    {
        UnitType.Warrior => "W",
        UnitType.Archer  => "A",
        UnitType.Lancer  => "L",
        UnitType.Monk    => "M",
        _                => "?"
    };

    private static Rect Shrink(Rect r, float amount) =>
        new Rect(r.x + amount, r.y + amount, r.width - amount * 2, r.height - amount * 2);

    private static void DrawBorder(Rect r, Color c)
    {
        var old = Handles.color;
        Handles.color = c;
        Handles.DrawPolyLine(
            new Vector3(r.xMin, r.yMin), new Vector3(r.xMax, r.yMin),
            new Vector3(r.xMax, r.yMax), new Vector3(r.xMin, r.yMax),
            new Vector3(r.xMin, r.yMin));
        Handles.color = old;
    }
}
