using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    // ── Runtime state ──────────────────────────────────────────
    public UnitData data     { get; private set; }
    public Faction  faction  { get; private set; }

    public int  Rank        { get; private set; }
    public int  CurrentHp   { get; private set; }
    public int  DetectRange { get; private set; } // 0이면 data.detectRange 사용

    private int _exp;
    private const int ExpPerRank = 50;
    public bool HasMoved   { get; private set; }
    public bool HasActed   { get; private set; }

    // 등급 보정 스탯
    public int EffectiveAttack => data.attack + Rank * 2;
    public int EffectiveMaxHp  => data.maxHp  + Rank * 10;

    public TileNode CurrentTile { get; private set; }

    public bool IsAlive     => CurrentHp > 0;
    public bool CanMove     => !HasMoved && IsAlive && data.moveRange > 0;
    public bool CanAct      => !HasActed && IsAlive;
    public bool IsExhausted => HasMoved && HasActed;

    // ── Events ─────────────────────────────────────────────────
    public event Action<Unit> OnDied;
    public event Action       OnStateChanged;

    // ── Components ─────────────────────────────────────────────
    private SpriteRenderer sr;
    private Animator       anim;
    private HealthBar      healthBar;
    private RankBar        rankBar;
    private Animator       _overlayAnim; // 타워 위 궁수 오버레이

    private static GameObject damageNumberPrefab;

    // ── Init ───────────────────────────────────────────────────
    public void Initialize(UnitData unitData, Faction unitFaction, TileNode tile)
    {
        data    = unitData;
        faction = unitFaction;

        CurrentHp = EffectiveMaxHp;

        sr        = GetComponent<SpriteRenderer>();
        anim      = GetComponent<Animator>();
        healthBar = GetComponent<HealthBar>();
        rankBar   = GetComponent<RankBar>();

        var controller = data.GetAnimController(faction);
        if (anim != null && controller != null)
            anim.runtimeAnimatorController = controller;
        else
        {
            if (anim != null) { anim.enabled = false; anim = null; } // 컨트롤러 없는 유닛(Tower 등) — Animator 경고 방지
            if (sr != null) sr.sprite = data.GetIdleSprite(faction);
        }

        if (data.unitType == UnitType.Tower)
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            // 체력바는 부모 스케일 상속을 상쇄해 다른 유닛과 동일한 크기 유지
            var hbRoot = transform.Find("HealthBarRoot");
            if (hbRoot != null) hbRoot.localScale = new Vector3(2f, 2f, 1f);

            // 타워 위 궁수 비주얼 오버레이 생성
            var archerData = GameManager.Instance?.settings?.GetUnitData(UnitType.Archer);
            if (archerData != null)
            {
                var overlayGO = new GameObject("ArcherOverlay");
                overlayGO.transform.SetParent(transform);
                overlayGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                overlayGO.transform.localScale    = Vector3.one;

                var overlaySr = overlayGO.AddComponent<SpriteRenderer>();
                overlaySr.sortingLayerName = sr != null ? sr.sortingLayerName : "Default";
                overlaySr.sortingOrder     = sr != null ? sr.sortingOrder + 1 : 1;

                var archerController = archerData.GetAnimController(faction);
                if (archerController != null)
                {
                    _overlayAnim = overlayGO.AddComponent<Animator>();
                    _overlayAnim.runtimeAnimatorController = archerController;
                }
                else
                {
                    overlaySr.sprite = archerData.GetIdleSprite(faction);
                }
            }
        }

        DetectRange = data.detectRange;
        PlaceOnTile(tile);
        RefreshColor();
    }

    public void SetDetectRange(int range)
    {
        DetectRange = Mathf.Max(1, range);
    }

    // ── Movement ───────────────────────────────────────────────
    public IEnumerator MoveTo(TileNode destination, List<TileNode> path)
    {
        HasMoved = true;
        anim?.SetTrigger("Run");

        if (CurrentTile != null) CurrentTile.OccupyingUnit = null;

        foreach (var node in path)
        {
            Vector3 target  = GridManager.Instance.GetWorldCenter(node.X, node.Y);
            Vector3 origin  = transform.position;
            float   elapsed = 0f;
            const float dur = 0.15f;

            while (elapsed < dur)
            {
                elapsed           += Time.deltaTime;
                transform.position = Vector3.Lerp(origin, target, elapsed / dur);
                yield return null;
            }
            transform.position = target;
        }

        CurrentTile               = destination;
        destination.OccupyingUnit = this;

        anim?.SetTrigger("Idle");
        RefreshColor();
        OnStateChanged?.Invoke();
    }

    // ── Combat ─────────────────────────────────────────────────
    public IEnumerator Attack(Unit target)
    {
        HasActed = true;
        HasMoved = true;
        anim?.SetTrigger("Attack");
        _overlayAnim?.SetTrigger("Attack");
        yield return new WaitForSeconds(0.4f);

        int baseAtk = data.unitType == UnitType.Monk
            ? EffectiveAttack
            : EffectiveAttack + UnityEngine.Random.Range(-5, 6);
        int dmg = Mathf.Max(0, baseAtk - target.data.defense);
        target.TakeDamage(dmg);
        GainExp(UnityEngine.Random.Range(5, 11));
        if (!target.IsAlive) GainExp(20);
        RefreshColor();
        OnStateChanged?.Invoke();

        // ── 반격 ──────────────────────────────────────────────
        if (target.IsAlive && target.CanCounter(this))
            yield return StartCoroutine(target.CounterAttack(this));
    }

    // 반격 가능 여부: Monk 제외, 공격자가 내 사거리 안에 있을 때
    public bool CanCounter(Unit attacker)
    {
        if (data.unitType == UnitType.Monk) return false;
        if (!FactionHelper.IsHostileTo(faction, attacker.faction)) return false;
        if (CurrentTile == null || attacker.CurrentTile == null) return false;
        int dist = Mathf.Abs(CurrentTile.X - attacker.CurrentTile.X)
                 + Mathf.Abs(CurrentTile.Y - attacker.CurrentTile.Y);
        return dist <= data.attackRange;
    }

    // 반격 실행: 공격력의 50%, 방어력 적용
    public IEnumerator CounterAttack(Unit attacker)
    {
        yield return new WaitForSeconds(0.2f);
        anim?.SetTrigger("Attack");
        _overlayAnim?.SetTrigger("Attack");
        yield return new WaitForSeconds(0.3f);

        int counterDmg = Mathf.Max(0,
            Mathf.RoundToInt(EffectiveAttack * 0.5f) - attacker.data.defense);
        attacker.TakeDamage(counterDmg);
        GainExp(3);   // 반격 성공 경험치
    }

    public IEnumerator HealAlly(Unit target)
    {
        HasActed = true;
        HasMoved = true;
        anim?.SetTrigger("Attack");
        yield return new WaitForSeconds(0.4f);
        target.Heal(EffectiveAttack);
        RefreshColor();
        OnStateChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        CurrentHp = Mathf.Max(0, CurrentHp - amount);
        healthBar?.SetHP(CurrentHp, EffectiveMaxHp);
        SpawnDamageNumber(amount, false);
        if (CurrentHp <= 0) { Die(); return; }
        GainExp(UnityEngine.Random.Range(3, 7));        // 피격 경험치 3~6
    }

    public void Heal(int amount)
    {
        CurrentHp = Mathf.Min(EffectiveMaxHp, CurrentHp + amount);
        healthBar?.SetHP(CurrentHp, EffectiveMaxHp);
        SpawnDamageNumber(amount, true);
    }

    // 등급 설정 (0~5). 등급 상승분만큼 현재 HP도 증가.
    public void SetRank(int rank)
    {
        int prevMax = EffectiveMaxHp;
        Rank = Mathf.Clamp(rank, 0, 5);
        int hpGain = EffectiveMaxHp - prevMax;
        CurrentHp = Mathf.Clamp(CurrentHp + hpGain, 0, EffectiveMaxHp);
        healthBar?.SetHP(CurrentHp, EffectiveMaxHp);
        rankBar?.SetRank(Rank);
        OnStateChanged?.Invoke();
    }

    private void GainExp(int amount)
    {
        if (Rank >= 5) return;
        _exp += amount;
        while (_exp >= ExpPerRank && Rank < 5)
        {
            _exp -= ExpPerRank;
            SetRank(Rank + 1);
        }
        if (Rank >= 5) _exp = 0;
    }

    private void SpawnDamageNumber(int amount, bool isHeal)
    {
        if (damageNumberPrefab == null)
            damageNumberPrefab = Resources.Load<GameObject>("DamageNumber");

        if (damageNumberPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        var go = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);
        go.GetComponent<DamageNumber>()?.Setup(amount, isHeal);
    }

    // ── Turn management ────────────────────────────────────────
    public void ResetTurn()
    {
        HasMoved = false;
        HasActed = false;
        RefreshColor();
        OnStateChanged?.Invoke();
    }

    public void SetExhausted()
    {
        HasMoved = true;
        HasActed = true;
        RefreshColor();
        OnStateChanged?.Invoke();
    }

    // ── Internals ──────────────────────────────────────────────
    private void PlaceOnTile(TileNode tile)
    {
        if (CurrentTile != null) CurrentTile.OccupyingUnit = null;
        CurrentTile               = tile;
        tile.OccupyingUnit        = this;
        transform.position        = GridManager.Instance.GetWorldCenter(tile.X, tile.Y);
    }

    private void Die()
    {
        if (CurrentTile != null) CurrentTile.OccupyingUnit = null;
        OnDied?.Invoke(this);

        if (sr != null) sr.enabled = false;
        if (healthBar != null) healthBar.enabled = false;

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        var prefab = GameManager.Instance?.settings?.deathEffectPrefab;
        if (prefab != null)
        {
            var effect = Instantiate(prefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        yield return new WaitForSeconds(0.85f);
        Destroy(gameObject);
    }

    private void RefreshColor()
    {
        if (sr == null) return;
        bool cannotAct = HasActed || (HasMoved && !HasAttackableTarget());
        bool isDark    = faction == Faction.Player && HasMoved && cannotAct;
        sr.color = isDark ? new Color(0.55f, 0.55f, 0.55f) : Color.white;
    }

    private bool HasAttackableTarget()
    {
        if (CurrentTile == null || GridManager.Instance == null) return false;
        for (int dx = -data.attackRange; dx <= data.attackRange; dx++)
        for (int dy = -data.attackRange; dy <= data.attackRange; dy++)
        {
            if (Mathf.Abs(dx) + Mathf.Abs(dy) > data.attackRange) continue;
            if (dx == 0 && dy == 0) continue;
            var node = GridManager.Instance.GetTile(CurrentTile.X + dx, CurrentTile.Y + dy);
            if (node?.OccupyingUnit != null && FactionHelper.IsHostileTo(faction, node.OccupyingUnit.faction))
                return true;
        }
        return false;
    }
}
