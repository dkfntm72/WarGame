using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Window > WarGame > Create Settings Panel Prefab
/// Assets/Prefabs/SettingsPanel.prefab 을 생성합니다.
/// </summary>
public static class CreateSettingsPanelPrefab
{
    private const string PrefabPath = "Assets/Prefabs/SettingsPanel.prefab";

    [MenuItem("Window/WarGame/Create Settings Panel Prefab")]
    public static void Execute()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/경기천년제목OTF_Bold SDF.asset");

        Material outlineMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Fonts/경기천년제목OTF_Bold SDF - Outline.mat");

        Sprite bgSprite = null;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Tiny Swords/UI Elements/Papers/SpecialPaper.png"))
        {
            if (obj is Sprite sp && sp.name == "SpecialPaper_0") { bgSprite = sp; break; }
        }

        Sprite btnSprite = null;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Tiny Swords/UI Elements/Buttons/SmallRedSquareButton_Regular.png"))
        {
            if (obj is Sprite sp) { btnSprite = sp; break; }
        }

        // ── 루트 패널 ─────────────────────────────────────────
        var root = new GameObject("SettingsPanel");
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin        = new Vector2(0.5f, 0.5f);
        rootRT.anchorMax        = new Vector2(0.5f, 0.5f);
        rootRT.pivot            = new Vector2(0.5f, 0.5f);
        rootRT.anchoredPosition = Vector2.zero;
        rootRT.sizeDelta        = new Vector2(500, 320);

        var rootImg = root.AddComponent<Image>();
        if (bgSprite != null) { rootImg.sprite = bgSprite; rootImg.type = Image.Type.Sliced; }
        else rootImg.color = new Color(0.1f, 0.08f, 0.15f, 0.97f);
        rootImg.color = Color.white;

        // ── 타이틀 ────────────────────────────────────────────
        var title = MakeTMP(root, "SettingsTitle", "설정", fontAsset, 36,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -20f), new Vector2(0f, 50f));
        title.alignment = TextAlignmentOptions.Center;

        // ── 음량 라벨 ─────────────────────────────────────────
        var volLabel = MakeTMP(root, "VolumeLabel", "음량", fontAsset, 26,
            new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.5f),
            new Vector2(0f, 55f), new Vector2(0f, 32f));
        volLabel.alignment = TextAlignmentOptions.Center;

        // ── 볼륨 슬라이더 ─────────────────────────────────────
        var sliderGO = new GameObject("VolumeSlider");
        sliderGO.transform.SetParent(root.transform, false);
        var sliderRT         = sliderGO.AddComponent<RectTransform>();
        sliderRT.anchorMin   = new Vector2(0.1f, 0.5f);
        sliderRT.anchorMax   = new Vector2(0.9f, 0.5f);
        sliderRT.anchoredPosition = new Vector2(0f, 10f);
        sliderRT.sizeDelta   = new Vector2(0f, 30f);

        var slider         = sliderGO.AddComponent<Slider>();
        slider.minValue    = 0f;
        slider.maxValue    = 1f;
        slider.value       = 1f;

        // Background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgRT         = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin   = Vector2.zero;
        bgRT.anchorMax   = Vector2.one;
        bgRT.sizeDelta   = Vector2.zero;
        bgGO.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Fill Area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        var faRT         = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin   = Vector2.zero;
        faRT.anchorMax   = Vector2.one;
        faRT.sizeDelta   = new Vector2(-20f, 0f);
        faRT.anchoredPosition = new Vector2(-5f, 0f);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillArea.transform, false);
        var fillRT       = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.sizeDelta = new Vector2(10f, 0f);
        fillGO.AddComponent<Image>().color = new Color(0.9f, 0.7f, 0.2f, 1f);
        slider.fillRect  = fillRT;

        // Handle Slide Area
        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGO.transform, false);
        var haRT         = handleArea.AddComponent<RectTransform>();
        haRT.anchorMin   = Vector2.zero;
        haRT.anchorMax   = Vector2.one;
        haRT.sizeDelta   = new Vector2(-20f, 0f);

        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(handleArea.transform, false);
        var handleRT     = handleGO.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(30f, 0f);
        var handleImg    = handleGO.AddComponent<Image>();
        handleImg.color  = Color.white;
        slider.handleRect      = handleRT;
        slider.targetGraphic   = handleImg;

        // ── 나가기 버튼 ───────────────────────────────────────
        var exitGO = new GameObject("ExitButton");
        exitGO.transform.SetParent(root.transform, false);
        var exitRT          = exitGO.AddComponent<RectTransform>();
        exitRT.anchorMin    = new Vector2(0.5f, 0f);
        exitRT.anchorMax    = new Vector2(0.5f, 0f);
        exitRT.pivot        = new Vector2(0.5f, 0f);
        exitRT.anchoredPosition = new Vector2(0f, 25f);
        exitRT.sizeDelta    = new Vector2(240f, 58f);

        var exitImg = exitGO.AddComponent<Image>();
        if (btnSprite != null) { exitImg.sprite = btnSprite; exitImg.type = Image.Type.Sliced; exitImg.color = Color.white; }
        else exitImg.color = new Color(0.55f, 0.15f, 0.15f, 1f);
        exitGO.AddComponent<Button>();

        var exitLabel = MakeTMP(exitGO, "Label", "스테이지 선택", fontAsset, 24,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        exitLabel.alignment = TextAlignmentOptions.Center;

        // ── 아웃라인 머티리얼 적용 ────────────────────────────
        if (outlineMat != null)
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                tmp.fontSharedMaterial = outlineMat;

        // ── 프리팹 저장 ───────────────────────────────────────
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (existing != null) AssetDatabase.DeleteAsset(PrefabPath);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[WarGame] SettingsPanel.prefab 생성 완료: " + PrefabPath);
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
    }

    static TextMeshProUGUI MakeTMP(GameObject parent, string name, string text,
        TMP_FontAsset font, int fontSize,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt          = go.AddComponent<RectTransform>();
        rt.anchorMin    = ancMin;
        rt.anchorMax    = ancMax;
        rt.pivot        = new Vector2(0.5f, 1f);
        rt.anchoredPosition = ancPos;
        rt.sizeDelta    = sizeDelta;
        var tmp         = go.AddComponent<TextMeshProUGUI>();
        tmp.text        = text;
        tmp.fontSize    = fontSize;
        tmp.color       = Color.white;
        if (font != null) tmp.font = font;
        return tmp;
    }
}
