using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RealoadWeapon : MonoBehaviour
{
    [SerializeField] private Text _currentBullet;
    //[SerializeField] private int totalBullet;
    [SerializeField] private Text _totalBullet;
    [SerializeField] private Button reloadBullet;
    [SerializeField] private Image iconWeapon;
    [SerializeField] private Image iconreload;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private GameObject CrossHair;
    [SerializeField] private GameObject defaulAmmo;
    [SerializeField] private GameObject infiniteAmmo;
    [SerializeField] private float rotationSpeed = 250f;
    private Quaternion rotation;
    private Coroutine reloadCoroutine;
    [Header("WeaponCircleReloading")]
    [SerializeField] private CanvasGroup circleReload;
    [SerializeField] private Text _reloadTime;
    [SerializeField] private Image _reloadIcon;
    WeaponController weaponController => WeaponController.instance;

    private int bulletTotal => weaponController.weaponInfo.bulletCount;

    private void Awake()
    {
        
    }
    private void Start()
    {
        reloadBullet.onClick.AddListener(ReloadingWeapon);
        rotation = transform.rotation;
        _totalBullet.text = bulletTotal.ToString();
        _currentBullet.text = bulletTotal.ToString();
        _reloadTime.text = $"{weaponController.weaponInfo.reloadTime:F1}";
        EventManager.AddListener<int>(EventName.UpdateBulletCount, UpdateBulletCountUI);
    }
    private void OnEnable()
    {
        EventManager.AddListener<bool>(EventName.OnReloading, StartReloadUI);
        EventManager.AddListener<bool>(EventName.OnChangeWeapon, OnChangeMachineGun);

    }


    private void OnDisable()
    {
        EventManager.RemoveListener<int>(EventName.UpdateBulletCount, UpdateBulletCountUI);
        EventManager.RemoveListener<bool>(EventName.OnReloading, StartReloadUI);
        EventManager.RemoveListener<bool>(EventName.OnChangeWeapon, OnChangeMachineGun);
    }
    private void OnChangeMachineGun(bool isChangeWeapon)
    {
        if (isChangeWeapon)
        {
            if(reloadCoroutine!=null)
                StopCoroutine(reloadCoroutine);
            
            iconWeapon.enabled = true;
            iconreload.enabled = false;
            circleReload.alpha = 0f;
            _bullet.SetActive(false);
            _currentBullet.enabled = false;
            _totalBullet.enabled = false;
            defaulAmmo.SetActive(false);
            infiniteAmmo.SetActive(true);
        }
        else
        {
            infiniteAmmo.SetActive(false);
            defaulAmmo.SetActive(true);
        }
    }
    private void StartReloadUI(bool isReload)
    {
        if (isReload)
        {
            reloadCoroutine = StartCoroutine(SetUIReaload());
        }
        else
        {
            StopCoroutine(reloadCoroutine);
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            ReloadingWeapon();
        }
        Rotate();
    }
    // Hàm nhận số lượng đạn và cập nhật lên UI
    private void UpdateBulletCountUI(int bulletCount)
    {
        _currentBullet.text = bulletCount.ToString(); // Hiển thị số lượng đạn lên Text UI

    }
    private void ReloadingWeapon()
    {
        if (weaponController.IsReloadFull()) return;
            weaponController.OnReload();
    }

    public IEnumerator SetUIReaload()
    {
        
        reloadBullet.interactable = false;
        //EventManager.Invoke(EventName.OnReloading);
        iconWeapon.enabled = false;
        iconreload.enabled = true;
        _bullet.SetActive(false);
        CrossHair.SetActive(false);
        circleReload.alpha = 1f;
        float elapsedTime = weaponController.weaponInfo.reloadTime;
        while (elapsedTime > 0f)
        {
            elapsedTime = Mathf.Max(0f, elapsedTime - Time.deltaTime);
            _reloadTime.text = $"{elapsedTime:F1}";
            yield return null;
        }
        //yield return new WaitForSeconds(weaponController.weaponInfo.reloadTime);
        UiReloadDone();
    }

    private void Rotate()
    {
        if (iconreload.enabled == true)
        {
          iconreload.transform.Rotate(Vector3.forward, -rotationSpeed * Time.deltaTime);
          _reloadIcon.transform.Rotate(Vector3.forward, -rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = rotation;
        }
    }

    private void UiReloadDone()
    {
        reloadBullet.interactable = true;
        _bullet.SetActive(true);
        CrossHair.SetActive(true);
        circleReload.alpha = 0f;
        iconWeapon.enabled = true;
        iconreload.enabled = false;
    }
    
}
