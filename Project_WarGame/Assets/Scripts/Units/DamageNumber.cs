using UnityEngine;
using TMPro;
using System.Collections;

public class DamageNumber : MonoBehaviour
{
    private TextMeshPro tmp;

    public void Setup(int amount, bool isHeal)
    {
        tmp = GetComponent<TextMeshPro>();
        tmp.text  = isHeal ? $"+{amount}" : $"-{amount}";
        tmp.color = isHeal ? new Color(0.3f, 1f, 0.3f) : Color.white;
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float duration = 1.0f;
        float elapsed  = 0f;
        Vector3 start  = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;

            transform.position = start + Vector3.up * (t * 0.8f);

            Color c = tmp.color;
            c.a     = 1f - t;
            tmp.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}
