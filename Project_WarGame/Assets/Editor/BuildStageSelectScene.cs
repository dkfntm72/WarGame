using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BuildStageSelectScene
{
    private const string FontPath  = "Assets/Fonts/경기천년제목OTF_Bold SDF.asset";
    private const string ScenePath = "Assets/Scenes/StageSelect.unity";

    [MenuItem("Window/WarGame/Build Stage Select Scene")]
    public static void Execute()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 카메라
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.10f, 0.10f, 0.12f);
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

        // ── 배경 ──
        CreatePanel(canvasGo.transform, "Background",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero,
            new Color(0.10f, 0.10f, 0.12f));

        // ── 타이틀 텍스트 (상단) ──
        var titleGo = CreateTMP(canvasGo.transform, "TitleText", "스테이지 선택",
            font, 40f, new Color(0.95f, 0.85f, 0.5f));
        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = titleRt.anchorMax = titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -30f);
        titleRt.sizeDelta = new Vector2(500f, 60f);

        // ── 스테이지 버튼 5개 (가로 중앙, 세로 상단 영역) ──
        // 버튼 크기: 160×160, 간격: 24
        // 전체 폭: 5*160 + 4*24 = 896  → 각 버튼 앵커포지션 x: -448, -264, -80+(-16?
        // 스텝 = 160+24 = 184  →  시작 x = -(184*2) = -368
        float btnSize   = 160f;
        float btnGap    = 22f;
        float step      = btnSize + btnGap;
        float startX    = -(step * 2f);         // -368
        float rowY      = 80f;                  // 화면 중앙 기준 +80

        var stageButtons = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            bool unlocked = (i == 0);
            var card = CreateStageCard(canvasGo.transform, i + 1, font, unlocked,
                new Vector2(startX + step * i, rowY), new Vector2(btnSize, btnSize));
            stageButtons[i] = card.GetComponent<Button>();
        }

        // ── 메인화면으로 버튼 (좌하단) ──
        var backBtn = CreateButton(canvasGo.transform, "BackButton", "메인화면으로",
            font, 26f,
            new Vector2(0f, 0f), new Vector2(120f, 44f), new Vector2(220f, 56f),
            new Color(0.20f, 0.20f, 0.22f), new Color(0.85f, 0.85f, 0.85f));
        AddOutline(backBtn, new Color(0.6f, 0.6f, 0.6f), 1.5f);

        // ── StageSelectUI 컴포넌트 연결 ──
        var ui = canvasGo.AddComponent<StageSelectUI>();
        ui.stageButtons = stageButtons;
        ui.backButton   = backBtn.GetComponent<Button>();

        // 씬 저장
        EditorSceneManager.SaveScene(scene, ScenePath);

        // 빌드 세팅에 추가 (index 1 — MainMenu=0, StageSelect=1, Stage01=2)
        var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string guid = AssetDatabase.AssetPathToGUID(ScenePath);
        if (!sceneList.Exists(s => s.guid.ToString() == guid))
        {
            // MainMenu 다음(index 1)에 삽입
            int insertAt = 1;
            if (insertAt > sceneList.Count) insertAt = sceneList.Count;
            sceneList.Insert(insertAt, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = sceneList.ToArray();
        }

        AssetDatabase.Refresh();
        Debug.Log("[BuildStageSelectScene] StageSelect 씬 생성 완료");
    }

    // ── 스테이지 카드 ──────────────────────────────────────────
    private static GameObject CreateStageCard(Transform parent, int stageNum, TMP_FontAsset font,
        bool unlocked, Vector2 anchoredPos, Vector2 size)
    {
        // 카드 배경
        var card = new GameObject($"StageCard_{stageNum}");
        card.transform.SetParent(parent, false);

        var img = card.AddComponent<Image>();
        img.color = unlocked ? new Color(0.16f, 0.16f, 0.19f) : new Color(0.12f, 0.12f, 0.14f);

        var btn = card.AddComponent<Button>();
        var cb = btn.colors;
        if (unlocked)
        {
            cb.highlightedColor = new Color(0.25f, 0.25f, 0.30f);
            cb.pressedColor     = new Color(0.10f, 0.10f, 0.12f);
        }
        else
        {
            cb.normalColor    = new Color(1f, 1f, 1f, 0.5f);
            cb.disabledColor  = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }
        btn.colors = btn.colors; // apply

        var rt = card.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        // 테두리
        Color borderColor = unlocked
            ? new Color(0.85f, 0.75f, 0.35f)   // 금색 (해금)
            : new Color(0.35f, 0.35f, 0.38f);   // 회색 (잠금)
        AddOutline(card, borderColor, 2.5f);

        // "스테이지 N" 텍스트
        var labelGo = CreateTMP(card.transform, "Label", $"스테이지\n{stageNum}",
            font, 28f, unlocked ? new Color(0.95f, 0.90f, 0.75f) : new Color(0.45f, 0.45f, 0.47f));
        SetFullStretch(labelGo);

        // 잠금 표시 텍스트
        if (!unlocked)
        {
            var lockGo = CreateTMP(card.transform, "LockLabel", "준비중",
                font, 18f, new Color(0.5f, 0.5f, 0.52f));
            var lockRt = lockGo.GetComponent<RectTransform>();
            lockRt.anchorMin = new Vector2(0f, 0f);
            lockRt.anchorMax = new Vector2(1f, 0.3f);
            lockRt.pivot = new Vector2(0.5f, 0f);
            lockRt.offsetMin = lockRt.offsetMax = Vector2.zero;
        }

        return card;
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
