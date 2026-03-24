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
    public TileBase wallTile;
    public TileBase slopeTile;
    public TileBase treeTile;
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
        TerrainType.Tree  => treeTile,
        TerrainType.Water => waterTile,
        _                 => grassTile
    };
}
