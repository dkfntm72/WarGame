using UnityEngine;
using UnityEditor;
using TMPro;

public class WireUnitStatusField
{
    public static void Execute()
    {
        var uiGo = GameObject.Find("UI");
        if (uiGo == null) { Debug.LogError("UI not found"); return; }

        var gameUI = uiGo.GetComponent<GameUI>();
        if (gameUI == null) { Debug.LogError("GameUI not found"); return; }

        var statusGo = GameObject.Find("UI/UnitInfoPanel/UnitStatus");
        if (statusGo == null) { Debug.LogError("UnitStatus not found"); return; }

        var tmp = statusGo.GetComponent<TextMeshProUGUI>();
        if (tmp == null) { Debug.LogError("TMP not found on UnitStatus"); return; }

        var so = new SerializedObject(gameUI);
        so.FindProperty("unitStatusText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(gameUI);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(uiGo.scene);
        Debug.Log("unitStatusText wired successfully");
    }
}
