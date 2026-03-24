using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData data    { get; private set; }
    public Faction      faction { get; private set; }
    public TileNode     tile    { get; private set; }

    private SpriteRenderer sr;

    public void Initialize(BuildingData buildingData, Faction buildingFaction, TileNode tileNode)
    {
        data    = buildingData;
        faction = buildingFaction;
        tile    = tileNode;

        tileNode.Building  = this;
        Vector3 pos = GridManager.Instance.GetWorldCenter(tileNode.X, tileNode.Y);
        if (buildingData.buildingType == BuildingType.Cathedral) pos.y += 0.6f;
        transform.position = pos;

        sr = GetComponent<SpriteRenderer>();
        RefreshSprite();

        transform.localScale = data.buildingType switch
        {
            BuildingType.Castle       => new Vector3(0.6f, 0.6f, 1f),
            BuildingType.House        => new Vector3(0.5f, 0.5f, 1f),
            BuildingType.ArcheryRange => new Vector3(0.5f, 0.5f, 1f),
            BuildingType.Cathedral    => new Vector3(0.5f, 0.5f, 1f),
            _                         => Vector3.one
        };
    }

    public void Capture(Faction newFaction)
    {
        faction = newFaction;
        RefreshSprite();
    }

    public int GetGoldProduction() => data.goldProduction;

    private void RefreshSprite()
    {
        if (sr != null)
            sr.sprite = data.GetSprite(faction);
    }

}
