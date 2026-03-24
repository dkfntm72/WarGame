public class TileNode
{
    public int X { get; }
    public int Y { get; }
    public TerrainType TerrainType { get; set; }

    public Unit OccupyingUnit { get; set; }
    public Building Building { get; set; }

    public bool IsWalkable  => TerrainType != TerrainType.Water && TerrainType != TerrainType.Wall;
    public bool HasUnit     => OccupyingUnit != null;
    public bool HasBuilding => Building != null;

    public TileNode(int x, int y, TerrainType terrainType)
    {
        X = x;
        Y = y;
        TerrainType = terrainType;
    }
}
