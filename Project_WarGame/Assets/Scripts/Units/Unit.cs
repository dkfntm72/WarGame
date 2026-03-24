using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    // ── Runtime state ──────────────────────────────────────────
    public UnitData data     { get; private set; }
    public Faction  faction  { get; private set; }

    public int  CurrentHp  { get; private set; }
    public bool HasMoved   { get; private set; }
    public bool HasActed   { get; private set; }

    public TileNode CurrentTile { get; private set; }

    public bool IsAlive     => CurrentHp > 0;
    public bool CanMove     => !HasMoved && IsAlive;
    public bool CanAct      => !HasActed && IsAlive;
    public bool IsExhausted => HasMoved && HasActed;

    // ── Events ─────────────────────────────────────────────────
    public event Action<Unit> OnDied;
    public event Action       OnStateChanged;

    // ── Components ─────────────────────────────────────────────
    private SpriteRenderer sr;
    private Animator       anim;
    private HealthBar      healthBar;

    private static GameObject damageNumberPrefab;

    // ── Init ───────────────────────────────────────────────────
    public void Initialize(UnitData unitData, Faction unitFaction, TileNode tile)
    {
        data    = unitData;
        faction = unitFaction;

        CurrentHp = data.maxHp;

        sr        = GetComponent<SpriteRenderer>();
        anim      = GetComponent<Animator>();
        healthBar = GetComponent<HealthBar>();

        var controller = data.GetAnimController(faction);
        if (anim != null && controller != null)
            anim.runtimeAnimatorController = controller;
        else if (sr != null)
            sr.sprite = data.GetIdleSprite(faction);

        PlaceOnTile(tile);
        RefreshColor();
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
        OnStateChanged?.Invoke();
    }

    // ── Combat ─────────────────────────────────────────────────
    public IEnumerator Attack(Unit target)
    {
        HasActed = true;
        HasMoved = true;
        anim?.SetTrigger("Attack");
        yield return new WaitForSeconds(0.4f);
        int dmg = Mathf.Max(0, data.attack - target.data.defense);
        target.TakeDamage(dmg);
        RefreshColor();
        OnStateChanged?.Invoke();
    }

    public IEnumerator HealAlly(Unit target)
    {
        HasActed = true;
        HasMoved = true;
        anim?.SetTrigger("Attack");
        yield return new WaitForSeconds(0.4f);
        target.Heal(data.attack);
        RefreshColor();
        OnStateChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        CurrentHp = Mathf.Max(0, CurrentHp - amount);
        healthBar?.SetHP(CurrentHp, data.maxHp);
        SpawnDamageNumber(amount, false);
        if (CurrentHp <= 0) Die();
    }

    public void Heal(int amount)
    {
        CurrentHp = Mathf.Min(data.maxHp, CurrentHp + amount);
        healthBar?.SetHP(CurrentHp, data.maxHp);
        SpawnDamageNumber(amount, true);
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
        Destroy(gameObject);
    }

    private void RefreshColor()
    {
        if (sr == null) return;
        bool isDark = faction == Faction.Player && HasMoved;
        sr.color = isDark ? new Color(0.55f, 0.55f, 0.55f) : Color.white;
    }
}
