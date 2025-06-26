using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float _shakeCamMin;
    [SerializeField] private float _shakeCamMax;    
    [SerializeField] private float _duration;
    private Camera _cameraMain;

    private void OnEnable()
    {
        EventManager.AddListener<bool>(EventName.OnCameraShake, Shake);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<bool>(EventName.OnCameraShake, Shake);
    }

    void Start()
    {
        _cameraMain = Camera.main;
    }

    public void Shake(bool isShake)
    {
        StartCoroutine(ShakeCoroutin());
        Debug.Log("shake");
    }

    private IEnumerator ShakeCoroutin()
    {
        float timeElapsed = 0;
        float roX = 0;
        float roY = 0;
        Quaternion startRotation = _cameraMain.transform.localRotation;
        while (timeElapsed < _duration)
        {
            roX = Random.Range(_shakeCamMin, _shakeCamMax);
            roY = Random.Range(_shakeCamMin, _shakeCamMax);
            _cameraMain.transform.localRotation = startRotation * Quaternion.Euler(roX,roY,0);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        _cameraMain.transform.localRotation = startRotation;
    }
}
