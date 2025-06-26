using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrationWithCutSceneCam : MonoBehaviour
{
    public Transform cameraTransform; // Transform của camera
    public float shakeAmount = 0.7f; // Độ mạnh của rung
    public float decreaseFactor = 1.0f; // Tốc độ giảm rung
    public float shakeDuration = 0.5f; // Thời gian rung

    private Vector3 _originalPos; // Vị trí ban đầu của camera

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = GetComponent<Transform>();
        }
    }

    public void TriggerShake(float shakeViolence)
    {
        StopAllCoroutines();
        shakeAmount = 1 / shakeViolence;
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float elapsed = 0.0f;
        _originalPos = cameraTransform.localPosition;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-0.5f, 0.5f) * shakeAmount;
            float y = Random.Range(-1.2f, 1.2f) * shakeAmount;
            float z = Random.Range(-0.5f, 0.5f) * shakeAmount;

            cameraTransform.localPosition += new Vector3(x, y, z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cameraTransform.localPosition = _originalPos;
    }
}

