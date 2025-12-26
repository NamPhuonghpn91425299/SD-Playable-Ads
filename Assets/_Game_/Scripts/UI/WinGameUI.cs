using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinGameUI : UIBase
{
    [SerializeField] private Button _continueButton;
    private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 1f;
    void Awake()
    {
        this._continueButton.onClick.AddListener(OnContinueButtonClicked);
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    void OnEnable()
    {
        Luna.Unity.LifeCycle.GameEnded();
        StartCoroutine(FadeIn());
    }

    private void OnContinueButtonClicked()
    {
        Luna.Unity.Playable.InstallFullGame();
    }

    private IEnumerator FadeIn()
    {
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
