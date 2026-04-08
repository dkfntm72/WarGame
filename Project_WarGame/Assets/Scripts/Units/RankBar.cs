using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유닛 위에 표시되는 등급 막대 UI (0~5단계).
/// Unit.Initialize() 호출 시 자동 생성되며, SetRank()로 갱신한다.
/// </summary>
public class RankBar : MonoBehaviour
{
    private const int MaxRank = 5;

    private static readonly Color ActiveColor   = new Color(1f, 0.82f, 0.1f);       // 금색
    private static readonly Color InactiveColor = new Color(0.25f, 0.25f, 0.25f, 0.65f);

    private Image[]     _bars;
    private GameObject  _root;

    private void Awake()
    {
        _bars = new Image[MaxRank];

        // ── World-Space Canvas 생성 ──────────────────────────────
        _root = new GameObject("RankBarRoot");
        _root.transform.SetParent(transform, false);

        var canvas = _root.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.sortingOrder = 6;   // HP바(5)보다 위

        var rt = _root.GetComponent<RectTransform>();
        rt.localPosition = new Vector3(0f, -0.55f, 0f);   // HP바 바로 아래
        rt.localScale    = Vector3.one * 0.01f;
        rt.sizeDelta     = new Vector2(96f, 12f);

        // ── 막대 5개 생성 ────────────────────────────────────────
        const float barW  = 14f;
        const float barH  = 8f;
        const float gap   = 2f;
        float startX = -(MaxRank * barW + (MaxRank - 1) * gap) * 0.5f + barW * 0.5f;

        for (int i = 0; i < MaxRank; i++)
        {
            var go = new GameObject($"Bar{i}");
            go.transform.SetParent(_root.transform, false);

            var img = go.AddComponent<Image>();
            img.color = InactiveColor;
            _bars[i] = img;

            var brt = go.GetComponent<RectTransform>();
            brt.anchoredPosition = new Vector2(startX + i * (barW + gap), 0f);
            brt.sizeDelta        = new Vector2(barW, barH);
        }

        _root.SetActive(false);   // 0등급에서는 숨김
    }

    public void SetRank(int rank)
    {
        if (_root == null) return;
        _root.SetActive(rank > 0);
        for (int i = 0; i < MaxRank; i++)
            _bars[i].color = i < rank ? ActiveColor : InactiveColor;
    }
}
