using UnityEngine;
using UnityEditor;

public class SetSelectionCorners
{
    public static void Execute()
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject canvas = null;
        foreach (var r in roots)
            if (r.name == "Canvas") { canvas = r; break; }
        if (canvas == null) { Debug.LogError("Canvas not found"); return; }

        var ui = canvas.GetComponent<StageSelectUI>();
        if (ui == null) { Debug.LogError("StageSelectUI not found"); return; }

        string basePath = "Assets/Tiny Swords/UI Elements/Cursors/Cursor_04.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(basePath);

        ui.selectionCorners = new Sprite[4];
        foreach (var asset in sprites)
        {
            if (asset is Sprite sp)
            {
                switch (sp.name)
                {
                    case "Cursor_04_0": ui.selectionCorners[0] = sp; break;
                    case "Cursor_04_1": ui.selectionCorners[1] = sp; break;
                    case "Cursor_04_2": ui.selectionCorners[2] = sp; break;
                    case "Cursor_04_3": ui.selectionCorners[3] = sp; break;
                }
            }
        }

        EditorUtility.SetDirty(canvas);
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log($"selectionCorners 연결 완료: {ui.selectionCorners[0]?.name}, {ui.selectionCorners[1]?.name}, {ui.selectionCorners[2]?.name}, {ui.selectionCorners[3]?.name}");
    }
}
