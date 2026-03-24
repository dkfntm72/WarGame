using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New MapData", menuName = "WarGame/Map Data")]
public class MapData : ScriptableObject
{
    public int width = 10;
    public int height = 8;
    public string scenarioName = "New Scenario";

    [TextArea(3, 10)]
    public string scenarioStory;

    public int playerStartGold = 100;
    public int enemyStartGold = 100;

    // Terrain indexed by y * width + x
    public TerrainType[] terrain;
    public BuildingPlacement[] buildings = new BuildingPlacement[0];
    public UnitPlacement[] units = new UnitPlacement[0];

    public void InitializeTerrain()
    {
        terrain = new TerrainType[width * height];
    }

    public TerrainType GetTerrain(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return TerrainType.Water;
        if (terrain == null || terrain.Length != width * height) return TerrainType.Grass;
        return terrain[y * width + x];
    }

    public void SetTerrain(int x, int y, TerrainType type)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (terrain == null || terrain.Length != width * height)
            terrain = new TerrainType[width * height];
        terrain[y * width + x] = type;
    }
}

[Serializable]
public class BuildingPlacement
{
    public int x, y;
    public BuildingType buildingType;
    public Faction faction;
}

[Serializable]
public class UnitPlacement
{
    public int x, y;
    public UnitType unitType;
    public Faction faction;
}
