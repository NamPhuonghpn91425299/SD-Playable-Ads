using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon81 : ReloadableWeapons
{
    [SerializeField] private Transform _muzzleTrans_2;
    private bool isLeftMuzzleNext;

    protected override void AddAnimationClips()
    {
        base.AddAnimationClips();
        if (_animation != null && weaponInfo != null)
        {
            _animation.AddClip(weaponInfo.Fire, "Fire_Left");
            _animation.AddClip(weaponInfo.Fire_Right, "Fire_Right");
            _animation.AddClip(weaponInfo.Idle, "Idle");
            _animation.AddClip(weaponInfo._reloadAnimIn, "ReloadIn");
            _animation.AddClip(weaponInfo._reloadAnimOn, "ReloadOn");
            _animation.AddClip(weaponInfo._reloadAnimOut, "ReloadOut");
        }
    }

    protected override void Shoot()
    {
        if (this == null || _cameraTransform == null) return;

        Vector3 forward= _cameraTransform.forward;
        //StartCoroutine(ShakeCamera(0.1f, 0.05f));

        forward += new Vector3(
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount),
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount),
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount)
        );

        if(isLeftMuzzleNext)
        {
            FireFromMuzzle(_muzzleTrans_1, forward);
            _animation.Play("Fire_Left");
            //Debug.Log("Fire from left muzzle");
        }
        else
        {
            FireFromMuzzle(_muzzleTrans_2, forward);
            _animation.Play("Fire_Right");
            //Debug.Log("Fire from right muzzle");
        }
        isLeftMuzzleNext = !isLeftMuzzleNext;
        
        _audioSource.clip = weaponInfo.audioClip;
        _audioSource.Play();

        //UICrosshairItem.Instance.Expand_Crosshair(15);

        PlayGunEffect();
    }
}
