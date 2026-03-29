using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 이웃 타일 종류를 구분할 수 있는 커스텀 RuleTile.
/// IsWall / IsWater 조건을 추가해 절벽 엣지 스프라이트를 정확하게 선택한다.
/// </summary>
[CreateAssetMenu(fileName = "TerrainRuleTile", menuName = "WarGame/Terrain Rule Tile")]
public class TerrainRuleTile : RuleTile<TerrainRuleTile.Neighbor>
{
    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int IsWall  = 3;   // 이웃이 Wall 타일
        public const int IsWater = 4;   // 이웃이 Water 타일 (또는 빈칸)
    }

    [Header("Neighbor Tile References")]
    public TileBase wallTile;
    public TileBase waterTile;

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.This:    return tile == this;
            case Neighbor.NotThis: return tile != this;
            case Neighbor.IsWall:  return wallTile  != null && tile == wallTile;
            case Neighbor.IsWater: return tile == waterTile || tile == null;
        }
        return base.RuleMatch(neighbor, tile);
    }
}
