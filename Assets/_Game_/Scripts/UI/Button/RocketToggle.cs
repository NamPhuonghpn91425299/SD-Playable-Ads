using System;
using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RocketToggle : MonoBehaviour,
Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<RocketEvent>
{
    [SerializeField] private RectTransform _switchRectTransform;
    [SerializeField] private float _position_min_x = -50f;
    [SerializeField] private float _position_max_x = 50f;
    [SerializeField] private Button _switchButton;
    [SerializeField] private Sprite _rocketOnSprite;
    [SerializeField] private Sprite _rocketOffSprite;
    [SerializeField] private Image _rocketImage;
    [SerializeField] private Image _rocketIconImageOn;
    [SerializeField] private Image _rocketIconImageOff;
    private bool _isRocketOn = false;
    [SerializeField] private Button _fireButton;
    [SerializeField] private Text _rocketCountText;
    [SerializeField] private Image ReloadImage;
    [SerializeField] private bool _isReloading = false;
    [SerializeField] private float _reloadTime = 2f; // Example reload time
    [SerializeField] private int rocketCount = 5; // Current rocket count
    [SerializeField] private MissileSO missileSO;
    [SerializeField] private Button _addMoreRocketButton;
    [SerializeField] private Button _switchButtonRocket;

    void Awake()
    {
        check = false;
        _switchButtonRocket.onClick.AddListener(OnSwitchButtonClicked);
        _switchButton.onClick.AddListener(OnSwitchButtonClicked);
        _fireButton.onClick.AddListener(OnFireButtonClicked);
        _addMoreRocketButton.onClick.AddListener(OnInStallLuna);
        rocketCount = missileSO.AmountRocket; // Initialize rocket count from MissileSO
        _reloadTime = missileSO.timeReload; // Initialize reload time from MissileSO
        _rocketCountText.text = rocketCount.ToString("D2"); // Initialize rocket count text
    }

    void OnEnable()
    {
        EventManager.Instance?.Subscribe<RocketEvent>(this);
    }


    void OnDisable()
    {
        _switchButtonRocket.onClick.RemoveListener(OnSwitchButtonClicked);
        _switchButton.onClick.RemoveListener(OnSwitchButtonClicked);
        _fireButton.onClick.RemoveListener(OnFireButtonClicked);
        _addMoreRocketButton.onClick.RemoveListener(OnInStallLuna);
        EventManager.Instance?.Unsubscribe<RocketEvent>(this);
    }



    void Update()
    {
        if (_isReloading)
        {
            _fireButton.interactable = false;
            ReloadImage.fillAmount += Time.deltaTime / _reloadTime; // Example reload time of 2 seconds
            if (ReloadImage.fillAmount >= 1f)
            {
                _isReloading = false;
                ReloadImage.fillAmount = 0f;
            }
        }
        else
        {
            _fireButton.interactable = true;
        }

    }

    private bool check = false;
    private void OnFireButtonClicked()
    {
        if (check)
            return;
        if (rocketCount >= 1)
            EventManager.Instance?.Publish(new RocketEvent(_isRocketOn, "Fire"));
        if (rocketCount <= 0)
        {
            // Debug.LogWarning("No rockets available to fire! ");
            _fireButton.gameObject.SetActive(false);
            check = true;
            _addMoreRocketButton.gameObject.SetActive(true);
            return;
        }
    }

    void OnDestroy()
    {
        _switchButton.onClick.RemoveListener(OnSwitchButtonClicked);
    }

    private void OnSwitchButtonClicked()
    {
        _isRocketOn = !_isRocketOn;
        UpdateRocketState();
    }

    private void UpdateRocketState()
    {
        _switchRectTransform.DOAnchorPosX(_isRocketOn ? _position_max_x : _position_min_x, 0.5f);
        _rocketImage.sprite = _isRocketOn ? _rocketOnSprite : _rocketOffSprite;
        _rocketIconImageOn.gameObject.SetActive(_isRocketOn);
        _rocketIconImageOff.gameObject.SetActive(!_isRocketOn);
        EventManager.Instance?.Publish(new RocketEvent(isRocketOn: _isRocketOn, state: "ChangeStateRocketAim"));
    }

    public void OnNotify(RocketEvent data)
    {

        if (data.State == "UpdateIndex")
        {
            rocketCount = data.RocketCount ?? 0;
            _rocketCountText.text = data.RocketCount.HasValue ? data.RocketCount.Value.ToString("D2") : "00";
            _reloadTime = data.TimerReload.HasValue ? data.TimerReload.Value : 2f;
            _isReloading = true;
        }
    }

    private void OnInStallLuna()
    {
        Luna.Unity.Playable.InstallFullGame();
        Debug.Log("InStallLuna called");
    }

}
