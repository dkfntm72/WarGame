using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class FixAllTerrainTiles
{
    public static void Execute()
    {
        // GameSettings 로드
        var guids = AssetDatabase.FindAssets("t:GameSettings");
        if (guids.Length == 0) { Debug.LogError("GameSettings not found"); return; }
        var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));

        // 타일 로드
        var grassTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/GameData/RuleTile_FlatGround.asset");
        var waterTile = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Background color.asset");
        var wallTile  = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/GameData/RuleTile_ElevatedGround.asset");

        // Slope용 Rule Tile 생성 (color2 - 모래/흙 색상)
        var slopeTile = CreateSlopeRuleTile();

        Undo.RecordObject(settings, "Fix All Terrain Tiles");
        settings.grassTile = grassTile;
        settings.waterTile = waterTile;
        settings.wallTile  = wallTile;
        settings.slopeTile = slopeTile;

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log("[WarGame] All terrain tiles assigned:\n" +
                  "  grassTile = RuleTile_FlatGround\n" +
                  "  waterTile = Water Background color\n" +
                  "  wallTile  = RuleTile_ElevatedGround\n" +
                  "  slopeTile = RuleTile_Slope (color2)");
    }

    static TileBase CreateSlopeRuleTile()
    {
        const string outPath = "Assets/GameData/RuleTile_Slope.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TileBase>(outPath);
        if (existing != null) return existing;

        const string sheet = "Assets/Tiny Swords/Terrain/Tileset/Tilemap_color2.png";
        const int T = RuleTile.TilingRule.Neighbor.This;
        const int X = RuleTile.TilingRule.Neighbor.NotThis;

        var tile = ScriptableObject.CreateInstance<RuleTile>();
        tile.m_DefaultSprite = LoadSprite(sheet, "Tilemap_color2_9");
        tile.m_TilingRules   = new System.Collections.Generic.List<RuleTile.TilingRule>();

        AddRule(tile, LoadSprite(sheet, "Tilemap_color2_0"),  X, T, T, X);
        AddRule(tile, LoadSprite(sheet, "Tilemap_color2_2"),  X, T, X, T);
        AddRule(tile, LoadSprite(sheet, "Tilemap_color2_16"), T, X, T, X);
        AddRule(tile, LoadSprite(sheet, "Tilemap_color2_18"), T, X, X, T);
        AddRule(tile, LoadSprite(sheet, "Tilemap_color2_1"),  X, T, T, T);
        AddRule(tile, LoadSprite(sheet, "Tilemap_color2_17"), T, X, T, T);
        AddRule(tile, LoadSprite(sheet, "Tilemap_color2_8"),  T, T, T, X);
        AddRule(tile, LoadSprite(sheet, "Tilemap_color2_10"), T, T, X, T);
        AddRule(tile, LoadSprite(sheet, "Tilemap_color2_3"),  X, X, X, X);

        AssetDatabase.CreateAsset(tile, outPath);
        return tile;
    }

    static Sprite LoadSprite(string path, string name)
    {
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
            if (a is Sprite s && s.name == name) return s;
        Debug.LogWarning($"[WarGame] Sprite not found: {name}");
        return null;
    }

    static void AddRule(RuleTile tile, Sprite sprite, int n, int s, int e, int w)
    {
        if (sprite == null) return;
        var rule = new RuleTile.TilingRule();
        rule.m_Sprites           = new Sprite[] { sprite };
        rule.m_Output            = RuleTile.TilingRule.OutputSprite.Single;
        rule.m_NeighborPositions = new System.Collections.Generic.List<Vector3Int>();
        rule.m_Neighbors         = new System.Collections.Generic.List<int>();

        void Add(int dx, int dy, int val)
        { rule.m_NeighborPositions.Add(new Vector3Int(dx, dy, 0)); rule.m_Neighbors.Add(val); }

        if (n != 0) Add( 0,  1, n);
        if (s != 0) Add( 0, -1, s);
        if (e != 0) Add( 1,  0, e);
        if (w != 0) Add(-1,  0, w);

        tile.m_TilingRules.Add(rule);
    }
}
