using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// 벽 Rule Tile 2종 생성
///   RuleTile_Wall_Grass.asset  : 벽 하단이 풀/지면 (_34~37)
///   RuleTile_Wall_Water.asset  : 벽 하단이 물 (_40~43)
/// Window > WarGame > Create Wall Rule Tiles
/// </summary>
public class CreateWallRuleTiles
{
    [MenuItem("Window/WarGame/Create Wall Rule Tiles")]
    public static void Execute()
    {
        const string sheet  = "Assets/Tiny Swords/Terrain/Tileset/Tilemap_color1.png";
        const string outDir = "Assets/GameData/";

        Build(sheet, outDir, "RuleTile_Wall_Grass.asset",
            "Tilemap_color1_34", "Tilemap_color1_35",
            "Tilemap_color1_36", "Tilemap_color1_37");

        Build(sheet, outDir, "RuleTile_Wall_Water.asset",
            "Tilemap_color1_40", "Tilemap_color1_41",
            "Tilemap_color1_42", "Tilemap_color1_43");

        // GameSettings 자동 연결
        var guids = AssetDatabase.FindAssets("t:GameSettings");
        if (guids.Length > 0)
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
            Undo.RecordObject(settings, "Update Wall Tiles");
            settings.wallTile        = AssetDatabase.LoadAssetAtPath<TileBase>(outDir + "RuleTile_Wall_Grass.asset");
            settings.wallOnWaterTile = AssetDatabase.LoadAssetAtPath<TileBase>(outDir + "RuleTile_Wall_Water.asset");
            EditorUtility.SetDirty(settings);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done",
            "Wall Rule Tiles 생성 완료!\n" +
            "  RuleTile_Wall_Grass.asset  (_34~37)\n" +
            "  RuleTile_Wall_Water.asset  (_40~43)\n" +
            "GameSettings 자동 연결됨.", "OK");
    }

    // ── 타일 4종 → RuleTile 생성 ──────────────────────────────────
    // left: 왼쪽 끝,  centerL/centerR: 가운데(랜덤),  right: 오른쪽 끝
    static void Build(string sheet, string outDir, string filename,
        string left, string centerL, string centerR, string right)
    {
        const int T = RuleTile.TilingRule.Neighbor.This;
        const int X = RuleTile.TilingRule.Neighbor.NotThis;

        var path = outDir + filename;
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var tile = ScriptableObject.CreateInstance<RuleTile>();
        tile.m_DefaultSprite = Sp(sheet, centerL);
        tile.m_TilingRules   = new List<RuleTile.TilingRule>();

        // 왼쪽 끝 (왼쪽=빈칸, 오른쪽=같은타일)
        AddRule(tile, Sp(sheet, left),   X, T);
        // 오른쪽 끝 (왼쪽=같은타일, 오른쪽=빈칸)
        AddRule(tile, Sp(sheet, right),  T, X);
        // 단독 (양쪽 모두 빈칸)
        AddRule(tile, Sp(sheet, left),   X, X);
        // 가운데 (양쪽 모두 같은타일) → centerL/centerR 랜덤
        AddRuleRandom(tile, Sp(sheet, centerL), Sp(sheet, centerR));

        AssetDatabase.CreateAsset(tile, path);
    }

    static void AddRule(RuleTile tile, Sprite sprite, int w, int e)
    {
        if (sprite == null) return;
        var rule = new RuleTile.TilingRule();
        rule.m_Sprites           = new Sprite[] { sprite };
        rule.m_Output            = RuleTile.TilingRule.OutputSprite.Single;
        rule.m_NeighborPositions = new List<Vector3Int>();
        rule.m_Neighbors         = new List<int>();
        rule.m_NeighborPositions.Add(new Vector3Int(-1, 0, 0)); rule.m_Neighbors.Add(w);
        rule.m_NeighborPositions.Add(new Vector3Int(1,  0, 0)); rule.m_Neighbors.Add(e);
        tile.m_TilingRules.Add(rule);
    }

    static void AddRuleRandom(RuleTile tile, Sprite s1, Sprite s2)
    {
        if (s1 == null || s2 == null) return;
        const int T = RuleTile.TilingRule.Neighbor.This;
        var rule = new RuleTile.TilingRule();
        rule.m_Sprites           = new Sprite[] { s1, s2 };
        rule.m_Output            = RuleTile.TilingRule.OutputSprite.Random;
        rule.m_NeighborPositions = new List<Vector3Int>();
        rule.m_Neighbors         = new List<int>();
        rule.m_NeighborPositions.Add(new Vector3Int(-1, 0, 0)); rule.m_Neighbors.Add(T);
        rule.m_NeighborPositions.Add(new Vector3Int(1,  0, 0)); rule.m_Neighbors.Add(T);
        tile.m_TilingRules.Add(rule);
    }

    static Sprite Sp(string path, string name)
    {
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
            if (a is Sprite s && s.name == name) return s;
        Debug.LogWarning($"[WallRuleTile] Sprite not found: {name}");
        return null;
    }
}
