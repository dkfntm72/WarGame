using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class CreateDamageNumberPrefab
{
    public static void Execute()
    {
        // Ensure Resources folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        var go  = new GameObject("DamageNumber");
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.fontSize     = 4f;
        tmp.fontStyle    = FontStyles.Bold;
        tmp.alignment    = TextAlignmentOptions.Center;
        tmp.sortingOrder = 10;
        tmp.text         = "0";

        go.AddComponent<DamageNumber>();

        string path = "Assets/Resources/DamageNumber.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        // Remove old prefab if it exists
        if (File.Exists("Assets/Prefabs/DamageNumber.prefab"))
            AssetDatabase.DeleteAsset("Assets/Prefabs/DamageNumber.prefab");

        AssetDatabase.Refresh();
        Debug.Log("DamageNumber prefab created at " + path);
    }
}
