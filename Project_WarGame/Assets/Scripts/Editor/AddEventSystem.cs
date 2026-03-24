using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

public class AddEventSystem
{
    public static void Execute()
    {
        var existing = Object.FindFirstObjectByType<EventSystem>();
        if (existing != null)
        {
            Debug.Log("[WarGame] EventSystem already exists.");
            return;
        }

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[WarGame] EventSystem added.");
    }
}
