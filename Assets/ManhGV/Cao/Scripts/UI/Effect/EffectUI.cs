using System;
using System.Collections;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class EffectUI : VFXBase
{
    public static EffectUI Instance;
    
    [SerializeField] Image _effect2;
    [SerializeField] Image _effect3;
    
    [SerializeField] Image Background;

    [SerializeField] float _duration = 1f;
    [SerializeField] float maxY = 31f;
    [SerializeField] float minY = -871f;
    [SerializeField] float maxX = 958f;
    [SerializeField] float minX = -867f;
    [SerializeField] bool _isPlay = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetAlpha(_effect2, 0);
        SetAlpha(_effect3, 0);
        SetAlpha(Background, 0);
        if (Screen.width < Screen.height)
        {
            minY = -738f;
            maxY = -181f;
            minX = -365f;
            maxX = 356f;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public override void Play()
    {
        if (_isPlay) return;
        _isPlay = true;
        int randomEffect = Random.Range(0, 2);
        // Reset alpha về 1 cho tất cả
        SetAlpha(_effect2, 0);
        SetAlpha(_effect3, 0);
        SetAlpha(Background, 0);

        Image chosenEffect = null;
        switch (randomEffect)
        {
            case 0:
                chosenEffect = _effect2;
                break;
            case 1:
                chosenEffect = _effect3;
                break;
        }
        if (chosenEffect != null && chosenEffect.gameObject.activeInHierarchy)
        {
            RectTransform chosenRectTransform = chosenEffect.GetComponent<RectTransform>();
            // Make sure Background is active and enabled
            if (Background != null && Background.gameObject.activeInHierarchy)
            {
                SetAlpha(Background, 1);
                StartCoroutine(FadeOutEffect(Background));
            }
            SetAlpha(chosenEffect, 1);
            chosenRectTransform.anchoredPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            chosenRectTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));
            // Only start coroutine if the effect is active
            StartCoroutine(FadeOutEffect(chosenEffect));
        }

    }
    public Vector3 GetRandomScreenPositionInRect(RectTransform rectTransform)
    {
        Vector2 size = rectTransform.rect.size;
        Vector2 pivot = rectTransform.pivot;

        // Random toạ độ trong local space dựa theo pivot
        float x = UnityEngine.Random.Range(-size.x * pivot.x, size.x * (1 - pivot.x));
        float y = UnityEngine.Random.Range(-size.y * pivot.y, size.y * (1 - pivot.y));
        Vector2 localPos = new Vector2(x, y);

        // Chuyển sang toạ độ thế giới
        Vector3 worldPos = rectTransform.TransformPoint(localPos);

        // Chuyển sang toạ độ màn hình
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);

        return screenPos;
    }
    private IEnumerator FadeOutEffect(Image img)
    {
        float t = 0f;
        float speed = 1f;
        Color color = img.color;
        while (color.a > 0.01f)
        {
            // Giảm alpha theo tốc độ giảm dần
            t += Time.deltaTime / _duration;
            speed = Mathf.Lerp(1f, 0.2f, t); // speed giảm dần
            color.a = Mathf.Lerp(1f, 0f, t * speed);
            img.color = color;
            yield return null;
        }
        color.a = 0f;
        img.color = color;
        _isPlay = false;
    }

    private void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }



    public override void SetActive(bool active)
    {

    }

    public override void Stop()
    {

    }
}
