using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class InspectWaterFoam
{
    public static void Execute()
    {
        // WaterFoam 에셋 타입 확인
        var foamAsset = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/GameData/WaterFoam.asset");
        if (foamAsset == null) { Debug.LogError("WaterFoam.asset not found"); return; }

        Debug.Log($"[WaterFoam] Type: {foamAsset.GetType().FullName}");

        if (foamAsset is AnimatedTile animTile)
        {
            Debug.Log($"[WaterFoam] AnimatedTile — frames: {animTile.m_AnimatedSprites?.Length}, speed: {animTile.m_MinSpeed}~{animTile.m_MaxSpeed}");
        }
        else if (foamAsset is Tile simpleTile)
        {
            Debug.Log($"[WaterFoam] 단순 Tile (애니메이션 없음) — sprite: {simpleTile.sprite?.name}");
        }

        // Water Foam 스프라이트 시트 확인
        var allSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Tiny Swords/Terrain/Tileset/Water Foam.png");
        int spriteCount = 0;
        foreach (var obj in allSprites)
            if (obj is Sprite s) { Debug.Log($"  Sprite: {s.name}"); spriteCount++; }
        Debug.Log($"[WaterFoam] Water Foam.png 총 스프라이트 수: {spriteCount}");
    }
}
