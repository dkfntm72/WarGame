using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// TerrainRuleTile_Grass.asset 생성
/// 아래(S)가 Wall이면 절벽 상단 스프라이트(_20/_21/_22),
/// 아래(S)가 Water/빈칸이면 평지 엣지 스프라이트(_16/_17/_18) 사용.
/// </summary>
public class CreateCustomTerrainRuleTiles
{
    const int T = RuleTile.TilingRule.Neighbor.This;    // 1 : 같은 타일
    const int X = RuleTile.TilingRule.Neighbor.NotThis; // 2 : 다른 타일
    const int W = TerrainRuleTile.Neighbor.IsWall;      // 3 : 벽 타일
    const int A = TerrainRuleTile.Neighbor.IsWater;     // 4 : 물/빈칸

    public static void Execute()
    {
        const string sheet  = "Assets/Tiny Swords/Terrain/Tileset/Tilemap_color1.png";
        const string outDir = "Assets/GameData/";

        // 참조할 타일 로드
        var wallTile  = AssetDatabase.LoadAssetAtPath<TileBase>(outDir + "RuleTile_ElevatedGround.asset");
        var waterTile = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Background color.asset");

        if (wallTile  == null) { Debug.LogError("RuleTile_ElevatedGround.asset 없음 - 먼저 CreateTerrainRuleTiles 실행"); return; }
        if (waterTile == null) { Debug.LogError("Water Background color.asset 없음"); return; }

        // 기존 에셋 삭제 후 재생성
        var existingPath = outDir + "TerrainRuleTile_Grass.asset";
        if (AssetDatabase.LoadAssetAtPath<Object>(existingPath) != null)
            AssetDatabase.DeleteAsset(existingPath);

        var grass = ScriptableObject.CreateInstance<TerrainRuleTile>();
        grass.m_DefaultSprite = Sp(sheet, "Tilemap_color1_9"); // 중앙 (기본)
        grass.m_TilingRules   = new List<RuleTile.TilingRule>();
        grass.wallTile        = wallTile;
        grass.waterTile       = waterTile;

        // ── 절벽 상단 엣지 (S=Wall) ─ 높은 우선순위 ──────────────────
        // 아래가 벽이면 절벽 꼭대기 스프라이트 사용
        // BL 절벽 코너: 아래=벽, 왼쪽=벽(또는 다름)
        AddRule(grass, Sp(sheet, "Tilemap_color1_20"), T, W, T, X); // N=T, S=Wall, E=T, W=NotThis
        AddRule(grass, Sp(sheet, "Tilemap_color1_22"), T, W, X, T); // N=T, S=Wall, E=NotThis, W=T
        AddRule(grass, Sp(sheet, "Tilemap_color1_20"), X, W, T, X); // N=X, S=Wall, W=NotThis
        AddRule(grass, Sp(sheet, "Tilemap_color1_22"), X, W, X, T); // N=X, S=Wall, E=NotThis
        AddRule(grass, Sp(sheet, "Tilemap_color1_21"), 0, W, 0, 0); // S=Wall (일반 - 모든 나머지)

        // ── 평지 코너 (NotThis = 물/빈칸) ────────────────────────────
        AddRule(grass, Sp(sheet, "Tilemap_color1_0"),  X, T, T, X); // TL
        AddRule(grass, Sp(sheet, "Tilemap_color1_2"),  X, T, X, T); // TR
        AddRule(grass, Sp(sheet, "Tilemap_color1_16"), T, X, T, X); // BL (물)
        AddRule(grass, Sp(sheet, "Tilemap_color1_18"), T, X, X, T); // BR (물)

        // ── 평지 엣지 ────────────────────────────────────────────────
        AddRule(grass, Sp(sheet, "Tilemap_color1_1"),  X, T, T, T); // Top
        AddRule(grass, Sp(sheet, "Tilemap_color1_17"), T, X, T, T); // Bottom (물)
        AddRule(grass, Sp(sheet, "Tilemap_color1_8"),  T, T, T, X); // Left
        AddRule(grass, Sp(sheet, "Tilemap_color1_10"), T, T, X, T); // Right

        // ── 고립 ─────────────────────────────────────────────────────
        AddRule(grass, Sp(sheet, "Tilemap_color1_3"),  X, X, X, X);

        AssetDatabase.CreateAsset(grass, existingPath);

        // GameSettings 업데이트
        var guids = AssetDatabase.FindAssets("t:GameSettings");
        if (guids.Length > 0)
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Undo.RecordObject(settings, "Update grassTile");
            settings.grassTile = grass;
            EditorUtility.SetDirty(settings);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[WarGame] TerrainRuleTile_Grass.asset 생성 완료\n" +
                  "  S=Wall  → 절벽 상단 스프라이트 (_20/_21/_22)\n" +
                  "  S=Water → 평지 엣지 스프라이트 (_16/_17/_18)");
    }

    static Sprite Sp(string path, string name)
    {
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
            if (a is Sprite s && s.name == name) return s;
        Debug.LogWarning($"[RuleTile] Sprite not found: {name}");
        return null;
    }

    static void AddRule(RuleTile tile, Sprite sprite, int n, int s, int e, int w)
    {
        if (sprite == null) return;
        var rule = new RuleTile.TilingRule();
        rule.m_Sprites           = new Sprite[] { sprite };
        rule.m_Output            = RuleTile.TilingRule.OutputSprite.Single;
        rule.m_NeighborPositions = new List<Vector3Int>();
        rule.m_Neighbors         = new List<int>();

        void Add(int dx, int dy, int val)
        { rule.m_NeighborPositions.Add(new Vector3Int(dx, dy, 0)); rule.m_Neighbors.Add(val); }

        if (n != 0) Add( 0,  1, n);
        if (s != 0) Add( 0, -1, s);
        if (e != 0) Add( 1,  0, e);
        if (w != 0) Add(-1,  0, w);

        tile.m_TilingRules.Add(rule);
    }
}
