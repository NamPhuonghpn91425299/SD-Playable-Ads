using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICrosshairItem : MonoBehaviour
{
    // Singleton instance
    private static UICrosshairItem _instance;

    // Accessor for the singleton instance
    public static UICrosshairItem Instance
    {
        get
        {
            return _instance;
        }
    }
    public RectTransform CrosshairRectTransform;
    public RectTransform CrosshairRectObj;
    public List<Image> CrosshairImg;
    public List<Color> CrosshairColor;
    [SerializeField] private float _distanceMax = 100;
    [SerializeField] private float _distanceMin = 16;
    [SerializeField] private float _speedShrink = 120;

    [SerializeField] private float _currentSpeed; // value: 0 -> 1 => speed
    private float _currentDistance;

    [Header("Rate Change Aim")]
    [SerializeField]
    private float _rateOnMove = 1;

    [SerializeField] private float _rateOnAttack = 15;
    private float _rateByDistance = 0;

    public float _noDamageDuration = 5f; // Thời gian tùy chỉnh
    private Coroutine _noDamageCoroutine;

    private void Awake()
    {
        _instance = this;
    }

    private void OnEnable()
    {
        EventManager.AddListener<bool>(EventName.OnCheckBotTakeDamage, OnCheckBotTakeDamage);
    }
    private void OnDisable()
    {
        EventManager.RemoveListener<bool>(EventName.OnCheckBotTakeDamage, OnCheckBotTakeDamage);
    }

    private void OnCheckBotTakeDamage(bool IsTakeDamage)
    {
        if (IsTakeDamage)
        {
            if (_noDamageCoroutine != null)
            {
                StopCoroutine(_noDamageCoroutine);
            }

            CrosshairRectObj.localRotation = Quaternion.Euler(0, 0, 45);

            foreach (var img in CrosshairImg)
            {
                img.color = CrosshairColor[1];
            }
            _noDamageCoroutine = StartCoroutine(NoDamageTimer());
        }
        else
        {
            CrosshairRectObj.localRotation = Quaternion.Euler(0, 0, 0);
            foreach (var img in CrosshairImg)
            {
                img.color = CrosshairColor[0];
            }
        }
    }

    private IEnumerator NoDamageTimer()
    {
        yield return new WaitForSeconds(_noDamageDuration);

        // Logic khi không có bot nào dính damage trong khoảng thời gian tùy chỉnh
        CrosshairRectObj.localRotation = Quaternion.Euler(0, 0, 0);
        foreach (var img in CrosshairImg)
        {
            img.color = CrosshairColor[0];
        }
    }

    public void Expand_Crosshair(float valueChange)
    {
        if (CrosshairRectTransform.sizeDelta.x >= _distanceMax) return;

        _currentDistance = CrosshairRectTransform.sizeDelta.x + valueChange;
        _currentDistance = _distanceMax > _currentDistance ? _currentDistance : _distanceMax;

        CrosshairRectTransform.sizeDelta = Vector2.one * _currentDistance;
    }
    public void Narrow_Crosshair()
    {
        if (CrosshairRectTransform.sizeDelta.x <= _distanceMin) return;

        float currentRate = (_currentDistance - _distanceMin) / (_distanceMax - _distanceMin);

        // Distance: current -> min => Rate: 1 -> 0
        _rateByDistance = (_currentDistance > _distanceMin)
            ? (currentRate > 0.3f) ? currentRate : 0.3f
            : 0;

        _currentDistance = (_distanceMin < _currentDistance)
            ? CrosshairRectTransform.sizeDelta.x - (Time.deltaTime * _speedShrink * _rateByDistance)
            : _distanceMin;

        CrosshairRectTransform.sizeDelta = Vector2.one * _currentDistance;
    }
}
