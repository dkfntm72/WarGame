using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private RectTransform fillRect;
    private Canvas        canvas;

    private void Awake()
    {
        var root = transform.Find("HealthBarRoot");
        if (root == null) return;

        canvas   = root.GetComponent<Canvas>();
        fillRect = root.Find("Fill")?.GetComponent<RectTransform>();

        canvas.gameObject.SetActive(false);
    }

    public void SetHP(int current, int max)
    {
        if (fillRect == null || canvas == null) return;

        float ratio = max > 0 ? (float)current / max : 0f;

        // Shrink the fill bar by moving its right anchor
        fillRect.anchorMax = new Vector2(ratio, 1f);
        fillRect.offsetMax = Vector2.zero;

        canvas.gameObject.SetActive(current < max);
    }
}
