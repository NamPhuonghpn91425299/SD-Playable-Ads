using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class UIAnimSimulator : MonoBehaviour
{
    public RectTransform TextRect;
    public CanvasGroup TextCanvas;
    public float durationAppear = 2.0f; // Thời gian để chữ hiện rõ và to lên hoặc ngược lại
    public float durationDisappear = 2.0f; // Thời gian để chữ hiện rõ và to lên hoặc ngược lại
    public float existTime = 2.0f; // Thời gian để chữ hiện rõ và to lên hoặc ngược lại
    public Vector3 DefaultScale; // Thời gian để chữ hiện rõ và to lên hoặc ngược lại
    public Vector3 targetScale; // Kích thước tối đa của chữ

    public GameObject EndGameObj;
    public RectTransform flare;
    public float rotationSpeed = 90f; // Tốc độ xoay, đơn vị là độ/giây
    public RectTransform WeaponIcon;
    public float WeaponIconValue;
    public RectTransform WeaponName;
    public float WeaponNameValue;
    public float durationTime;
    public bool isShow;

    public void StartAnimateTextAppear()
    {
        StartCoroutine(AnimateTextAppear());
    }
   
    private void Update()
    {
        if (isShow)
        {
            flare.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
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

    public IEnumerator ShowUIEndGame()
    {
        isShow = false;
        yield return new WaitForSeconds(4f);
        EndGameObj.SetActive(true);
        float WeaponIconStart = WeaponIcon.anchoredPosition3D.y;
        float WeaponIconEnd = WeaponIcon.anchoredPosition3D.y + WeaponIconValue;
        float WeaponNameStart = WeaponName.anchoredPosition3D.x;
        float WeaponNameEnd = WeaponName.anchoredPosition3D.x + WeaponNameValue;
        float Showtime = 0f;
        while (Showtime < durationTime)
        {
            float t = Showtime / durationTime;
            float newWeaponIconX = Mathf.Lerp(WeaponIconStart, WeaponIconEnd, t);
            float newWeaponNameX = Mathf.Lerp(WeaponNameStart, WeaponNameEnd, t);

            WeaponIcon.anchoredPosition = new Vector3(WeaponIcon.anchoredPosition.x, newWeaponIconX, 0);
            WeaponName.anchoredPosition = new Vector3(newWeaponNameX, WeaponName.anchoredPosition.y, 0);

            Showtime += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo rằng các phần tử UI đến đúng vị trí cuối cùng
        WeaponIcon.anchoredPosition = new Vector3(WeaponIcon.anchoredPosition.x, WeaponIconEnd, 0);
        WeaponName.anchoredPosition = new Vector3(WeaponNameEnd, WeaponName.anchoredPosition.y, 0);
        isShow = true;
    }
}
