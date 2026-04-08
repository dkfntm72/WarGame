using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public IEnumerator ExecuteTurn()
    {
        yield return new WaitForSeconds(0.4f);

        var units = new List<Unit>(GameManager.Instance.EnemyUnits);
        foreach (var unit in units)
        {
            if (!unit.IsAlive) continue;
            yield return StartCoroutine(ActUnit(unit));
            yield return new WaitForSeconds(0.3f);
        }

        TryProduceUnit();
        yield return new WaitForSeconds(0.3f);

        TurnManager.Instance.EndEnemyTurn();
    }

    private IEnumerator ActUnit(Unit unit)
    {
        var target = FindNearestHostileUnit(unit);
        if (target == null) { unit.SetExhausted(); yield break; }

        // Attack immediately if already in range
        if (unit.CanAct && InAttackRange(unit, target))
        {
            yield return StartCoroutine(PerformAttack(unit, target));
            yield break;
        }

        // Move toward target
        if (unit.CanMove)
        {
            var moveTiles = GridManager.Instance.GetMovementRange(
                unit.CurrentTile, unit.data.moveRange, Faction.Enemy);

            var bestTile = moveTiles
                .Where(t => !t.HasUnit)
                .OrderBy(t => ManhattanDist(t, target.CurrentTile))
                .FirstOrDefault();

            if (bestTile != null)
            {
                var path = Pathfinding.FindPath(GridManager.Instance, unit.CurrentTile, bestTile, Faction.Enemy);
                if (path.Count > 0)
                    yield return StartCoroutine(unit.MoveTo(bestTile, path));

                // Capture neutral/player buildings on landing
                if (bestTile.HasBuilding && bestTile.Building.faction != Faction.Enemy)
                    GameManager.Instance.CaptureBuilding(bestTile.Building, Faction.Enemy);
            }
        }

        // Attack after moving if now in range
        if (unit.CanAct)
        {
            var newTarget = FindNearestHostileUnit(unit);
            if (newTarget != null && InAttackRange(unit, newTarget))
                yield return StartCoroutine(PerformAttack(unit, newTarget));
            else
                unit.SetExhausted();
        }
    }

    private IEnumerator PerformAttack(Unit attacker, Unit target)
    {
        if (attacker.data.unitType == UnitType.Monk)
        {
            var ally = GameManager.Instance.EnemyUnits
                .Where(u => u != attacker && u.IsAlive && ManhattanDist(attacker.CurrentTile, u.CurrentTile) <= attacker.data.attackRange)
                .OrderBy(u => u.CurrentHp)
                .FirstOrDefault();
            if (ally != null) { yield return StartCoroutine(attacker.HealAlly(ally)); yield break; }
        }
        yield return StartCoroutine(attacker.Attack(target));
    }

    private void TryProduceUnit()
    {
        var castle = GameManager.Instance.Buildings
            .FirstOrDefault(b => b.data.buildingType == BuildingType.Castle && b.faction == Faction.Enemy);
        if (castle == null) return;

        var spawnTile = FindEmptyAdjacent(castle.tile);
        if (spawnTile == null) return;

        var uData = GameManager.Instance.settings.GetUnitData(UnitType.Warrior);
        if (ResourceManager.Instance.SpendGold(Faction.Enemy, uData.goldCost))
            GameManager.Instance.SpawnUnit(uData, Faction.Enemy, spawnTile);
    }

    // 적에게 적대적인 유닛(플레이어 + 동맹군) 중 가장 가까운 유닛
    private Unit FindNearestHostileUnit(Unit from) =>
        GameManager.Instance.PlayerUnits
            .Concat(GameManager.Instance.AllyUnits)
            .Where(u => u.IsAlive && ManhattanDist(from.CurrentTile, u.CurrentTile) <= from.DetectRange)
            .OrderBy(u => ManhattanDist(from.CurrentTile, u.CurrentTile))
            .FirstOrDefault();

    private bool InAttackRange(Unit attacker, Unit target) =>
        ManhattanDist(attacker.CurrentTile, target.CurrentTile) <= attacker.data.attackRange;

    private int ManhattanDist(TileNode a, TileNode b) =>
        Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);

    private TileNode FindEmptyAdjacent(TileNode center)
    {
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };
        for (int i = 0; i < 4; i++)
        {
            var t = GridManager.Instance.GetTile(center.X + dx[i], center.Y + dy[i]);
            if (t != null && t.IsWalkable && !t.HasUnit) return t;
        }
        return null;
    }
}
