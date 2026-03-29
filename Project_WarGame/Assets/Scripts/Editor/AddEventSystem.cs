using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class AddEventSystem
{
    public static void Execute()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            Debug.Log("[WarGame] EventSystem already exists.");
            return;
        }

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();

        Undo.RegisterCreatedObjectUndo(go, "Add EventSystem");
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[WarGame] EventSystem added to scene.");
    }
}
