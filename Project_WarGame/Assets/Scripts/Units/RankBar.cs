using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유닛 오른쪽에 세로로 표시되는 등급 UI (0~5단계).
/// Unit.Initialize() 호출 시 자동 생성되며, SetRank()로 갱신한다.
/// </summary>
public class RankBar : MonoBehaviour
{
    private const int MaxRank = 5;

    private static readonly Color ActiveColor   = new Color(1f, 0.82f, 0.1f);          // 금색
    private static readonly Color InactiveColor = new Color(0.2f, 0.2f, 0.2f, 0.55f);  // 어두운 회색

    private Image[]    _bars;
    private Canvas     _canvas;
    private GameObject _root;

    private void Awake()
    {
        _bars = new Image[MaxRank];

        // ── World-Space Canvas 생성 ──────────────────────────────
        _root = new GameObject("RankBarRoot");
        _root.transform.SetParent(transform, false);

        _canvas = _root.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;

        var rt = _root.GetComponent<RectTransform>();
        // 유닛 오른쪽, 가로로 긴 바를 세로로 쌓은 형태
        rt.localPosition = new Vector3(0.35f, 0.2f, 0f);
        rt.localScale    = Vector3.one * 0.007f;
        rt.sizeDelta     = new Vector2(32f, 64f);

        // ── 가로로 긴 바 5개를 세로로 쌓기 (아래 = 1등급, 위 = 5등급) ──
        const float barW  = 28f;
        const float barH  = 9f;
        const float gap   = 3f;
        float totalH = MaxRank * barH + (MaxRank - 1) * gap;
        float startY = -totalH * 0.5f + barH * 0.5f;

        for (int i = 0; i < MaxRank; i++)
        {
            var go = new GameObject($"Bar{i}");
            go.transform.SetParent(_root.transform, false);

            var img = go.AddComponent<Image>();
            img.color = InactiveColor;
            _bars[i] = img;

            var brt = go.GetComponent<RectTransform>();
            brt.anchoredPosition = new Vector2(0f, startY + i * (barH + gap));
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

    public void SetSortingOrder(int order)
    {
        if (_canvas != null) _canvas.sortingOrder = order;
    }
}
