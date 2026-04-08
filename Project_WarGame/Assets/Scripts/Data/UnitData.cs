using UnityEngine;

[CreateAssetMenu(fileName = "New UnitData", menuName = "WarGame/Unit Data")]
public class UnitData : ScriptableObject
{
    public UnitType unitType;
    public string unitName;
    public bool isElite;   // 정예유닛 여부 (생산 불가, 플레이어 전용)
    public int maxHp = 100;
    public int attack = 10;
    public int defense = 0;
    public int moveRange = 3;
    public int attackRange = 1;
    public int detectRange = 6;
    public int goldCost = 50;

    [Header("Player Faction")]
    public RuntimeAnimatorController playerAnimController;
    public Sprite playerIdleSprite;

    [Header("Enemy Faction")]
    public RuntimeAnimatorController enemyAnimController;
    public Sprite enemyIdleSprite;

    [Header("Ally Faction")]
    public RuntimeAnimatorController allyAnimController;
    public Sprite allyIdleSprite;

    public RuntimeAnimatorController GetAnimController(Faction faction) =>
        faction == Faction.Player ? playerAnimController :
        faction == Faction.Ally   ? allyAnimController  :
        enemyAnimController;

    public Sprite GetIdleSprite(Faction faction) =>
        faction == Faction.Player ? playerIdleSprite :
        faction == Faction.Ally   ? allyIdleSprite   :
        enemyIdleSprite;
}
