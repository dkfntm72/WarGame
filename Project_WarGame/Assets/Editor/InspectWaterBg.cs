using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class InspectWaterBg
{
    public static void Execute()
    {
        // Water Background color 에셋 확인
        var bgAsset = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Background color.asset");
        if (bgAsset == null) { Debug.LogError("Water Background color.asset not found"); return; }

        Debug.Log($"[WaterBg] Type: {bgAsset.GetType().FullName}");

        if (bgAsset is AnimatedTile animTile)
        {
            Debug.Log($"[WaterBg] AnimatedTile — frames: {animTile.m_AnimatedSprites?.Length}, speed: {animTile.m_MinSpeed}~{animTile.m_MaxSpeed}");
            if (animTile.m_AnimatedSprites != null)
                foreach (var s in animTile.m_AnimatedSprites)
                    Debug.Log($"  Frame: {s?.name}");
        }
        else if (bgAsset is Tile simpleTile)
        {
            Debug.Log($"[WaterBg] 단순 Tile — sprite: {simpleTile.sprite?.name}, color: {simpleTile.color}");
        }

        // Water Background 관련 스프라이트 검색
        string[] guids = AssetDatabase.FindAssets("Water Background", new[] { "Assets/Tiny Swords" });
        Debug.Log($"[WaterBg] 관련 에셋 {guids.Length}개:");
        foreach (var guid in guids)
            Debug.Log($"  {AssetDatabase.GUIDToAssetPath(guid)}");
    }
}
