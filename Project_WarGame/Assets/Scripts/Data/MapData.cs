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

    public CameraEdge cameraStartEdge = CameraEdge.BottomLeft;
    public VictoryCondition victoryCondition = VictoryCondition.AnnihilateEnemy;

    // Terrain indexed by y * width + x
    public TerrainType[] terrain;
    public BuildingPlacement[] buildings = new BuildingPlacement[0];
    public UnitPlacement[] units = new UnitPlacement[0];
    public EventTrigger[] eventTriggers = new EventTrigger[0];

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
public class TriggerAction
{
    public TriggerActionType actionType;
    // ShowText
    public string text;
    // SpawnUnit
    public UnitType unitType;
    public Faction  faction;
    public int      spawnX, spawnY;
    public int      spawnRank; // 0~5
}

[Serializable]
public class EventTrigger
{
    public int                id;
    public EventConditionType conditionType;
    // OnTileEnter — 단일 타일은 x1==x2, y1==y2로 설정
    public int x1, y1; // 범위 시작 (좌하단)
    public int x2, y2; // 범위 끝 (우상단)
    // OnTurnStart
    public int turnNumber;
    // common
    public bool           fireOnce = true;
    public TriggerAction[] actions = new TriggerAction[0];

    public bool ContainsTile(int x, int y) =>
        x >= x1 && x <= x2 && y >= y1 && y <= y2;
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
    public int rank;        // 0~5
    public int detectRange; // 0 = UnitData 기본값 사용
}
