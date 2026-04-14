using UnityEngine;
using UnityEditor;

public class ConnectStageData
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

        var mapData = AssetDatabase.LoadAssetAtPath<MapData>("Assets/GameData/Maps/Stage01.asset");
        if (mapData == null) { Debug.LogError("Stage01.asset not found"); return; }

        ui.stageDatas = new MapData[] { mapData };

        EditorUtility.SetDirty(canvas);
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("stageDatas[0] 연결 완료: " + mapData.name);
    }
}
