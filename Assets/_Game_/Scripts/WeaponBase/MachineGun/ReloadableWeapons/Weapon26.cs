using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using static GameConstants;

public class Weapon26 : ReloadableWeapons
{
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
        _materialGun.SetVector("_Muzzle", new Vector4(_muzzleCenter.position.x,_muzzleCenter.position.y, _muzzleCenter.position.z, 0f));
       // UICrosshairItem.Instance.Narrow_Crosshair();
        if (_timeSinceLastShoot >= fireRateDefault)
        {
            if (_currentBulletCount <= 0 && !weaponInfo.infiniteBullet)
            {
                OnReload_Corountine();
            }
            else
            {
                UpOrDowTemperature(true);
                Shoot();
                _timeSinceLastShoot = 0f;

                if (!weaponInfo.infiniteBullet)
                {
                    _currentBulletCount--;
                    EventManager.Instance?.Publish(new GameDataChangedEvent(bulletRemaning: _currentBulletCount));
                }
                PlayGunEffect(); // Kích hoạt hiệu ứng nổ súng
            }
        }
    }
}