using UnityEngine;
using UnityEditor;

public class CreateTileTerrainMapping
{
    public static void Execute()
    {
        var asset = ScriptableObject.CreateInstance<TileTerrainMapping>();
        asset.entries = new TileTerrainMapping.Entry[0];

        AssetDatabase.CreateAsset(asset, "Assets/GameData/TileTerrainMapping.asset");
        AssetDatabase.SaveAssets();

        Selection.activeObject = asset;
        EditorUtility.FocusProjectWindow();

        Debug.Log("[WarGame] TileTerrainMapping.asset created at Assets/GameData/");
    }
}
