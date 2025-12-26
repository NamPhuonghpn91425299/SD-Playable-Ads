using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakeDamageUIVfx : VFXBase
{
    [SerializeField] private Image _image;

    private Coroutine _currentCoroutine;

    public override void Play<T>(T parameter)
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        _currentCoroutine = StartCoroutine(IEPlayDamageVFX(parameter));
    }

    private IEnumerator IEPlayDamageVFX<T>(T parameter)
    {
        // Convert parameter to float (speed)
        float fadeSpeed = 1f;
        if (parameter is float speed)
        {
            fadeSpeed = speed;
        }
        else if (float.TryParse(parameter.ToString(), out float parsedSpeed))
        {
            fadeSpeed = parsedSpeed;
        }

        // Set initial alpha to 1 (fully visible)
        Color color = _image.color;
        color.a = 1f;
        _image.color = color;

        // Fade out gradually
        while (color.a > 0f)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            color.a = Mathf.Clamp01(color.a); // Keep alpha between 0 and 1
            _image.color = color;
            yield return null;
        }

        // Ensure alpha is exactly 0 at the end
        color.a = 0f;
        _image.color = color;
        
        _currentCoroutine = null;
    }
}
