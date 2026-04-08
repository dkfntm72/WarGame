using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BuildMainMenuScene
{
    private const string FontPath = "Assets/Fonts/경기천년제목OTF_Bold SDF.asset";
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";

    public static void Execute()
    {
        // 씬 생성
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 카메라
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
        cam.orthographic = true;
        camGo.tag = "MainCamera";

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        // Canvas
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = canvasGo.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 720);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── 배경 이미지 (전체 화면) ──
        var bg = CreatePanel(canvasGo.transform, "Background",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero,
            new Color(0.10f, 0.10f, 0.12f));

        // ── 로고 영역 (상단 중앙) ──
        var logoPanel = CreatePanel(canvasGo.transform, "LogoPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(560f, 160f),
            new Color(0.18f, 0.18f, 0.20f));
        AddOutline(logoPanel, new Color(0.8f, 0.7f, 0.4f), 2f);

        var logoText = CreateTMP(logoPanel.transform, "LogoText", "WarGame",
            font, 64f, new Color(0.95f, 0.85f, 0.5f));
        SetFullStretch(logoText);

        // ── 시작 버튼 (중앙) ──
        var startBtn = CreateButton(canvasGo.transform, "StartButton", "시작",
            font, 32f, new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(280f, 72f),
            new Color(0.85f, 0.70f, 0.25f), new Color(0.12f, 0.12f, 0.14f));
        AddOutline(startBtn, new Color(0.9f, 0.75f, 0.3f), 2f);

        // ── 설정 버튼 (우상단) ──
        var settingsBtn = CreateButton(canvasGo.transform, "SettingsButton", "설정",
            font, 24f, new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(100f, 56f),
            new Color(0.25f, 0.25f, 0.28f), new Color(0.9f, 0.9f, 0.9f));
        AddOutline(settingsBtn, new Color(0.6f, 0.6f, 0.6f), 1.5f);

        // MainMenuUI 컴포넌트
        var menuUI = canvasGo.AddComponent<MainMenuUI>();
        menuUI.startButton    = startBtn.GetComponent<Button>();
        menuUI.settingsButton = settingsBtn.GetComponent<Button>();

        // 씬 저장
        EditorSceneManager.SaveScene(scene, ScenePath);

        // 빌드 세팅에 추가
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string guid = AssetDatabase.AssetPathToGUID(ScenePath);
        if (!scenes.Exists(s => s.guid.ToString() == guid))
        {
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        AssetDatabase.Refresh();
        Debug.Log("[BuildMainMenuScene] MainMenu 씬 생성 완료");
    }

    // ── Helpers ───────────────────────────────────────────────

    private static GameObject CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
        return go;
    }

    private static GameObject CreateTMP(Transform parent, string name, string text,
        TMP_FontAsset font, float fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string label,
        TMP_FontAsset font, float fontSize,
        Vector2 anchor, Vector2 anchoredPos, Vector2 size,
        Color bgColor, Color textColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.highlightedColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.1f, bgColor.b + 0.1f);
        cb.pressedColor     = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
        btn.colors = cb;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var textGo = CreateTMP(go.transform, "Text", label, font, fontSize, textColor);
        SetFullStretch(textGo);

        return go;
    }

    private static void SetFullStretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void AddOutline(GameObject go, Color color, float width)
    {
        var outline = go.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(width, -width);
    }
}
