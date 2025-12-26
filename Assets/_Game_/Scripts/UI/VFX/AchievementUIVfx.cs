using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUIVfx : VFXBase
{
    [SerializeField] private Animator _animatorMain;
    [SerializeField] private Animator _animatorMedal1;
    [SerializeField] private Animator _animatorLight;
    [SerializeField] private float _duration = 1.5f;
    [SerializeField] private float _delay = 1f;
    [SerializeField] private Text textClaimed;
    [SerializeField] private int _claimCount = 0;

    private Coroutine _followCoroutine;

    public override void Play<T>(T parameter)
    {
        _animatorMain.SetTrigger("Main");
        if (_followCoroutine != null) StopCoroutine(_followCoroutine);
        _followCoroutine = StartCoroutine(IEDelayActiveLight());
        _claimCount++;
    }


    IEnumerator IEDelayActiveLight()
    {
        yield return HelperCoroutine.GetWait(_delay);
        _animatorLight.gameObject.SetActive(true);
        _animatorLight.SetTrigger("Play");
        yield return HelperCoroutine.GetWait(_duration);
        _animatorLight.gameObject.SetActive(false);
        _animatorMedal1.SetTrigger("Play");
        textClaimed.text = _claimCount.ToString();
    }
}
