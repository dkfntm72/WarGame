using UnityEditor;

public class FixGameSettingsTiles
{
    public static void Execute()
    {
        var guids = AssetDatabase.FindAssets("t:GameSettings");
        if (guids.Length == 0) { UnityEngine.Debug.LogError("GameSettings not found"); return; }
        var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));

        var grassRule = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.TileBase>("Assets/GameData/RuleTile_FlatGround.asset");
        var waterBg   = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Background color.asset");

        Undo.RecordObject(settings, "Fix GameSettings Tiles");
        settings.grassTile = grassRule;
        settings.waterTile = waterBg;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        UnityEngine.Debug.Log("[WarGame] GameSettings tiles updated: grassTile=RuleTile_FlatGround, waterTile=Water Background color");
    }
}
