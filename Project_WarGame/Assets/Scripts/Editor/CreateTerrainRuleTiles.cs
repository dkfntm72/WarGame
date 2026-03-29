using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Tiny Swords color1 기준 Rule Tile 생성
/// Flat Ground (left group): 순수 잔디, 물과 접함
/// Elevated Ground (right group): 절벽 상단 잔디
/// </summary>
public class CreateTerrainRuleTiles
{
    const int T = RuleTile.TilingRule.Neighbor.This;
    const int X = RuleTile.TilingRule.Neighbor.NotThis;

    static Sprite Sp(string sheetPath, string name)
    {
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(sheetPath))
            if (a is Sprite s && s.name == name) return s;
        Debug.LogWarning($"[RuleTile] Sprite not found: {name}");
        return null;
    }

    // n=north, s=south, e=east, w=west  |  T=same tile, X=different, 0=ignore
    static void AddRule(RuleTile tile, Sprite sprite, int n, int s, int e, int w)
    {
        if (sprite == null) return;
        var rule = new RuleTile.TilingRule();
        rule.m_Sprites = new Sprite[] { sprite };
        rule.m_Output = RuleTile.TilingRule.OutputSprite.Single;
        rule.m_NeighborPositions = new List<Vector3Int>();
        rule.m_Neighbors        = new List<int>();

        void Add(int dx, int dy, int val)
        { rule.m_NeighborPositions.Add(new Vector3Int(dx, dy, 0)); rule.m_Neighbors.Add(val); }

        if (n != 0) Add( 0,  1, n);
        if (s != 0) Add( 0, -1, s);
        if (e != 0) Add( 1,  0, e);
        if (w != 0) Add(-1,  0, w);

        tile.m_TilingRules.Add(rule);
    }

    public static void Execute()
    {
        const string sheet = "Assets/Tiny Swords/Terrain/Tileset/Tilemap_color1.png";
        const string outDir = "Assets/GameData/";

        // ── Flat Ground Rule Tile ──────────────────────────────────
        // Left group: cols 0-3
        // Row0: _0=TL, _1=Top,  _2=TR,  _3=Single
        // Row1: _8=L,  _9=Ctr, _10=R,  _11=?
        // Row2: _16=BL,_17=Bot,_18=BR, _19=?
        var flat = ScriptableObject.CreateInstance<RuleTile>();
        flat.m_DefaultSprite = Sp(sheet, "Tilemap_color1_9"); // Center (fallback)
        flat.m_TilingRules   = new List<RuleTile.TilingRule>();

        // Corners (all 4 dirs checked → most specific)
        AddRule(flat, Sp(sheet, "Tilemap_color1_0"),  X, T, T, X); // TL: N=X W=X
        AddRule(flat, Sp(sheet, "Tilemap_color1_2"),  X, T, X, T); // TR: N=X E=X
        AddRule(flat, Sp(sheet, "Tilemap_color1_16"), T, X, T, X); // BL: S=X W=X
        AddRule(flat, Sp(sheet, "Tilemap_color1_18"), T, X, X, T); // BR: S=X E=X

        // Edges (3 dirs checked)
        AddRule(flat, Sp(sheet, "Tilemap_color1_1"),  X, T, T, T); // Top:    N=X
        AddRule(flat, Sp(sheet, "Tilemap_color1_17"), T, X, T, T); // Bottom: S=X
        AddRule(flat, Sp(sheet, "Tilemap_color1_8"),  T, T, T, X); // Left:   W=X
        AddRule(flat, Sp(sheet, "Tilemap_color1_10"), T, T, X, T); // Right:  E=X

        // Isolated (no neighbors)
        AddRule(flat, Sp(sheet, "Tilemap_color1_3"),  X, X, X, X);

        // Center is the default sprite — no rule needed

        AssetDatabase.CreateAsset(flat, outDir + "RuleTile_FlatGround.asset");

        // ── Elevated Ground Rule Tile ──────────────────────────────
        // Right group: cols 5-8
        // Row0: _4=TL,  _5=Top,  _6=TR,  _7=Single
        // Row1: _12=L,  _13=Ctr, _14=R,  _15=?
        // Row2: _20=BL(cliff), _21=Bot(cliff), _22=BR(cliff)  ← cliff top
        var elev = ScriptableObject.CreateInstance<RuleTile>();
        elev.m_DefaultSprite = Sp(sheet, "Tilemap_color1_13");
        elev.m_TilingRules   = new List<RuleTile.TilingRule>();

        // Corners
        AddRule(elev, Sp(sheet, "Tilemap_color1_4"),  X, T, T, X); // TL
        AddRule(elev, Sp(sheet, "Tilemap_color1_6"),  X, T, X, T); // TR
        AddRule(elev, Sp(sheet, "Tilemap_color1_20"), T, X, T, X); // BL (cliff edge)
        AddRule(elev, Sp(sheet, "Tilemap_color1_22"), T, X, X, T); // BR (cliff edge)

        // Edges
        AddRule(elev, Sp(sheet, "Tilemap_color1_5"),  X, T, T, T); // Top
        AddRule(elev, Sp(sheet, "Tilemap_color1_21"), T, X, T, T); // Bottom (cliff center)
        AddRule(elev, Sp(sheet, "Tilemap_color1_12"), T, T, T, X); // Left
        AddRule(elev, Sp(sheet, "Tilemap_color1_14"), T, T, X, T); // Right

        // Isolated
        AddRule(elev, Sp(sheet, "Tilemap_color1_7"),  X, X, X, X);

        AssetDatabase.CreateAsset(elev, outDir + "RuleTile_ElevatedGround.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[WarGame] Rule Tiles created: RuleTile_FlatGround.asset / RuleTile_ElevatedGround.asset");
        EditorUtility.DisplayDialog("Done", "Rule Tiles 생성 완료!\nAssets/GameData/ 에서 확인하세요.", "OK");
    }
}
