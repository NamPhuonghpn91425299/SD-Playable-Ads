using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEvent : MonoBehaviour
{
    public static WeaponEvent Instance;
    public  WeaponInfo[] weaponInfo;
    public  Transform WeaponDefault;
    public Transform WeaponChange;
    public float[] DefaultFireRate;
    public bool IsChangeBullet;

    public float PosWeaponDefaultEnd;
    public float PosWeaponChangeEnd;
    public float transitionTime = 1f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < weaponInfo.Length; i++)
        {
            DefaultFireRate[i] = weaponInfo[i].FireRate;
        }

    }

    private void OnEnable()
    {
        EventManager.AddListener<float>(EventName.OnUpgradeFireRate, OnUpgradeFireRate);
        EventManager.AddListener<bool>(EventName.OnChangeMachineGun, OnChangeMachineGun);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<float>(EventName.OnUpgradeFireRate, OnUpgradeFireRate);
        EventManager.RemoveListener<bool>(EventName.OnChangeMachineGun, OnChangeMachineGun);
        for (int i = 0; i < weaponInfo.Length; i++)
        {
            weaponInfo[i].FireRate = DefaultFireRate[i];
        }
    }


    private void OnChangeMachineGun(bool IsChange)
    {
        if (IsChange)
        {
            StartCoroutine(OnChangeWeapon());
        }
        else
        {
            WeaponDefault.gameObject.SetActive(true);
            WeaponChange.gameObject.SetActive(false);
        }

    }

    public void OnUpgradeFireRate(float Value)
    {
        for (int i = 0; i < weaponInfo.Length; i++)
        {
            weaponInfo[i].FireRate = Value;
        }

        IsChangeBullet = true;
        EventManager.Invoke(EventName.OnChangeFireRate, true);
    }

    private IEnumerator OnChangeWeapon()
    {
        // Di chuyển WeaponDefault từ vị trí A đến B theo trục Z
        float elapsedTime = 0f;
        Vector3 startPosition = WeaponDefault.localPosition;
        Vector3 endPosition = new Vector3(startPosition.x, startPosition.y, PosWeaponDefaultEnd);

        while (elapsedTime < transitionTime)
        {
            WeaponDefault.localPosition = Vector3.Lerp(startPosition, endPosition, elapsedTime / transitionTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        WeaponDefault.localPosition = endPosition;

        // Ẩn WeaponDefault
        WeaponDefault.gameObject.SetActive(false);

        // Hiển thị WeaponChange
        WeaponChange.gameObject.SetActive(true);
        if (IsChangeBullet)
        {
            EventManager.Invoke(EventName.OnChangeFireRate, true);
        }
        // Di chuyển WeaponChange từ vị trí M đến N theo trục Y
        float elapsedTime2 = 0f;
        Vector3 startPosition2 = WeaponChange.localPosition;
        Vector3 endPosition2 = new Vector3(startPosition2.x, PosWeaponChangeEnd, startPosition2.z);

        while (elapsedTime2 < transitionTime)
        {
            WeaponChange.localPosition = Vector3.Lerp(startPosition2, endPosition2, elapsedTime2 / transitionTime);
            elapsedTime2 += Time.deltaTime;
            yield return null;
        }
        WeaponChange.localPosition = endPosition2;

    }
}
