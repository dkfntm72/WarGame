using UnityEngine;
using UnityEditor;
using System.Linq;

public class CheckSprites
{
    public static void Execute()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Tiny Swords/UI Elements/Bars/SmallBar_Base.png")
            .OfType<Sprite>().ToArray();

        foreach (var s in sprites)
            Debug.Log($"[Sprite] name={s.name}  rect={s.rect}  border={s.border}  pivot={s.pivot}");

        Debug.Log($"Total sprites: {sprites.Length}");
    }
}
