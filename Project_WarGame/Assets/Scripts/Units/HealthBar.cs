using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Image     fillImage;
    private Canvas    canvas;
    private Coroutine hideCoroutine;

    private const float HideDelay = 2f;

    private void Awake()
    {
        var root = transform.Find("HealthBarRoot");
        if (root == null) return;

        canvas    = root.GetComponent<Canvas>();
        fillImage = root.Find("Fill")?.GetComponent<Image>();

        canvas.gameObject.SetActive(false);
    }

    public void SetHP(int current, int max)
    {
        if (fillImage == null || canvas == null) return;

        float ratio = max > 0 ? (float)current / max : 0f;

        fillImage.fillAmount = ratio;

        if (current < max)
        {
            canvas.gameObject.SetActive(true);
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideAfterDelay());
        }
        else
        {
            if (hideCoroutine != null) { StopCoroutine(hideCoroutine); hideCoroutine = null; }
            canvas.gameObject.SetActive(false);
        }
    }

    public void SetSortingOrder(int order)
    {
        if (canvas != null) canvas.sortingOrder = order;
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(HideDelay);
        if (canvas != null) canvas.gameObject.SetActive(false);
        hideCoroutine = null;
    }
}
