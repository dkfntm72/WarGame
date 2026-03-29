using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "GameSettings", menuName = "WarGame/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Unit Data (index = UnitType)")]
    public UnitData[] unitDataList;

    [Header("Building Data (index = BuildingType)")]
    public BuildingData[] buildingDataList;

    [Header("Terrain Tiles")]
    public TileBase grassTile;
    public TileBase wallTile;        // 벽-풀 (하단이 풀/경사/등)
    public TileBase wallOnWaterTile; // 벽-물 (하단이 물)
    public TileBase slopeTile;
    public TileBase waterTile;

    [Header("Highlight Tiles")]
    public TileBase moveHighlightTile;
    public TileBase attackHighlightTile;

    [Header("Prefabs")]
    public GameObject unitPrefab;
    public GameObject buildingPrefab;

    public UnitData GetUnitData(UnitType type)     => unitDataList[(int)type];
    public BuildingData GetBuildingData(BuildingType type) => buildingDataList[(int)type];

    public TileBase GetTerrainTile(TerrainType type) => type switch
    {
        TerrainType.Wall  => wallTile,
        TerrainType.Slope => slopeTile,
        TerrainType.Water => waterTile,
        _                 => grassTile
    };

    // 아래 셀 지형을 고려한 Wall 타일 반환
    public TileBase GetWallTile(TerrainType belowTerrain) =>
        belowTerrain == TerrainType.Water ? (wallOnWaterTile != null ? wallOnWaterTile : wallTile) : wallTile;
}
