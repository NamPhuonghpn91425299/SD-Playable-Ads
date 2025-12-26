using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRotationMuzzle : ReloadableWeapons
{
    protected bool isShooting = false; // Trạng thái đang bắn
    protected bool canShoot = false;
    private bool isBarrelSpinning = false;
    [SerializeField] private Transform[] gunBarrels;
    public float currentRotationSpeed = 0f; // Tốc độ quay hiện tại của nòng súng

    protected override void AddAnimationClips()
    {
        base.AddAnimationClips();
        if (_animation != null && weaponInfo != null)
        {
            _animation.AddClip(weaponInfo.Fire, "Fire");
            _animation.AddClip(weaponInfo.Idle, "Idle");
            _animation.AddClip(weaponInfo._reloadAnimIn, "ReloadIn");
            _animation.AddClip(weaponInfo._reloadAnimOn, "ReloadOn");
            _animation.AddClip(weaponInfo._reloadAnimOut, "ReloadOut");
        }
    }

    protected override void LogicPlayGun()
    {
        if (!isShooting)
        {
            isShooting = true;
            if (shootingCoroutine == null)
                shootingCoroutine = StartCoroutine(StartShootingAfterDelay());
            
            if (!isBarrelSpinning)
            {
                currentRotationSpeed = 300;
                _audioSource.clip = weaponInfo.AudioStartBarrel;
                _audioSource.Play();
                isBarrelSpinning = true;
            }
        }

        if (canShoot && _timeSinceLastShoot >= fireRateDefault)
        {
            base.LogicPlayGun();
        }
        
        HandleGatlingGunRotation();
    }

    IEnumerator StartShootingAfterDelay()
    {
        yield return HelperCoroutine.GetWait(weaponInfo.WaitToShoot);
        canShoot = true;
        shootingCoroutine = null;
    }
    
    protected override void LogicStopGun()
    {
        print("LogicStopGun");
        UpOrDowTemperature(false);
        if (isShooting)
        {
            StopShootingSound();
            isShooting = false;
            canShoot = false;
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
            StopGunEffect(); // Dừng hiệu ứng nổ súng
        }
        if (isBarrelSpinning)
        {
            _audioSource.clip = weaponInfo.AudioEndBarrel;
            _audioSource.Play();
            isBarrelSpinning = false;
        }
        if (currentRotationSpeed <= 0)
            return;
        currentRotationSpeed -= (weaponInfo.MaxSpeedRotaBarrel / Mathf.Max(1, weaponInfo.WaitToShoot * 3)) * Time.deltaTime;
        RotateGunBarrels();
    }

    protected override void IsReloading()
    {
        base.IsReloading();
        if (currentRotationSpeed <= 0)
            return;
        currentRotationSpeed -= (weaponInfo.MaxSpeedRotaBarrel / Mathf.Max(1, weaponInfo.WaitToShoot * 3)) * Time.deltaTime;
        RotateGunBarrels();
    }

    public void HandleGatlingGunRotation()
    {
        if (currentRotationSpeed < weaponInfo.MaxSpeedRotaBarrel)
            currentRotationSpeed += (weaponInfo.MaxSpeedRotaBarrel / Mathf.Max(1, weaponInfo.WaitToShoot)) * Time.deltaTime;
        else
            currentRotationSpeed = weaponInfo.MaxSpeedRotaBarrel;
        RotateGunBarrels();
    }

    private void RotateGunBarrels()
    {
        foreach (var barrel in gunBarrels)
        {
            var currentRotation = barrel.localRotation.eulerAngles;
            var newRotationZ = currentRotation.z + currentRotationSpeed * Time.deltaTime;
            var newRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, newRotationZ);
            barrel.localRotation = newRotation;
        }
    }
}