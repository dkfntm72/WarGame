using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance { get; private set; }

    private Unit           selectedUnit;
    private List<TileNode> moveTiles   = new();
    private List<TileNode> attackTiles = new();

    // Only two states: nothing selected, or a unit is selected
    private bool unitSelected => selectedUnit != null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (TurnManager.Instance == null || TurnManager.Instance.CurrentState != GameState.PlayerTurn) return;

        // 드래그가 아닌 짧은 탭(릴리즈 시점)일 때만 타일 선택 처리
        if (GameUI.Instance != null && GameUI.Instance.IsEventShowing)  return;
        if (GameUI.Instance != null && GameUI.Instance.IsSettingsOpen)  return;

        if (Input.GetMouseButtonUp(0) && !CameraDragController.IsDragging)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0;
            HandleTap(world);
        }
    }

    private void HandleTap(Vector3 worldPos)
    {
        var tile = GridManager.Instance.GetTileAtWorld(worldPos);
        if (tile == null) return;

        if (!unitSelected)
            OnIdle(tile);
        else
            OnUnitSelected(tile);
    }

    // ── No unit selected ──────────────────────────────────────
    private void OnIdle(TileNode tile)
    {
        if (tile.HasUnit)
        {
            var unit = tile.OccupyingUnit;
            if (unit.faction == Faction.Player && !unit.IsExhausted)
                SelectUnit(unit);
            else
                GameUI.Instance?.ShowUnitInfo(unit);
            return;
        }

        if (tile.HasBuilding && tile.Building.faction == Faction.Player)
            GameUI.Instance?.ShowBuildingPanel(tile.Building);
    }

    // ── Unit selected ─────────────────────────────────────────
    private void OnUnitSelected(TileNode tile)
    {
        // Tap same unit → deselect
        if (tile.HasUnit && tile.OccupyingUnit == selectedUnit)
        {
            Deselect();
            return;
        }

        // Tap enemy in attack range → attack
        if (selectedUnit.CanAct && tile.HasUnit && tile.OccupyingUnit.faction == Faction.Enemy
            && attackTiles.Contains(tile))
        {
            if (selectedUnit.data.unitType != UnitType.Monk)
                StartCoroutine(AttackAndFollowUp(selectedUnit, tile.OccupyingUnit));
            else
                PostAction();
            return;
        }

        // Monk: tap friendly in heal range → heal
        if (selectedUnit.CanAct && selectedUnit.data.unitType == UnitType.Monk
            && tile.HasUnit && tile.OccupyingUnit.faction == Faction.Player
            && tile.OccupyingUnit != selectedUnit && attackTiles.Contains(tile))
        {
            StartCoroutine(HealAndFollowUp(selectedUnit, tile.OccupyingUnit));
            return;
        }

        // Tap move tile (empty) → move
        if (selectedUnit.CanMove && moveTiles.Contains(tile) && !tile.HasUnit)
        {
            var path = Pathfinding.FindPath(GridManager.Instance, selectedUnit.CurrentTile, tile, Faction.Player);
            StartCoroutine(MoveAndFollowUp(selectedUnit, tile, path));
            return;
        }

        // Tap another own unit → switch selection
        if (tile.HasUnit && tile.OccupyingUnit.faction == Faction.Player && !tile.OccupyingUnit.IsExhausted)
        {
            SelectUnit(tile.OccupyingUnit);
            return;
        }

        // Tap any other unit (enemy out of range, exhausted ally) → show info only
        if (tile.HasUnit)
        {
            Deselect();
            GameUI.Instance?.ShowUnitInfo(tile.OccupyingUnit);
            return;
        }

        Deselect();
    }

    // ── Move then allow attack ────────────────────────────────
    private IEnumerator MoveAndFollowUp(Unit unit, TileNode dest, List<TileNode> path)
    {
        GridManager.Instance.ClearHighlights();
        yield return StartCoroutine(unit.MoveTo(dest, path));

        EventTriggerManager.Instance?.OnUnitEnteredTile(unit, dest);

        // Capture buildings on landing
        if (dest.HasBuilding && dest.Building.faction != Faction.Player)
            GameManager.Instance.CaptureBuilding(dest.Building, Faction.Player);

        GameUI.Instance?.ShowUnitInfo(unit);

        if (unit.CanAct)
        {
            // Stay selected — refresh attack highlights from new position
            RefreshHighlights();
        }
        else
        {
            PostAction();
        }
    }

    // ── Select a unit ─────────────────────────────────────────
    private void SelectUnit(Unit unit)
    {
        selectedUnit = unit;
        GameUI.Instance?.ShowUnitInfo(unit);
        RefreshHighlights();
    }

    // 이동 가능 빈 칸(파란색) + 공격 가능 적/힐 대상 칸(빨간색) 동시 표시
    private void RefreshHighlights()
    {
        if (selectedUnit == null) return;

        moveTiles.Clear();
        attackTiles.Clear();

        var settings = GameManager.Instance.settings;

        if (selectedUnit.CanMove)
        {
            var reachable = GridManager.Instance.GetMovementRange(
                selectedUnit.CurrentTile, selectedUnit.data.moveRange, Faction.Player);
            // 빈 칸만 이동 대상으로 표시
            moveTiles = reachable.FindAll(t => !t.HasUnit);
        }

        if (selectedUnit.CanAct)
        {
            var inRange = GridManager.Instance.GetAttackRange(
                selectedUnit.CurrentTile, selectedUnit.data.attackRange);

            if (selectedUnit.data.unitType == UnitType.Monk)
                // 몽크: 범위 내 아군 칸 표시
                attackTiles = inRange.FindAll(t => t.HasUnit
                    && t.OccupyingUnit.faction == Faction.Player
                    && t.OccupyingUnit != selectedUnit);
            else
                // 일반 유닛: 범위 내 적 칸만 표시
                attackTiles = inRange.FindAll(t => t.HasUnit
                    && t.OccupyingUnit.faction == Faction.Enemy);
        }

        GridManager.Instance.ShowBothHighlights(moveTiles, attackTiles, settings);
    }

    // ── Attack / Heal coroutines ──────────────────────────────
    private IEnumerator AttackAndFollowUp(Unit attacker, Unit target)
    {
        GridManager.Instance.ClearHighlights();
        yield return StartCoroutine(attacker.Attack(target));
        PostAction();
    }

    private IEnumerator HealAndFollowUp(Unit healer, Unit target)
    {
        GridManager.Instance.ClearHighlights();
        yield return StartCoroutine(healer.HealAlly(target));
        PostAction();
    }

    // ── Cleanup ───────────────────────────────────────────────
    private void PostAction()
    {
        GridManager.Instance.ClearHighlights();
        GameUI.Instance?.HideUnitInfo();
        selectedUnit = null;
        moveTiles.Clear();
        attackTiles.Clear();
    }

    private void Deselect()
    {
        selectedUnit = null;
        moveTiles.Clear();
        attackTiles.Clear();
        GridManager.Instance.ClearHighlights();
        GameUI.Instance?.HideUnitInfo();
    }

    // ── UI Callbacks ──────────────────────────────────────────
    public void OnEndTurnPressed()
    {
        Deselect();
        TurnManager.Instance.EndPlayerTurn();
    }

    public void OnProduceUnit(UnitType type)
    {
        if (TurnManager.Instance.CurrentState != GameState.PlayerTurn) return;

        var castle = GameManager.Instance.Buildings.Find(
            b => b.faction == Faction.Player && b.data.buildingType == BuildingType.Castle);
        if (castle == null) return;

        var uData = GameManager.Instance.settings.GetUnitData(type);
        if (!ResourceManager.Instance.SpendGold(Faction.Player, uData.goldCost)) return;

        var spawnTile = FindEmptyAdjacent(castle.tile);
        if (spawnTile == null)
        {
            ResourceManager.Instance.AddGold(Faction.Player, uData.goldCost);
            return;
        }

        GameManager.Instance.SpawnUnit(uData, Faction.Player, spawnTile);
        GameUI.Instance?.HideUnitInfo();
    }

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
