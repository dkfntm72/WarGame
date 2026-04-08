using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StageSelectUI : MonoBehaviour
{
    [Header("Stage Buttons (index 0 = Stage1)")]
    public Button[] stageButtons;

    [Header("Navigation")]
    public Button backButton;

    // 현재 구현된 스테이지만 활성화 (나머지는 잠금 처리)
    private static readonly string[] SceneNames = { "Stage01", null, null, null, null };

    private void Start()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int idx = i;
            if (SceneNames[idx] != null)
            {
                stageButtons[idx].onClick.AddListener(() => OnStageClicked(idx));

                if (PlayerPrefs.GetInt("Cleared_" + SceneNames[idx], 0) == 1)
                    ShowClearBadge(stageButtons[idx]);
            }
            else
            {
                // 미구현 스테이지 비활성화
                stageButtons[idx].interactable = false;
                var colors = stageButtons[idx].colors;
                colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                stageButtons[idx].colors = colors;
            }
        }

        backButton.onClick.AddListener(OnBackClicked);
    }

    private void ShowClearBadge(Button button)
    {
        // 기존 버튼의 TMP 폰트 상속
        var existingTmp = button.GetComponentInChildren<TextMeshProUGUI>();
        TMP_FontAsset font = existingTmp?.font;

        var go = new GameObject("ClearBadge");
        go.transform.SetParent(button.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = "★ CLEAR";
        tmp.fontSize  = 20;
        tmp.color     = new Color(1f, 0.85f, 0.2f, 1f);
        tmp.alignment = TextAlignmentOptions.Bottom;
        tmp.fontStyle = FontStyles.Bold;
        tmp.margin    = new Vector4(0, 0, 0, 8f);
        if (font != null) tmp.font = font;
    }

    private void OnStageClicked(int idx)
    {
        SceneManager.LoadScene(SceneNames[idx]);
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
