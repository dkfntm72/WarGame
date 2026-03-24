using UnityEngine;
using System;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public int       TurnNumber   { get; private set; }
    public GameState CurrentState { get; private set; }

    public event Action OnPlayerTurnStart;
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
    }

    public void EndPlayerTurn()
    {
        if (CurrentState != GameState.PlayerTurn) return;
        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        CurrentState = GameState.EnemyTurn;

        foreach (var unit in GameManager.Instance.EnemyUnits)
            unit.ResetTurn();

        OnEnemyTurnStart?.Invoke();

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

        if (!playerHasCastle && !playerHasUnits)
        {
            CurrentState = GameState.Defeat;
            OnDefeat?.Invoke();
        }
        else if (!enemyHasCastle || !enemyHasUnits)
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
