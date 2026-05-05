public enum UnitType
{
    Warrior,
    Archer,
    Lancer,
    Monk,
    EliteWarrior, // 정예 전사 — 플레이어 전용, 생산 불가
    Tower         // 타워 — 이동 불가, 사거리 3, 생산 불가
}

public enum BuildingType
{
    Castle,
    ArcheryRange,
    Cathedral,
    House
}

public enum TerrainType
{
    Grass,
    Wall,
    Slope,
    Water,
    Grass2
}

public enum Faction
{
    Neutral,
    Player,
    Enemy,
    Ally   // 동맹군: 플레이어 미통제, 자동 행동, 적과 교전
}

public enum GameState
{
    PlayerTurn,
    AllyTurn,
    EnemyTurn,
    Victory,
    Defeat
}

public enum VictoryCondition
{
    AnnihilateEnemy,  // 적 유닛 전멸
    CaptureEnemyCastle // 적 성 점령
}

public enum CameraEdge
{
    BottomLeft,
    BottomRight,
    TopLeft,
    TopRight
}

public enum EventConditionType
{
    OnTileEnter,
    OnTurnStart,         // 플레이어 턴 N 시작
    OnEnemyTurnStart,    // 적 턴 N 시작
    OnStageStart,        // 스테이지 시작 시 (맵 로드 완료 직후)
    OnBuildingCapture,   // 특정 건물 점령 시 (x1,y1 위치의 건물, captureByFaction으로 팩션 필터)
}

public enum TriggerActionType
{
    ShowText,
    SpawnUnit
}
