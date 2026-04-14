using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class StageSelectUI : MonoBehaviour
{
    [Header("Stage Buttons (index 0 = Stage1)")]
    public Button[] stageButtons;

    [Header("Selection Indicator")]
    // 순서: [0]좌상, [1]우상, [2]좌하, [3]우하
    public Sprite[] selectionCorners = new Sprite[4];

    [Header("Stage Data (index 0 = Stage1)")]
    public MapData[] stageDatas;

    [Header("Stage Description")]
    public TextMeshProUGUI stageTitleText;
    public TextMeshProUGUI stageOverviewText;

    [Header("Navigation")]
    public Button startButton;
    public Button backButton;

    // 현재 구현된 스테이지만 활성화 (나머지는 잠금 처리)
    private static readonly string[] SceneNames = { "Stage01", null, null, null, null };

    private int _selectedIndex = -1;
    private GameObject[] _selectionIndicators;

    private static readonly Color DeselectedColor = new Color(1f, 1f, 1f, 1f);
    // 시작 버튼 색상
    private static readonly Color StartBrightColor = new Color(1f,   1f,    1f,   1f);
    private static readonly Color StartDimColor    = new Color(0.45f, 0.45f, 0.45f, 1f);

    private void Start()
    {
        // 코너 배치 정의: 순서 = [0]좌상, [1]우상, [2]좌하, [3]우하
        var cornerDefs = new (Vector2 anchor, Vector2 pivot)[]
        {
            (new Vector2(0f, 1f), new Vector2(0f, 1f)),  // 좌상
            (new Vector2(1f, 1f), new Vector2(1f, 1f)),  // 우상
            (new Vector2(0f, 0f), new Vector2(0f, 0f)),  // 좌하
            (new Vector2(1f, 0f), new Vector2(1f, 0f)),  // 우하
        };

        _selectionIndicators = new GameObject[stageButtons.Length];
        for (int i = 0; i < stageButtons.Length; i++)
        {
            var root = new GameObject("SelectionIndicator");
            root.transform.SetParent(stageButtons[i].transform, false);
            var rootRt = root.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            for (int c = 0; c < 4; c++)
            {
                var (anchor, pivot) = cornerDefs[c];
                var go = new GameObject("Corner_" + c);
                go.transform.SetParent(root.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin        = anchor;
                rt.anchorMax        = anchor;
                rt.pivot            = pivot;
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta        = new Vector2(50f, 50f);

                var img = go.AddComponent<Image>();
                if (selectionCorners != null && c < selectionCorners.Length)
                    img.sprite = selectionCorners[c];
                img.raycastTarget = false;
            }

            root.SetActive(false);
            _selectionIndicators[i] = root;
        }

        for (int i = 0; i < stageButtons.Length; i++)
        {
            int idx = i;
            if (SceneNames[idx] != null)
            {
                stageButtons[idx].onClick.AddListener(() => SelectStage(idx));
                AddHoverListener(stageButtons[idx], idx);

                // 호버/선택 색 변화 없애기
                DisableHoverColor(stageButtons[idx]);

                if (PlayerPrefs.GetInt("Cleared_" + SceneNames[idx], 0) == 1)
                    ShowClearBadge(stageButtons[idx]);
            }
            else
            {
                stageButtons[idx].interactable = false;
                var colors = stageButtons[idx].colors;
                colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                stageButtons[idx].colors = colors;
            }
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
            // 초기 비선택 상태: 어둡게
            SetStartButtonDim(true);
        }

        backButton.onClick.AddListener(OnBackClicked);
    }

    // 스테이지 버튼 호버/선택 색 변화 제거
    private void DisableHoverColor(Button button)
    {
        var colors = button.colors;
        colors.highlightedColor = DeselectedColor;
        colors.selectedColor    = DeselectedColor;
        colors.pressedColor     = new Color(0.85f, 0.85f, 0.85f, 1f);
        button.colors = colors;
    }

    // 시작 버튼 밝기 제어
    private void SetStartButtonDim(bool dim)
    {
        if (startButton == null) return;
        var colors = startButton.colors;
        if (dim)
        {
            colors.normalColor      = StartDimColor;
            colors.highlightedColor = StartDimColor;
            colors.selectedColor    = StartDimColor;
            colors.pressedColor     = StartDimColor;
            colors.disabledColor    = StartDimColor;
        }
        else
        {
            colors.normalColor      = StartBrightColor;
            colors.highlightedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor    = StartBrightColor;
            colors.pressedColor     = new Color(0.6f, 0.6f, 0.6f, 1f);
            colors.disabledColor    = StartDimColor;
        }
        startButton.colors       = colors;
        startButton.interactable = !dim;
    }

    private void SelectStage(int idx)
    {
        _selectedIndex = idx;

        // 인디케이터 표시 갱신
        for (int i = 0; i < _selectionIndicators.Length; i++)
        {
            if (_selectionIndicators[i] != null)
                _selectionIndicators[i].SetActive(i == idx);
        }

        ShowStageDescription(idx);

        SetStartButtonDim(SceneNames[idx] == null);
    }

    private void AddHoverListener(Button button, int idx)
    {
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var entry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener(_ => ShowStageDescription(idx));
        trigger.triggers.Add(entry);
    }

    private void ShowStageDescription(int idx)
    {
        if (stageDatas == null || idx >= stageDatas.Length || stageDatas[idx] == null)
        {
            if (stageTitleText != null)    stageTitleText.text    = "";
            if (stageOverviewText != null) stageOverviewText.text = "";
            return;
        }

        var data = stageDatas[idx];
        if (stageTitleText != null)    stageTitleText.text    = data.stageTitle;
        if (stageOverviewText != null) stageOverviewText.text = data.stageOverview;
    }

    private void ShowClearBadge(Button button)
    {
        var existingTmp = button.GetComponentInChildren<TextMeshProUGUI>();
        TMP_FontAsset font = existingTmp?.font;

        var go = new GameObject("ClearBadge");
        go.transform.SetParent(button.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = "★ CLEAR";
        tmp.fontSize  = 20;
        tmp.color     = new Color(1f, 0.85f, 0.2f, 1f);
        tmp.alignment = TextAlignmentOptions.Bottom;
        tmp.fontStyle = FontStyles.Bold;
        tmp.margin    = new Vector4(0, 0, 0, 8f);
        if (font != null) tmp.font = font;
    }

    private void OnStartClicked()
    {
        if (_selectedIndex < 0 || _selectedIndex >= SceneNames.Length) return;
        if (SceneNames[_selectedIndex] == null) return;
        SceneManager.LoadScene(SceneNames[_selectedIndex]);
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
