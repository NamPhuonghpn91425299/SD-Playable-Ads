using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ToggleRocketControl : MonoBehaviour
{
    [SerializeField] private ToggleGroup _toggleGroupRocket;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Image _iconRocketFoward;
    [SerializeField] private Text _bulletCountRocketForward;
    [SerializeField] private Image _iconRocketFollow;
    [SerializeField] private Text _bulletCountRocketFollow;
    [SerializeField] private Text _countBullet;
    [SerializeField] private int _numberRocketForward;
    [SerializeField] private int _numberRocketFollow;
    
    [Header("CountDown Rocket")]
    [SerializeField] private float timeCountDownRocket = 5f;
    [SerializeField] private Image rocketImage;
    [SerializeField] private Text timeCoundown_Text;
    [SerializeField] private GameObject btnGetMore;

    private void OnEnable()
    {
        EventManager.AddListener<int>(EventName.UpdateRocketFollowCount, OnFollowRocketBulletChange);
        EventManager.AddListener<int>(EventName.UpdateRocketForwardCount, OnForwardRocketBulletChange);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<int>(EventName.UpdateRocketFollowCount, OnFollowRocketBulletChange);
        EventManager.RemoveListener<int>(EventName.UpdateRocketForwardCount, OnForwardRocketBulletChange);
    }



    private void Start()
    {
        btnGetMore.SetActive(false); 
        _countBullet.text = _numberRocketForward.ToString();
        _bulletCountRocketForward.text = _numberRocketForward.ToString();
        _bulletCountRocketFollow.text = _numberRocketFollow.ToString();
        RocketController.Instance.bulletFollowRocket = _numberRocketFollow;
        RocketController.Instance.bulletForwardRocket = _numberRocketForward;
    }

    public void ToggleRocket()
    {
        if (_toggleGroupRocket.ActiveToggles().FirstOrDefault().gameObject.CompareTag("Follow"))
        {
            EventManager.Invoke(EventName.OnSwithToggleRocket, true);
            SetSelected(_iconRocketFollow, _bulletCountRocketFollow);
            SetDefault(_iconRocketFoward, _bulletCountRocketForward);
        }
        else
        {
            EventManager.Invoke(EventName.OnSwithToggleRocket, false);
            SetSelected(_iconRocketFoward, _bulletCountRocketForward);
            SetDefault(_iconRocketFollow, _bulletCountRocketFollow);
        }
        RocketController.Instance.bulletFollowRocket = _numberRocketFollow;
        RocketController.Instance.bulletForwardRocket = _numberRocketForward;
        RocketController.Instance.listBot = GameProcessBeachHead.Instance.GetListBot();
    }
    private void SetDefault(Image img, Text text)
    {
        img.color = _defaultColor;
        text.color = _defaultColor;
    }
    private void SetSelected(Image img, Text text)
    {
        img.color = _selectedColor;
        text.color = _selectedColor;
    }
    private void OnFollowRocketBulletChange(int count)
    {
        _numberRocketFollow = count;
        _bulletCountRocketFollow.text = count.ToString();
    }
    private void OnForwardRocketBulletChange(int count)
    {
        _numberRocketForward = count;
        SetCooldownOf(rocketImage);
        _bulletCountRocketForward.text = count.ToString();
        _countBullet.text = _numberRocketForward.ToString();
    }

    private void Update()
    {
        if(_numberRocketForward<=0)
        {            
            if(!btnGetMore.activeSelf)
                btnGetMore.SetActive(true);
            return;
        }
        
        CheckCooldownOf(rocketImage, timeCountDownRocket);
    }
    
    private void CheckCooldownOf(Image _image, float _cooldown)
    {
        if(_numberRocketForward <= 0)
            return;
        
        if (_image.fillAmount > 0)
        {
            _image.fillAmount -= 1 / _cooldown * Time.deltaTime;
            float secondsRemaining = _image.fillAmount * _cooldown;
            timeCoundown_Text.text = secondsRemaining.ToString("0.#") + " s";
        }
        else
            rocketImage.gameObject.SetActive(false);
    }
    
    private void SetCooldownOf(Image _image)
    {
        if (_image.fillAmount <= 0)
            _image.fillAmount = 1;
        if(!rocketImage.gameObject.activeSelf)
            rocketImage.gameObject.SetActive(true);
    }
}
