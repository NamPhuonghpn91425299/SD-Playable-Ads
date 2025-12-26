using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoseGameUI : UIBase
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 1f;
    [SerializeField] private ScreenImpactEffect _screenImpactEffect;
    [SerializeField] private float _timerDelayActiveBackground = 1.5f;
    

    void Awake()
    {
        this._continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    void OnEnable()
    {
        Luna.Unity.LifeCycle.GameEnded();
        _screenImpactEffect.ShowScreenImpact(false);
        StartCoroutine(FadeIn());
    }

    private void OnContinueButtonClicked()
    {
        Luna.Unity.Playable.InstallFullGame();
    }

    private IEnumerator FadeIn()
    {
        yield return HelperCoroutine.GetWait(_timerDelayActiveBackground);
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeDuration);
            yield return null;
        }

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        yield return HelperCoroutine.GetWait(delayTimeScale0);
        
    }
}
