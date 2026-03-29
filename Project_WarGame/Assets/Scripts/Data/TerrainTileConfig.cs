using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 이웃 조건에 따라 다른 타일을 반환하는 설정.
/// 규칙은 위에서부터 순서대로 평가하며, 처음 일치하는 규칙의 타일을 사용한다.
/// </summary>
[CreateAssetMenu(fileName = "TerrainTileConfig", menuName = "WarGame/Terrain Tile Config")]
public class TerrainTileConfig : ScriptableObject
{
    public TerrainType terrainType;
    public TileBase    defaultTile;
    public List<TileRule> rules = new();

    /// <summary>
    /// neighbors8: N, NE, E, SE, S, SW, W, NW 순서의 TerrainType 배열
    /// </summary>
    public TileBase Resolve(TerrainType[] neighbors8)
    {
        foreach (var rule in rules)
        {
            if (rule.tile == null) continue;   // null 타일 규칙 건너뜀
            if (rule.Matches(terrainType, neighbors8))
                return rule.tile;
        }
        return defaultTile;
    }
}

// ──────────────────────────────────────────────────────────────────────────
// 이웃 한 칸의 조건
// Any=-1, Grass/Wall/Slope/Water = TerrainType과 int 값 동일 (기존 에셋 호환)
// Same=-2 / NotSame=-3: 자신 지형 기준 상대 조건 (신규)
// ──────────────────────────────────────────────────────────────────────────
public enum NeighborReq
{
    NotSame = -3,  // 자신과 다른 지형
    Same    = -2,  // 자신과 같은 지형
    Any     = -1,  // 조건 없음
    Grass   =  0,
    Wall    =  1,
    Slope   =  2,
    Water   =  3,
}

// ──────────────────────────────────────────────────────────────────────────
// 규칙 하나 (이웃 조건 8개 + 출력 타일)
// ──────────────────────────────────────────────────────────────────────────
[Serializable]
public class TileRule
{
    // 인덱스: 0=N, 1=NE, 2=E, 3=SE, 4=S, 5=SW, 6=W, 7=NW
    public NeighborReq[] neighbors = new NeighborReq[8]
    {
        NeighborReq.Any, NeighborReq.Any, NeighborReq.Any, NeighborReq.Any,
        NeighborReq.Any, NeighborReq.Any, NeighborReq.Any, NeighborReq.Any
    };
    public TileBase tile;

    public bool Matches(TerrainType self, TerrainType[] n)
    {
        for (int i = 0; i < 8; i++)
        {
            switch (neighbors[i])
            {
                case NeighborReq.Any:     break;
                case NeighborReq.Same:    if (n[i] != self) return false; break;
                case NeighborReq.NotSame: if (n[i] == self) return false; break;
                default:
                    if ((int)neighbors[i] != (int)n[i]) return false; break;
            }
        }
        return true;
    }
}
