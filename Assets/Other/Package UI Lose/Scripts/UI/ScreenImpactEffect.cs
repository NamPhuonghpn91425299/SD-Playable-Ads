using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Random = UnityEngine.Random;

public class ScreenImpactEffect : MonoBehaviour
{
    public Image impactImage; // UI Image để tạo hiệu ứng
    public float impactScale = 1.2f; // Scale khi va chạm
    public float shakeAmount = 10f; // Độ lắc nhẹ
    public float fadeOutTime = 0.3f; // Thời gian mờ dần
    public float delayTime = 0.3f; // Thời gian mờ dần
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource audioSource_2;
    [SerializeField] AudioClip[] audioClips;
    private Vector3 originalScale;
    private Vector2 originalPosition;
    private bool isShowing = false;

    public void ShowScreenImpact(bool _loseOrWin)
    {
        originalScale = impactImage.rectTransform.localScale;
        originalPosition = impactImage.rectTransform.anchoredPosition;
        impactImage.color = new Color(impactImage.color.r, impactImage.color.g, impactImage.color.b, 0);
        audioSource_2.Play();
        if (!isShowing)
        {
            if (!_loseOrWin)
            {
                
                ShowImpact();
                isShowing = true;
            }
            else if(_loseOrWin)
            {
                audioSource.PlayOneShot(audioClips[2]);
                isShowing = true;
            }
        }
    }

    public void ShowImpact()
    {

        StopAllCoroutines();
        StartCoroutine(ImpactEffect());
    }

    private IEnumerator ImpactEffect()
    {
        audioSource.PlayOneShot(audioClips[0]);
        impactImage.enabled = true;
        Color color = impactImage.color;

        // Hiện ảnh ngay lập tức
        color.a = 1;
        impactImage.color = color;

        // Scale to lên nhanh chóng
        impactImage.rectTransform.localScale = originalScale * impactScale;

        // Lắc nhẹ để tạo hiệu ứng "đập"
        for (int i = 0; i < 5; i++)
        {
            impactImage.rectTransform.anchoredPosition = originalPosition + new Vector2(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount)
            );
            yield return HelperCoroutine.GetWait(0.02f);
        }

        // Đưa về vị trí ban đầu
        impactImage.rectTransform.localScale = originalScale;
        impactImage.rectTransform.anchoredPosition = originalPosition;

        yield return HelperCoroutine.GetWait(delayTime); // Giữ ảnh trên màn hình một chút
        audioSource.PlayOneShot(audioClips[1]);
        yield return HelperCoroutine.GetWait(audioClips[1].length);
        audioSource.clip = audioClips[2];
        audioSource.Play();
        audioSource.loop = true;

    }
}