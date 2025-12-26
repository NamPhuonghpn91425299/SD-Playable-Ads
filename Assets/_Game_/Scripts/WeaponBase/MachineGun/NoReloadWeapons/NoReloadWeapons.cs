using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

public class NoReloadWeapons : WeaponBase
{
    [Header("Lằng nhằng")] 
    [SerializeField] protected Animation _animation;
    [SerializeField] protected GameConstants.ProjecttilePlayer _bulletType; // Loại đạn sẽ được bắn

    [Header("Muzzle")] 
    [SerializeField] protected Transform _muzzleTrans_1;

    public override void OnInit()
    {
        base.OnInit();
        EventManager.Instance?.Publish(new GameDataChangedEvent(isInfinityBullet: weaponInfo.infiniteBullet));
    }

    protected override void Update()
    {
        base.Update();

        if (_isHoldScreen)
            LogicPlayGun();
        else
            LogicStopGun();
    }
    
    protected virtual void LogicStopGun()
    {
        StopShootingSound();
        StopGunEffect(); // Dừng hiệu ứng nổ súng
    }

    protected virtual void LogicPlayGun()
    {
        PlayGunEffect();
    }

    protected virtual void Shoot()
    {
        
    }

    protected virtual void FireFromMuzzle(Transform muzzle, Vector3 forward)
    {
        var shotRotation = Quaternion.Euler(Random.insideUnitCircle * weaponInfo.inaccuracy) * forward;
        var ray = new Ray(_cameraTransform.position, shotRotation);
        // BulletTrail bullet = SimplePool.Spawn<BulletTrail>(_bulletType, muzzle.position, muzzle.rotation);
        // Vector3 posGizmod = GizmodCaculatorPointShoot();
        // bullet.Init((posGizmod - muzzle.position).normalized, posGizmod);
    }
}