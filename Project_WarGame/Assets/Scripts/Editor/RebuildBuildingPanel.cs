using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RebuildBuildingPanel
{
    public static void Execute()
    {
        CreateUnitButtonPrefab();
        FixContainer();
    }

    static void FixContainer()
    {
        var uiRoots = SceneManager.GetActiveScene().GetRootGameObjects();
        var uiRoot  = System.Array.Find(uiRoots, g => g.name == "UI");
        if (uiRoot == null) return;

        var panelT = uiRoot.transform.Find("BuildingPanel");
        if (panelT == null) return;

        var titleT = panelT.Find("BuildingTitle");
        if (titleT != null)
        {
            var rt              = titleT.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(1f, 1f);
            rt.pivot            = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -10f);
            rt.sizeDelta        = new Vector2(-20f, 40f);
            var tmp = titleT.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.fontSize = 22f;
        }

        var containerT = panelT.Find("UnitButtonContainer");
        if (containerT != null)
        {
            var rt              = containerT.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 0f);
            rt.anchorMax        = new Vector2(1f, 1f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -22f);
            rt.sizeDelta        = new Vector2(-40f, -65f);

            var hlg                    = containerT.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing                = 24f;
            hlg.childAlignment         = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth      = false;
            hlg.childControlHeight     = false;
        }

        EditorUtility.SetDirty(panelT.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panelT.gameObject.scene);
    }

    static void CreateUnitButtonPrefab()
    {
        var root   = new GameObject("UnitButton");
        root.AddComponent<Image>().color = new Color(0.1f, 0.12f, 0.18f, 0.95f);
        var btn    = root.AddComponent<Button>();
        var rootRT = root.GetComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(180f, 215f);

        // UnitIcon
        var iconGo = new GameObject("UnitIcon");
        iconGo.transform.SetParent(root.transform, false);
        var iconImg            = iconGo.AddComponent<Image>();
        iconImg.preserveAspect = true;
        var iconRT             = iconGo.GetComponent<RectTransform>();
        iconRT.anchorMin       = new Vector2(0.5f, 1f);
        iconRT.anchorMax       = new Vector2(0.5f, 1f);
        iconRT.pivot           = new Vector2(0.5f, 1f);
        iconRT.anchoredPosition = new Vector2(0f, -12f);
        iconRT.sizeDelta       = new Vector2(130f, 130f);

        // UnitName
        var nameGo = new GameObject("UnitName");
        nameGo.transform.SetParent(root.transform, false);
        var nameTmp          = nameGo.AddComponent<TextMeshProUGUI>();
        nameTmp.text         = "Unit";
        nameTmp.fontSize     = 18f;
        nameTmp.color        = Color.white;
        nameTmp.alignment    = TextAlignmentOptions.Center;
        var nameRT           = nameGo.GetComponent<RectTransform>();
        nameRT.anchorMin     = new Vector2(0f, 0f);
        nameRT.anchorMax     = new Vector2(1f, 0f);
        nameRT.pivot         = new Vector2(0.5f, 0f);
        nameRT.anchoredPosition = new Vector2(0f, 42f);
        nameRT.sizeDelta     = new Vector2(0f, 28f);

        // UnitCost
        var costGo = new GameObject("UnitCost");
        costGo.transform.SetParent(root.transform, false);
        var costTmp          = costGo.AddComponent<TextMeshProUGUI>();
        costTmp.text         = "00G";
        costTmp.fontSize     = 16f;
        costTmp.color        = new Color(1f, 0.85f, 0.3f);
        costTmp.alignment    = TextAlignmentOptions.Center;
        var costRT           = costGo.GetComponent<RectTransform>();
        costRT.anchorMin     = new Vector2(0f, 0f);
        costRT.anchorMax     = new Vector2(1f, 0f);
        costRT.pivot         = new Vector2(0.5f, 0f);
        costRT.anchoredPosition = new Vector2(0f, 10f);
        costRT.sizeDelta     = new Vector2(0f, 28f);

        var colors           = btn.colors;
        colors.normalColor   = Color.white;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.6f);
        btn.colors           = colors;
        btn.targetGraphic    = root.GetComponent<Image>();

        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/UnitButton.prefab");
        Object.DestroyImmediate(root);
        Debug.Log("UnitButton prefab resized");
    }
}
