using UnityEngine;
using System;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public int       TurnNumber   { get; private set; }
    public GameState CurrentState { get; private set; }

    public event Action OnPlayerTurnStart;
    public event Action OnAllyTurnStart;
    public event Action OnEnemyTurnStart;
    public event Action OnVictory;
    public event Action OnDefeat;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartGame()
    {
        TurnNumber = 1;
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        CurrentState = GameState.PlayerTurn;

        foreach (var unit in GameManager.Instance.PlayerUnits)
            unit.ResetTurn();

        CollectGold();
        OnPlayerTurnStart?.Invoke();
        EventTriggerManager.Instance?.OnTurnStart(TurnNumber);
    }

    public void EndPlayerTurn()
    {
        if (CurrentState != GameState.PlayerTurn) return;
        // 동맹군이 있으면 동맹군 턴, 없으면 바로 적 턴
        if (GameManager.Instance.AllyUnits.Count > 0)
            StartAllyTurn();
        else
            StartEnemyTurn();
    }

    public void StartAllyTurn()
    {
        CurrentState = GameState.AllyTurn;

        foreach (var unit in GameManager.Instance.AllyUnits)
            unit.ResetTurn();

        OnAllyTurnStart?.Invoke();

        if (AllyAI.Instance != null)
            AllyAI.Instance.StartCoroutine(AllyAI.Instance.ExecuteTurn());
        else
            EndAllyTurn();
    }

    public void EndAllyTurn()
    {
        if (CurrentState != GameState.AllyTurn) return;
        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        CurrentState = GameState.EnemyTurn;

        foreach (var unit in GameManager.Instance.EnemyUnits)
            unit.ResetTurn();

        OnEnemyTurnStart?.Invoke();
        EventTriggerManager.Instance?.OnEnemyTurnStart(TurnNumber);

        // Trigger AI
        if (EnemyAI.Instance != null)
            EnemyAI.Instance.StartCoroutine(EnemyAI.Instance.ExecuteTurn());
    }

    public void EndEnemyTurn()
    {
        if (CurrentState != GameState.EnemyTurn) return;
        TurnNumber++;
        CheckVictoryConditions();
        if (CurrentState == GameState.EnemyTurn)
            StartPlayerTurn();
    }

    public void CheckVictoryConditions()
    {
        bool playerHasCastle = false;
        bool enemyHasCastle  = false;

        foreach (var b in GameManager.Instance.Buildings)
        {
            if (b.data.buildingType != BuildingType.Castle) continue;
            if (b.faction == Faction.Player) playerHasCastle = true;
            if (b.faction == Faction.Enemy)  enemyHasCastle  = true;
        }

        bool playerHasUnits = GameManager.Instance.PlayerUnits.Count > 0;
        bool enemyHasUnits  = GameManager.Instance.EnemyUnits.Count > 0;

        // 패배 조건: 아군 성 + 유닛 모두 소멸 (승리 조건과 무관하게 공통 적용)
        if (!playerHasCastle && !playerHasUnits)
        {
            CurrentState = GameState.Defeat;
            OnDefeat?.Invoke();
            return;
        }

        // 승리 조건: 맵 데이터에 따라 분기
        var condition = GameManager.Instance.currentMap.victoryCondition;
        bool victory = condition switch
        {
            VictoryCondition.AnnihilateEnemy    => !enemyHasUnits,
            VictoryCondition.CaptureEnemyCastle => !enemyHasCastle,
            _                                   => false
        };

        if (victory)
        {
            CurrentState = GameState.Victory;
            OnVictory?.Invoke();
        }
    }

    private void CollectGold()
    {
        foreach (var b in GameManager.Instance.Buildings)
        {
            int gold = b.GetGoldProduction();
            if (gold > 0 && b.faction != Faction.Neutral)
                ResourceManager.Instance.AddGold(b.faction, gold);
        }
    }
}
