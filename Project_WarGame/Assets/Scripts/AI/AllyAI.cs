using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 동맹군 AI. 플레이어 통제 불가, 자동으로 적 유닛을 공격한다.
/// TurnManager.StartAllyTurn() 에서 호출됨.
/// </summary>
public class AllyAI : MonoBehaviour
{
    public static AllyAI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public IEnumerator ExecuteTurn()
    {
        yield return new WaitForSeconds(0.4f);

        var units = new List<Unit>(GameManager.Instance.AllyUnits);
        foreach (var unit in units)
        {
            if (!unit.IsAlive) continue;
            yield return StartCoroutine(ActUnit(unit));
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.3f);
        TurnManager.Instance.EndAllyTurn();
    }

    private IEnumerator ActUnit(Unit unit)
    {
        var target = FindNearestEnemy(unit);
        if (target == null) { unit.SetExhausted(); yield break; }

        // 사거리 내 즉시 공격
        if (unit.CanAct && InAttackRange(unit, target))
        {
            yield return StartCoroutine(PerformAttack(unit, target));
            yield break;
        }

        // 이동
        if (unit.CanMove)
        {
            var moveTiles = GridManager.Instance.GetMovementRange(
                unit.CurrentTile, unit.data.moveRange, Faction.Ally);

            var bestTile = moveTiles
                .Where(t => !t.HasUnit)
                .OrderBy(t => ManhattanDist(t, target.CurrentTile))
                .FirstOrDefault();

            if (bestTile != null)
            {
                var path = Pathfinding.FindPath(GridManager.Instance, unit.CurrentTile, bestTile, Faction.Ally);
                if (path.Count > 0)
                    yield return StartCoroutine(unit.MoveTo(bestTile, path));
            }
        }

        // 이동 후 공격
        if (unit.CanAct)
        {
            var newTarget = FindNearestEnemy(unit);
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
            // 동맹군 Monk: 체력 낮은 아군(Player/Ally) 치료
            var healTarget = GameManager.Instance.PlayerUnits
                .Concat(GameManager.Instance.AllyUnits)
                .Where(u => u != attacker && u.IsAlive
                    && ManhattanDist(attacker.CurrentTile, u.CurrentTile) <= attacker.data.attackRange)
                .OrderBy(u => u.CurrentHp)
                .FirstOrDefault();
            if (healTarget != null) { yield return StartCoroutine(attacker.HealAlly(healTarget)); yield break; }
        }
        yield return StartCoroutine(attacker.Attack(target));
    }

    private Unit FindNearestEnemy(Unit from) =>
        GameManager.Instance.EnemyUnits
            .Where(u => u.IsAlive && ManhattanDist(from.CurrentTile, u.CurrentTile) <= from.DetectRange)
            .OrderBy(u => ManhattanDist(from.CurrentTile, u.CurrentTile))
            .FirstOrDefault();

    private bool InAttackRange(Unit attacker, Unit target) =>
        ManhattanDist(attacker.CurrentTile, target.CurrentTile) <= attacker.data.attackRange;

    private int ManhattanDist(TileNode a, TileNode b) =>
        Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
}
