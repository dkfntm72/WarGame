using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileTerrainMapping", menuName = "WarGame/Tile Terrain Mapping")]
public class TileTerrainMapping : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public TileBase tile;
        public TerrainType terrainType;
    }

    public Entry[] entries;

    public bool TryGetTerrain(TileBase tile, out TerrainType result)
    {
        if (tile != null && entries != null)
        {
            foreach (var e in entries)
            {
                if (e.tile == tile)
                {
                    result = e.terrainType;
                    return true;
                }
            }
        }
        result = TerrainType.Grass;
        return false;
    }
}
