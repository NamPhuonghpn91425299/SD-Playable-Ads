using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadUIVfx : VFXBase
{
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private RectTransform _crosshairRectTransform;
    [SerializeField] private Text _reloadText;
    private Coroutine _currentCoroutine;
    private float _speedRotation = 2f; 


    public override void Play<T>(T duration)
    {
        _currentCoroutine = StartCoroutine(IEPlayReloadVFX(duration is float speed ? speed : 2f)); // Default speed is 2 if not provided
    }

    public override void Stop<T>(T parameter)
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        _currentCoroutine = null;
    }

    private IEnumerator IEPlayReloadVFX(float duration)
    {
        // Show the crosshair and reload text
        _crosshair.SetActive(false);
        _reloadText.gameObject.SetActive(true);
        _crosshairRectTransform.gameObject.SetActive(true);


        // Rotate the crosshair
        float elapsedTime = 0f;
        while (elapsedTime < duration) // Rotate for the specified duration
        {
            float rotationAngle = _speedRotation * Time.deltaTime * 360f; // Convert speed to degrees
            _crosshairRectTransform.Rotate(0, 0, rotationAngle);
            elapsedTime += Time.deltaTime;
            _reloadText.text = $"{Math.Round(duration - elapsedTime, 1)}";
            yield return null;
        }

        // Hide the crosshair and reload text after rotation
        _crosshair.SetActive(true);
        _reloadText.gameObject.SetActive(false);
        _crosshairRectTransform.gameObject.SetActive(false);

        _currentCoroutine = null;

    }
}
