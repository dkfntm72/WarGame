using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Data")]
    public GameSettings settings;
    public MapData      currentMap;

    [Header("Scene References")]
    public GridManager     gridManager;
    public ResourceManager resourceManager;
    public TurnManager     turnManager;

    public List<Unit>     PlayerUnits { get; } = new();
    public List<Unit>     EnemyUnits  { get; } = new();
    public List<Unit>     AllyUnits   { get; } = new();
    public List<Building> Buildings   { get; } = new();

    // Units unlocked for production at the castle
    public HashSet<UnitType> PlayerUnlockedUnits { get; } = new() { UnitType.Warrior, UnitType.Lancer };
    public HashSet<UnitType> EnemyUnlockedUnits  { get; } = new() { UnitType.Warrior, UnitType.Lancer };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (currentMap == null)
        {
            Debug.LogError("GameManager: No MapData assigned!");
            return;
        }
        LoadMap();
    }

    private void LoadMap()
    {
        Debug.Log($"[GameManager] LoadMap 시작: {currentMap.scenarioName} ({currentMap.width}x{currentMap.height})");
        Debug.Log($"[GameManager] settings={settings != null}, prefab={settings?.unitPrefab != null}, buildingPrefab={settings?.buildingPrefab != null}");

        gridManager.Initialize(currentMap, settings);
        resourceManager.Initialize(currentMap.playerStartGold, currentMap.enemyStartGold);

        Debug.Log($"[GameManager] 건물 배치: {currentMap.buildings?.Length ?? 0}개");
        foreach (var bp in currentMap.buildings)
        {
            var tile = gridManager.GetTile(bp.x, bp.y);
            if (tile == null) { Debug.LogWarning($"[GameManager] 건물 타일 null: ({bp.x},{bp.y})"); continue; }

            var bData    = settings.GetBuildingData(bp.buildingType);
            var go       = Instantiate(settings.buildingPrefab);
            var building = go.GetComponent<Building>();
            building.Initialize(bData, bp.faction, tile);
            Buildings.Add(building);
            ApplyBuildingUnlocks(building);
            Debug.Log($"[GameManager] 건물 배치 완료: {bData.buildingName} [{bp.faction}] ({bp.x},{bp.y})");
        }

        Debug.Log($"[GameManager] 유닛 배치: {currentMap.units?.Length ?? 0}개");
        foreach (var up in currentMap.units)
        {
            var tile = gridManager.GetTile(up.x, up.y);
            if (tile == null || tile.HasUnit) { Debug.LogWarning($"[GameManager] 유닛 타일 스킵: ({up.x},{up.y})"); continue; }
            var unit = SpawnUnit(settings.GetUnitData(up.unitType), up.faction, tile, up.rank, up.detectRange);
            Debug.Log($"[GameManager] 유닛 배치 완료: {unit?.data.unitName} [{up.faction}] ({up.x},{up.y})");
        }

        Debug.Log($"[GameManager] LoadMap 완료 - 플레이어:{PlayerUnits.Count} 적:{EnemyUnits.Count} 건물:{Buildings.Count}");

        CameraDragController.Instance?.PositionAtMapEdge(currentMap.cameraStartEdge);

        EventTriggerManager.Instance?.Initialize(currentMap);

        turnManager.StartGame();
    }

    public Unit SpawnUnit(UnitData uData, Faction faction, TileNode tile, int rank = 0, int detectRange = 0)
    {
        if (tile == null || tile.HasUnit) return null;
        if (uData == null) { Debug.LogError($"[GameManager] SpawnUnit: uData가 null입니다. faction={faction} tile=({tile.X},{tile.Y})"); return null; }

        var go   = Instantiate(settings.unitPrefab);
        if (go.GetComponent<RankBar>() == null) go.AddComponent<RankBar>();
        var unit = go.GetComponent<Unit>();
        unit.Initialize(uData, faction, tile);
        if (rank > 0) unit.SetRank(Mathf.Clamp(rank, 0, 5));
        if (detectRange > 0) unit.SetDetectRange(detectRange);
        unit.OnDied += HandleUnitDied;

        if (faction == Faction.Player)      PlayerUnits.Add(unit);
        else if (faction == Faction.Enemy)  EnemyUnits.Add(unit);
        else if (faction == Faction.Ally)   AllyUnits.Add(unit);

        return unit;
    }

    public void CaptureBuilding(Building building, Faction newFaction)
    {
        var old = building.faction;
        building.Capture(newFaction);

        // Rebuild unlock lists if ownership changed
        if (old == Faction.Player || newFaction == Faction.Player)
            RebuildUnlocks(Faction.Player);
        if (old == Faction.Enemy || newFaction == Faction.Enemy)
            RebuildUnlocks(Faction.Enemy);

        turnManager.CheckVictoryConditions();
    }

    private void HandleUnitDied(Unit unit)
    {
        PlayerUnits.Remove(unit);
        EnemyUnits.Remove(unit);
        AllyUnits.Remove(unit);
        turnManager.CheckVictoryConditions();
    }

    private void ApplyBuildingUnlocks(Building building)
    {
        var unlocks = building.faction == Faction.Player ? PlayerUnlockedUnits : EnemyUnlockedUnits;
        if (building.faction == Faction.Neutral) return;

        switch (building.data.buildingType)
        {
            case BuildingType.ArcheryRange: unlocks.Add(UnitType.Archer); break;
            case BuildingType.Cathedral:    unlocks.Add(UnitType.Monk);   break;
        }
    }

    private void RebuildUnlocks(Faction faction)
    {
        var set = faction == Faction.Player ? PlayerUnlockedUnits : EnemyUnlockedUnits;
        set.Clear();
        set.Add(UnitType.Warrior);
        set.Add(UnitType.Lancer);
        foreach (var b in Buildings)
            if (b.faction == faction) ApplyBuildingUnlocks(b);
    }
}
