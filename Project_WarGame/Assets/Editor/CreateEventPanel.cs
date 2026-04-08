using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Stage01 씬에 EventPanel UI를 생성하고 GameUI에 연결합니다.
/// Window > WarGame > Create Event Panel
/// </summary>
public class CreateEventPanel
{
    private const string FontPath = "Assets/Fonts/경기천년제목OTF_Bold SDF.asset";

    [MenuItem("Window/WarGame/Create Event Panel")]
    public static void Execute()
    {
        // ── GameUI 찾기 ───────────────────────────────────────
        var uiGo   = GameObject.Find("UI");
        var gameUI = uiGo?.GetComponent<GameUI>();
        if (gameUI == null) { Debug.LogError("[CreateEventPanel] 'UI' GameObject 또는 GameUI 컴포넌트를 찾을 수 없습니다."); return; }

        var canvas = uiGo.GetComponentInParent<Canvas>() ?? uiGo.GetComponent<Canvas>();
        Transform canvasRoot = canvas != null ? canvas.transform : uiGo.transform;

        // 이미 존재하면 제거 후 재생성
        var existing = canvasRoot.Find("EventPanel");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        // ── EventPanel: 전체화면 투명 오버레이 (입력 차단용) ──
        var panelGo = new GameObject("EventPanel");
        panelGo.transform.SetParent(canvasRoot, false);

        var panelImg = panelGo.AddComponent<Image>();
        panelImg.color         = new Color(0f, 0f, 0f, 0f);   // 완전 투명
        panelImg.raycastTarget = true;                          // 아래 입력 차단

        var panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = panelRt.offsetMax = Vector2.zero;

        // ── DialogBox: 하단 검정 테두리 박스 ─────────────────
        var boxGo = new GameObject("DialogBox");
        boxGo.transform.SetParent(panelGo.transform, false);

        var boxImg = boxGo.AddComponent<Image>();
        boxImg.color = new Color(0f, 0f, 0f, 0.65f);  // 검정 반투명 테두리

        var boxRt = boxGo.GetComponent<RectTransform>();
        boxRt.anchorMin        = new Vector2(0.1625f, 0.04f);
        boxRt.anchorMax        = new Vector2(0.8375f, 0.36f);
        boxRt.offsetMin        = Vector2.zero;
        boxRt.offsetMax        = Vector2.zero;

        // ── DialogInner: 검정 반투명 내부 배경 (테두리 두께 = 4px) ──
        var innerGo = new GameObject("DialogInner");
        innerGo.transform.SetParent(boxGo.transform, false);

        var innerImg = innerGo.AddComponent<Image>();
        innerImg.color = new Color(0f, 0f, 0f, 0.55f);  // 검정 반투명

        var innerRt = innerGo.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero;
        innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(4f,  4f);
        innerRt.offsetMax = new Vector2(-4f, -4f);

        // ── EventText: 대사 텍스트 ────────────────────────────
        var textGo = new GameObject("EventText");
        textGo.transform.SetParent(innerGo.transform, false);

        var tmp              = textGo.AddComponent<TextMeshProUGUI>();
        tmp.font             = font;
        tmp.fontSize         = 26f;
        tmp.alignment        = TextAlignmentOptions.Center;
        tmp.color            = Color.white;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.04f, 0.08f);
        textRt.anchorMax = new Vector2(0.96f, 0.92f);
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;

        // ── Canvas 최상단 고정 ────────────────────────────────
        panelGo.transform.SetAsLastSibling();

        var overrideCanvas = panelGo.AddComponent<Canvas>();
        overrideCanvas.overrideSorting = true;
        overrideCanvas.sortingOrder    = 999;
        panelGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // ── 기본 비활성화 ──────────────────────────────────────
        panelGo.SetActive(false);

        // ── GameUI 연결 ────────────────────────────────────────
        gameUI.eventPanel         = panelGo;
        gameUI.eventText          = tmp;
        gameUI.eventConfirmButton = null;
        EditorUtility.SetDirty(uiGo);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[CreateEventPanel] 대사창 스타일 EventPanel 생성 및 GameUI 연결 완료");
    }
}
