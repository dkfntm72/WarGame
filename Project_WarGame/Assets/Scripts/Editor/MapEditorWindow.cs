using UnityEngine;
using UnityEditor;
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

        GUILayout.Space(10);

        // Zoom
        GUILayout.Label("Zoom", GUILayout.Width(40));
        cellSize = EditorGUILayout.Slider(cellSize, 20f, 80f, GUILayout.Width(120));

        GUILayout.FlexibleSpace();

        if (mapData != null)
        {
            GUILayout.Label($"Map: {mapData.scenarioName}  ({mapData.width}x{mapData.height})", EditorStyles.miniLabel);
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
            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                { alignment = TextAnchor.MiddleCenter, fontSize = 8 };
            EditorGUI.LabelField(r, BuildingChar(bp.buildingType), labelStyle);
        }

        // Unit marker
        var up = units.Find(u => u.x == x && u.y == y);
        if (up != null)
        {
            var unitRect = Shrink(r, 10);
            EditorGUI.DrawRect(unitRect, FactionColor(up.faction));
            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                { alignment = TextAnchor.MiddleCenter, fontSize = 8 };
            EditorGUI.LabelField(r, UnitChar(up.unitType), labelStyle);
        }

        // Coordinates (small)
        if (cellSize >= 36)
        {
            var coordStyle = new GUIStyle(EditorStyles.miniLabel)
                { alignment = TextAnchor.LowerRight, fontSize = 7, normal = { textColor = new Color(0,0,0,0.4f) } };
            EditorGUI.LabelField(r, $"{x},{y}", coordStyle);
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
        TerrainType.Tree  => new Color(0.20f, 0.52f, 0.20f),
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
