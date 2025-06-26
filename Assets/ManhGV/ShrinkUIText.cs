using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShrinkLoopUIText : MonoBehaviour
{
    public Text uiText;
    public float duration = 1.0f;
    public float targetScale = 0.5f;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 smallScale;

    void Start()
    {
        if (uiText != null)
        {
            rectTransform = uiText.GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
            smallScale = originalScale * targetScale;

            StartCoroutine(LoopShrinkEffect());
        }
    }

    IEnumerator LoopShrinkEffect()
    {
        while (true)
        {
            // Thu nhỏ
            yield return StartCoroutine(ScaleOverTime(rectTransform, originalScale, smallScale, duration));
            // Phóng to lại
            yield return StartCoroutine(ScaleOverTime(rectTransform, smallScale, originalScale, duration));
        }
    }

    IEnumerator ScaleOverTime(RectTransform target, Vector3 from, Vector3 to, float time)
    {
        float elapsed = 0f;

        while (elapsed < time)
        {
            float t = elapsed / time;
            target.localScale = Vector3.Lerp(from, to, t);
            elapsed += Time.unscaledDeltaTime; // hoạt động kể cả khi Time.timeScale = 0
            yield return null;
        }

        target.localScale = to;
    }
}