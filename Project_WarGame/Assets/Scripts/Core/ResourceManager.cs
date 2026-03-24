using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public int PlayerGold { get; private set; }
    public int EnemyGold  { get; private set; }

    public event Action OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(int playerStart, int enemyStart)
    {
        PlayerGold = playerStart;
        EnemyGold  = enemyStart;
        OnGoldChanged?.Invoke();
    }

    public void AddGold(Faction faction, int amount)
    {
        if (faction == Faction.Player) PlayerGold += amount;
        else if (faction == Faction.Enemy) EnemyGold += amount;
        OnGoldChanged?.Invoke();
    }

    public bool SpendGold(Faction faction, int amount)
    {
        if (faction == Faction.Player)
        {
            if (PlayerGold < amount) return false;
            PlayerGold -= amount;
        }
        else if (faction == Faction.Enemy)
        {
            if (EnemyGold < amount) return false;
            EnemyGold -= amount;
        }
        OnGoldChanged?.Invoke();
        return true;
    }

    public int GetGold(Faction faction) => faction == Faction.Player ? PlayerGold : EnemyGold;
}
