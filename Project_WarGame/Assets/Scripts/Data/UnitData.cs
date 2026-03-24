using UnityEngine;

[CreateAssetMenu(fileName = "New UnitData", menuName = "WarGame/Unit Data")]
public class UnitData : ScriptableObject
{
    public UnitType unitType;
    public string unitName;
    public int maxHp = 100;
    public int attack = 10;
    public int defense = 0;
    public int moveRange = 3;
    public int attackRange = 1;
    public int goldCost = 50;

    [Header("Player Faction")]
    public RuntimeAnimatorController playerAnimController;
    public Sprite playerIdleSprite;

    [Header("Enemy Faction")]
    public RuntimeAnimatorController enemyAnimController;
    public Sprite enemyIdleSprite;

    public RuntimeAnimatorController GetAnimController(Faction faction) =>
        faction == Faction.Player ? playerAnimController : enemyAnimController;

    public Sprite GetIdleSprite(Faction faction) =>
        faction == Faction.Player ? playerIdleSprite : enemyIdleSprite;
}
