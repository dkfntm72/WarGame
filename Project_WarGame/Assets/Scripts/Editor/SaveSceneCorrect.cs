using UnityEditor;
using UnityEditor.SceneManagement;

public class SaveSceneCorrect
{
    public static void Execute()
    {
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
            "Assets/Scenes/Stage01.unity");
        UnityEngine.Debug.Log("[WarGame] Scene saved to Assets/Scenes/Stage01.unity");
    }
}
