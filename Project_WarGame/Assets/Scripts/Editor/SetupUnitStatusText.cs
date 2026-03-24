using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SetupUnitStatusText
{
    public static void Execute()
    {
        var uiRoots = SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject uiRoot = System.Array.Find(uiRoots, g => g.name == "UI");
        if (uiRoot == null) { Debug.LogError("UI root not found"); return; }

        var panelT = uiRoot.transform.Find("UnitInfoPanel");
        if (panelT == null) { Debug.LogError("UnitInfoPanel not found"); return; }

        var statusT = panelT.Find("UnitStatus");
        if (statusT == null) { Debug.LogError("UnitStatus not found"); return; }

        var tmp = statusT.GetComponent<TextMeshProUGUI>();
        if (tmp == null) { Debug.LogError("TMP not found"); return; }

        // Copy font from UnitName sibling
        var nameTmp = panelT.Find("UnitName")?.GetComponent<TextMeshProUGUI>();
        if (nameTmp != null)
            tmp.font = nameTmp.font;

        EditorUtility.SetDirty(statusT.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(statusT.gameObject.scene);
        Debug.Log("Font assigned to UnitStatus");
    }
}
