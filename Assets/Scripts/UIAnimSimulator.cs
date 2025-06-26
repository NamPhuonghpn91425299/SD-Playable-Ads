using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class UIAnimSimulator : MonoBehaviour
{
    public RectTransform TextRect;
    public CanvasGroup TextCanvas;
    public float durationAppear = 0.5f; // Thời gian để chữ hiện rõ và to lên hoặc ngược lại
    public float durationDisappear = 0.5f; // Thời gian để chữ hiện rõ và to lên hoặc ngược lại
    public float existTime = 2.0f; // Thời gian để chữ hiện rõ và to lên hoặc ngược lại
    public Vector3 DefaultScale = new Vector3(0.9f, 0.9f, 0.9f); // Thời gian để chữ hiện rõ và to lên hoặc ngược lại
    public Vector3 targetScale = new Vector3(1.1f, 1.1f, 1.1f); // Kích thước tối đa của chữ
    [FormerlySerializedAs("WeaponIcon")] public RectTransform[] _group;
    public float moveSpeed = 1200f; // Tốc độ di chuyển có thể chỉnh
    public void StartAnimateTextAppear()
    {
        StartCoroutine(AnimateTextAppear());
    }

    IEnumerator AnimateTextAppear()
    {
        yield return new WaitForSeconds(0.5f);
        Vector3 initialScale = DefaultScale;
        TextCanvas.alpha = 0;
        TextRect.localScale = initialScale;
        float elapsedTime = 0f;

        while (elapsedTime < durationAppear)
        {
            float t = elapsedTime / durationAppear;
            TextCanvas.alpha = Mathf.Lerp(0, 1, t);
            TextRect.localScale = Vector3.Lerp(initialScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        TextCanvas.alpha = 1;
        TextRect.localScale = targetScale;

        yield return new WaitForSeconds(existTime);
        float initialAlpha = TextCanvas.alpha;
        float elapsedTime2 = 0f;

        while (elapsedTime2 < durationDisappear)
        {
            float t = elapsedTime2 / durationDisappear;
            TextCanvas.alpha = Mathf.Lerp(initialAlpha, 0, t);
            TextRect.localScale = Vector3.Lerp(targetScale, DefaultScale, t);
            elapsedTime2 += Time.deltaTime;
            yield return null;
        }

        TextCanvas.alpha = 0;
        TextRect.localScale = DefaultScale;
    }

    public void StartShowUIEndGame()
    {
        StartCoroutine(ShowUIEndGame(_group[1]));
    }   
    public void ShowUIEndGameWin()
    {
        StartCoroutine(ShowUIEndGame(_group[0]));
    }
    public IEnumerator ShowUIEndGame(RectTransform group)
    {
        Vector3 startWeaponIcon = group.anchoredPosition;
        Vector3 targetPosition = Vector3.zero; // Mục tiêu là (0,0,0)
        float distanceIcon = Vector3.Distance(startWeaponIcon, targetPosition);
        float timeToMoveIcon = distanceIcon / moveSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < Mathf.Max(timeToMoveIcon))
        {
            float tIcon = Mathf.Clamp01(elapsedTime / timeToMoveIcon);
            group.anchoredPosition = Vector3.Lerp(startWeaponIcon, targetPosition, tIcon);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Đảm bảo vị trí chính xác là (0,0,0)
        group.anchoredPosition = targetPosition;
    }

}
