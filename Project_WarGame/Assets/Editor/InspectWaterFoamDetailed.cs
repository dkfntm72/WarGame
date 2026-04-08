using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class InspectWaterFoamDetailed
{
    public static void Execute()
    {
        var foamAsset = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/GameData/WaterFoam.asset");
        Debug.Log($"[Detail] WaterFoam Type: {foamAsset?.GetType().FullName ?? "NULL"}");

        if (foamAsset is AnimatedTile anim)
        {
            Debug.Log($"[Detail] frames:{anim.m_AnimatedSprites?.Length}, speed:{anim.m_MinSpeed}");
            if (anim.m_AnimatedSprites != null)
                for (int i = 0; i < anim.m_AnimatedSprites.Length; i++)
                {
                    var s = anim.m_AnimatedSprites[i];
                    Debug.Log($"  [{i}] {s?.name ?? "NULL"} | pivot:{s?.pivot} | rect:{s?.rect}");
                }
        }

        // WaterFoamTilemap 타일맵 컬러 확인
        var grid = GameObject.Find("Grid");
        foreach (Transform child in grid.transform)
        {
            var tm = child.GetComponent<Tilemap>();
            if (tm == null) continue;
            Debug.Log($"[Color] {child.name} | tilemap.color:{tm.color}");
        }
    }
}
