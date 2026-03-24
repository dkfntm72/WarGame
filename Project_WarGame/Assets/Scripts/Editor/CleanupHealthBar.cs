using UnityEngine;
using UnityEditor;

public class CleanupHealthBar
{
    public static void Execute()
    {
        const string prefabPath = "Assets/Prefabs/Unit.prefab";

        using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
        var root = scope.prefabContentsRoot;

        // Remove ALL existing HealthBar components
        var hbs = root.GetComponents<HealthBar>();
        Debug.Log($"Found {hbs.Length} HealthBar components — removing all");
        foreach (var h in hbs)
            Object.DestroyImmediate(h);

        // Remove HealthBarRoot child
        var existing = root.transform.Find("HealthBarRoot");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
            Debug.Log("Removed HealthBarRoot");
        }

        Debug.Log("Cleanup done");
    }
}
