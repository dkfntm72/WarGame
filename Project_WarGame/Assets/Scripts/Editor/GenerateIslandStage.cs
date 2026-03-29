using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class GenerateIslandStage
{
    [MenuItem("Window/WarGame/Generate Island Stage")]
    public static void Run() => Execute();

    // G=Grass, W=Water
    // [y][x], y=0 is bottom row
    static readonly int[,] Layout = new int[12, 12]
    {
        { 1,1,1,1,1,1,1,1,1,1,1,1 }, // y=0  (bottom)
        { 1,1,1,0,0,0,0,0,0,1,1,1 }, // y=1
        { 1,1,0,0,0,0,0,0,0,0,1,1 }, // y=2
        { 1,0,0,0,0,0,0,0,0,0,0,1 }, // y=3
        { 1,0,0,0,0,0,0,0,0,0,0,1 }, // y=4
        { 1,0,0,0,0,0,0,0,0,0,0,1 }, // y=5
        { 1,0,0,0,0,0,0,0,0,0,0,1 }, // y=6
        { 1,0,0,0,0,0,0,0,0,0,0,1 }, // y=7
        { 1,0,0,0,0,0,0,0,0,0,0,1 }, // y=8
        { 1,1,0,0,0,0,0,0,0,0,1,1 }, // y=9
        { 1,1,1,0,0,0,0,0,0,1,1,1 }, // y=10
        { 1,1,1,1,1,1,1,1,1,1,1,1 }, // y=11 (top)
    };
    // 0=Grass(FlatGround), 1=Water

    static bool IsWater(int x, int y)
    {
        if (x < 0 || x >= 12 || y < 0 || y >= 12) return false;
        return Layout[y, x] == 1;
    }

    static bool IsAdjacentToWater(int x, int y)
    {
        return IsWater(x - 1, y) || IsWater(x + 1, y) || IsWater(x, y - 1) || IsWater(x, y + 1);
    }

    public static void Execute()
    {
        const int W = 12, H = 12;

        // ── MapData 로드 ───────────────────────────────────────────
        var mapGuids = AssetDatabase.FindAssets("t:MapData Stage01");
        if (mapGuids.Length == 0) { Debug.LogError("Stage01 MapData not found"); return; }
        var mapData = AssetDatabase.LoadAssetAtPath<MapData>(AssetDatabase.GUIDToAssetPath(mapGuids[0]));

        Undo.RecordObject(mapData, "Generate Island Stage");
        mapData.width  = W;
        mapData.height = H;
        mapData.InitializeTerrain();

        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                mapData.SetTerrain(x, y, Layout[y, x] == 1 ? TerrainType.Water : TerrainType.Grass);

        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();

        // ── 타일맵 적용 ───────────────────────────────────────────
        var gm = Object.FindFirstObjectByType<GridManager>();
        if (gm == null) { Debug.LogError("GridManager not found in scene"); return; }

        var tilemap = gm.terrainTilemap;
        tilemap.ClearAllTiles();

        // Rule Tile (Grass)
        var grassTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/GameData/RuleTile_FlatGround.asset");
        // Water Background Tile (실제 물 배경)
        var waterTile = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Background color.asset");

        if (grassTile == null) { Debug.LogError("RuleTile_FlatGround.asset not found"); return; }
        if (waterTile == null) { Debug.LogError("Water Tile animated.asset not found"); return; }

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                var pos  = new Vector3Int(x, y, 0);
                var tile = Layout[y, x] == 1 ? waterTile : grassTile;
                tilemap.SetTile(pos, tile);
            }
        }

        // ── Water Foam 레이어 ──────────────────────────────────────
        var foamTile = GetOrCreateWaterFoamTile();
        if (foamTile != null)
        {
            var foamTilemap = GetOrCreateFoamTilemap(gm.grid);
            foamTilemap.ClearAllTiles();

            // 물과 인접한 잔디 타일 위치에 foam 배치
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (Layout[y, x] == 0 && IsAdjacentToWater(x, y))
                        foamTilemap.SetTile(new Vector3Int(x, y, 0), foamTile);
        }

        // 타일맵 갱신
        tilemap.RefreshAllTiles();

        // 씬 저장
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[WarGame] 12x12 Island stage generated!");
    }

    static TileBase GetOrCreateWaterFoamTile()
    {
        const string assetPath = "Assets/GameData/WaterFoam.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
        if (existing != null) return existing;

        // Water Foam_0 스프라이트 로드
        var allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Tiny Swords/Terrain/Tileset/Water Foam.png");
        Sprite foamSprite = null;
        foreach (var obj in allAssets)
        {
            if (obj is Sprite s && s.name == "Water Foam_0")
            {
                foamSprite = s;
                break;
            }
        }

        if (foamSprite == null) { Debug.LogError("Water Foam_0 sprite not found"); return null; }

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = foamSprite;
        tile.colliderType = Tile.ColliderType.None;
        AssetDatabase.CreateAsset(tile, assetPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[WarGame] WaterFoam.asset created at " + assetPath);
        return tile;
    }

    static Tilemap GetOrCreateFoamTilemap(Grid grid)
    {
        // 기존 WaterFoamTilemap 찾기
        foreach (Transform child in grid.transform)
        {
            if (child.name == "WaterFoamTilemap")
            {
                var tm = child.GetComponent<Tilemap>();
                if (tm != null) return tm;
            }
        }

        // 새로 생성 (TerrainTilemap 아래 레이어)
        var go = new GameObject("WaterFoamTilemap");
        go.transform.SetParent(grid.transform, false);

        var tilemap  = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = -1; // TerrainTilemap(0)보다 낮게

        Undo.RegisterCreatedObjectUndo(go, "Create WaterFoamTilemap");
        Debug.Log("[WarGame] WaterFoamTilemap created");
        return tilemap;
    }
}
