using UnityEditor;

public class RedesignTerrain
{
    public static void Execute()
    {
        var mapData = AssetDatabase.LoadAssetAtPath<MapData>("Assets/GameData/Maps/Stage01.asset");
        if (mapData == null) { UnityEngine.Debug.LogError("Stage01 not found"); return; }

        // 10x8 map — terrain[y * width + x]
        // G=Grass(0), T=Tree(3), W=Water(4)
        // Buildings/units positions must stay Grass
        int[,] grid = new int[8, 10]
        {
            // x: 0  1  2  3  4  5  6  7  8  9
            {   0, 0, 3, 3, 0, 0, 3, 3, 0, 0 }, // y=0
            {   0, 3, 0, 0, 0, 0, 0, 0, 3, 0 }, // y=1
            {   0, 3, 3, 0, 0, 0, 0, 3, 3, 0 }, // y=2
            {   0, 0, 0, 3, 0, 0, 3, 0, 0, 0 }, // y=3
            {   0, 0, 0, 0, 4, 4, 0, 0, 0, 0 }, // y=4  (water)
            {   0, 0, 0, 0, 4, 4, 0, 0, 0, 0 }, // y=5  (water)
            {   0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // y=6
            {   0, 3, 3, 0, 0, 0, 0, 3, 3, 0 }, // y=7
        };

        mapData.InitializeTerrain();
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 10; x++)
                mapData.SetTerrain(x, y, (TerrainType)grid[y, x]);

        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();
        UnityEngine.Debug.Log("Terrain redesigned");
    }
}
