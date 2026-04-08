using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventTriggerManager : MonoBehaviour
{
    public static EventTriggerManager Instance { get; private set; }

    private List<EventTrigger> _triggers = new();
    private HashSet<int>       _firedOnce = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(MapData mapData)
    {
        _triggers.Clear();
        _firedOnce.Clear();

        if (mapData.eventTriggers != null)
            _triggers.AddRange(mapData.eventTriggers);
    }

    // Called from PlayerInputHandler.MoveAndFollowUp after unit.MoveTo completes
    public void OnUnitEnteredTile(Unit unit, TileNode tile)
    {
        foreach (var trigger in _triggers)
        {
            if (trigger.conditionType != EventConditionType.OnTileEnter) continue;
            if (!trigger.ContainsTile(tile.X, tile.Y)) continue;
            if (trigger.fireOnce && _firedOnce.Contains(trigger.id)) continue;
            StartCoroutine(ExecuteTrigger(trigger));
        }
    }

    // Called from TurnManager.StartPlayerTurn
    public void OnTurnStart(int turnNumber)
    {
        foreach (var trigger in _triggers)
        {
            if (trigger.conditionType != EventConditionType.OnTurnStart) continue;
            if (trigger.turnNumber != turnNumber) continue;
            if (trigger.fireOnce && _firedOnce.Contains(trigger.id)) continue;
            StartCoroutine(ExecuteTrigger(trigger));
        }
    }

    // Called from TurnManager.StartEnemyTurn
    public void OnEnemyTurnStart(int turnNumber)
    {
        foreach (var trigger in _triggers)
        {
            if (trigger.conditionType != EventConditionType.OnEnemyTurnStart) continue;
            if (trigger.turnNumber != turnNumber) continue;
            if (trigger.fireOnce && _firedOnce.Contains(trigger.id)) continue;
            StartCoroutine(ExecuteTrigger(trigger));
        }
    }


    private IEnumerator ExecuteTrigger(EventTrigger trigger)
    {
        if (trigger.fireOnce)
            _firedOnce.Add(trigger.id);

        foreach (var action in trigger.actions)
            yield return StartCoroutine(ExecuteAction(action));
    }

    private IEnumerator ExecuteAction(TriggerAction action)
    {
        switch (action.actionType)
        {
            case TriggerActionType.ShowText:
            {
                bool confirmed = false;
                GameUI.Instance.ShowEventText(action.text, () => confirmed = true);
                yield return new WaitUntil(() => confirmed);
                break;
            }
            case TriggerActionType.SpawnUnit:
            {
                var tile = GridManager.Instance.GetTile(action.spawnX, action.spawnY);
                if (tile != null && !tile.HasUnit)
                {
                    var uData = GameManager.Instance.settings.GetUnitData(action.unitType);
                    GameManager.Instance.SpawnUnit(uData, action.faction, tile, action.spawnRank);
                }
                break;
            }
        }
    }
}
