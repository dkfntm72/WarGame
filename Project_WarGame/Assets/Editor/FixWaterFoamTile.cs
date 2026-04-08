using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class FixWaterFoamTile
{
    public static void Execute()
    {
        const string foamAssetPath = "Assets/GameData/WaterFoam.asset";

        // Water Foam 스프라이트 16장 수집 (0~15 순서)
        var allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Tiny Swords/Terrain/Tileset/Water Foam.png");
        var sprites = new Sprite[16];
        foreach (var obj in allAssets)
        {
            if (obj is Sprite s && s.name.StartsWith("Water Foam_"))
            {
                if (int.TryParse(s.name.Replace("Water Foam_", ""), out int idx) && idx < 16)
                    sprites[idx] = s;
            }
        }

        for (int i = 0; i < 16; i++)
        {
            if (sprites[i] == null) { Debug.LogError($"Water Foam_{i} not found"); return; }
        }

        // 기존 WaterFoam.asset 삭제 후 AnimatedTile로 재생성
        AssetDatabase.DeleteAsset(foamAssetPath);

        var animTile = ScriptableObject.CreateInstance<AnimatedTile>();
        animTile.m_AnimatedSprites = sprites;
        animTile.m_MinSpeed = 1f;
        animTile.m_MaxSpeed = 1f;
        animTile.m_AnimationStartTime = 0f;

        AssetDatabase.CreateAsset(animTile, foamAssetPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[WaterFoam] AnimatedTile 재생성 완료 — {sprites.Length}프레임, 속도: {animTile.m_MinSpeed}");
    }
}
