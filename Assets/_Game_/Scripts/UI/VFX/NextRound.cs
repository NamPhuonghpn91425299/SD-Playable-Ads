using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextRound : VFXBase
{
    private Animator _animator;
    private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 1f; // Duration for the fade effect
    [SerializeField] float _waitStartFade = 1f; // Time to wait before starting the fade

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    
    void OnEnable()
    {
        _canvasGroup.alpha = 1f;
        StartCoroutine(FadeOutCoroutine(_fadeDuration));

    }

    public override void Play<T>(T duration)
    {
        _canvasGroup.alpha = 1f;
        _animator.Play("E_NextRound_anim", 0, 0f);
        StartCoroutine(FadeOutCoroutine(duration is float fadeDuration ? fadeDuration : _fadeDuration));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        yield return HelperCoroutine.GetWait(_waitStartFade);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = 1f - (elapsed / duration);
            yield return null;
        }
        _canvasGroup.alpha = 0f;
    }
}
