using UnityEngine;

[CreateAssetMenu(fileName = "New BuildingData", menuName = "WarGame/Building Data")]
public class BuildingData : ScriptableObject
{
    public BuildingType buildingType;
    public string buildingName;
    public int goldProduction = 0;
    public UnitType[] producibleUnits;

    [Header("Sprites by Faction")]
    public Sprite neutralSprite;
    public Sprite playerSprite;
    public Sprite enemySprite;

    public Sprite GetSprite(Faction faction) => faction switch
    {
        Faction.Player => playerSprite,
        Faction.Enemy  => enemySprite,
        _              => neutralSprite
    };
}
