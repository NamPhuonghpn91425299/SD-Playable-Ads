using System;
using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using Random = UnityEngine.Random;

public class ReloadableWeapons : WeaponBase
{
    [Header("Độ đỏ đầu nòng")]
    [SerializeField] protected ParticleSystem[] _vfxSmokeMuzzle; // Hiệu ứng khói từ nòng súng
    private bool _isSmokePlaying = false;
    [SerializeField] protected Transform _muzzleCenter; // Nơi bắn đạn từ nòng súng
    [SerializeField] protected Material _materialGun;
    [Tooltip("Độ đỏ tối đa của nòng")] protected float _maxGlowColor = 6f;
    [Tooltip("Nhiệt độ nòng")][SerializeField] protected float _temperatureCurrent;
    [Tooltip("Nhiệt độ để nòng đạt độ đỏ tối đa")][SerializeField] protected float _temperatureToColorMax = 2f;

    [Header("Shooting")]
    protected float fireRateDefault; // Tốc độ bắn mặc định
    protected float _timeSinceLastShoot = 0f; // Thời gian từ lần bắn cuối cùng
    [SerializeField] protected int _currentBulletCount; // Số lượng đạn hiện tại trong băng
    protected Coroutine shootingCoroutine;

    [Header("Reload")]
    private bool _isReloading = false; // Trạng thái đang nạp đạn
    private Coroutine _reloadCoroutine;
    //[SerializeField] private AudioClip _reloadFastAudio;

    [Header("Lằng nhằng")]
    [SerializeField] protected Animation _animation;
    [SerializeField] protected GameConstants.ProjecttilePlayer _bulletType;  // Loại đạn sẽ được bắn
    public GameConstants.ProjecttilePlayer BulletType => _bulletType;  // Loại đạn sẽ được bắn
    [Header("Muzzle")]
    [SerializeField] protected Transform _muzzleTrans_1;
    public int CurrentBulletCount => _currentBulletCount;
    public int DefaultBulletCount => weaponInfo.bulletCount;

    private void FixedUpdate()
    {
        _materialGun.SetVector("_Muzzle", new Vector4(_muzzleCenter.position.x, _muzzleCenter.position.y, _muzzleCenter.position.z, 0f));
    }

    protected override void Update()
    {
        base.Update();
        if (_isReloading)
        {
            IsReloading();
            UpOrDowTemperature(false);
            _materialGun.SetVector("_Muzzle", new Vector4(_muzzleCenter.position.x, _muzzleCenter.position.y, _muzzleCenter.position.z, 0f));
            return;
        }

        _timeSinceLastShoot += Time.deltaTime;

        if (_isHoldScreen)
            LogicPlayGun();
        else
            LogicStopGun();
    }

    public override void OnInit()
    {
        WeaponBase weaponBase = GameController.Instance.CurrentWeapon;
        if (weaponBase != null && weaponBase is ReloadableWeapons reloadableWeapon)
            _bulletType = reloadableWeapon._bulletType;
        fireRateDefault = weaponInfo.FireRate;
        base.OnInit();
        EventManager.Instance?.Publish(new GameDataChangedEvent(bullet: weaponInfo.bulletCount, bulletRemaning: weaponInfo.bulletCount, isInfinityBullet: weaponInfo.infiniteBullet));
        _temperatureCurrent = 0;
        _currentBulletCount = weaponInfo.bulletCount;
        _materialGun.SetVector("_Glow", Vector4.zero);
    }

    protected virtual void LogicStopGun()
    {
        UpOrDowTemperature(false);
        StopShootingSound();
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
        StopGunEffect(); // Dừng hiệu ứng nổ súng
    }

    protected virtual void LogicPlayGun()
    {
        _materialGun.SetVector("_Muzzle", new Vector4(_muzzleCenter.position.x, _muzzleCenter.position.y, _muzzleCenter.position.z, 0f));
        //UICrosshairItem.Instance.Narrow_Crosshair();
        if (_timeSinceLastShoot >= fireRateDefault)
        {
            if (_currentBulletCount <= 0 && !weaponInfo.infiniteBullet)
                OnReload_Corountine();
            else
            {
                UpOrDowTemperature(true);
                Shoot();
                _timeSinceLastShoot = 0f;

                if (!weaponInfo.infiniteBullet)
                {
                    _currentBulletCount--;
                    EventManager.Instance?.Publish(new GameDataChangedEvent(bulletRemaning: _currentBulletCount));
                    Debug.Log($"Current bullet count: {_currentBulletCount}");
                }
                PlayGunEffect(); // Kích hoạt hiệu ứng nổ súng
            }
        }
    }

    protected void UpOrDowTemperature(bool _isUp)
    {
        if (_isUp)
        {
            if (_isSmokePlaying)
            {
                foreach (ParticleSystem _vfx in _vfxSmokeMuzzle)
                    _vfx.Stop();

                _isSmokePlaying = false;
            }
            if (Math.Abs(_temperatureCurrent + _temperatureToColorMax) < .01f)
                return;

            if (_temperatureCurrent < _temperatureToColorMax)
                _temperatureCurrent += 6f * Time.deltaTime;
            else
                _temperatureCurrent = _temperatureToColorMax;
            _materialGun.SetVector("_Glow", new Vector4(0, .4f, _temperatureCurrent / _temperatureToColorMax * _maxGlowColor, 0));
        }
        else
        {
            if (_temperatureCurrent == 0)
                return;
            if (!_isSmokePlaying)
            {
                foreach (ParticleSystem _vfx in _vfxSmokeMuzzle)
                    _vfx.Play();

                _isSmokePlaying = true;
            }

            if (_temperatureCurrent >= 0)
                _temperatureCurrent -= .8f * Time.deltaTime;
            else
            {
                if (_isSmokePlaying)
                {
                    foreach (ParticleSystem _vfx in _vfxSmokeMuzzle)
                        _vfx.Stop();

                    _isSmokePlaying = false;
                }
                _temperatureCurrent = 0;
            }
            _materialGun.SetVector("_Glow", new Vector4(0, .4f, _temperatureCurrent / _temperatureToColorMax * _maxGlowColor, 0));
        }
    }

    protected virtual void Shoot()
    {
        if (this == null || _cameraTransform == null) return;

        Vector3 forward = _cameraTransform.forward;

        forward += new Vector3(
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount),
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount),
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount)
        );

        // Bắn từ nòng đầu tiên
        FireFromMuzzle(_muzzleTrans_1, forward);

        _animation.Play("Fire");
        _animation["Fire"].speed = 2.0f;
        _audioSource.clip = weaponInfo.audioClip;
        _audioSource.Play();

        //UICrosshairItem.Instance.Expand_Crosshair(15);

        PlayGunEffect();
    }
    private Dictionary<Transform, ITakeDamage> damageCache = new Dictionary<Transform, ITakeDamage>();// Cache để lưu trữ kết quả tìm kiếm ITakeDamage
    private ITakeDamage FindNearestTakeDamage(Transform start)
    {
        // Check cache trước
        if (damageCache.TryGetValue(start, out ITakeDamage cached))
            return cached;

        Transform current = start;
        while (current != null)
        {
            var damage = current.GetComponent<ITakeDamage>();
            if (damage != null)
            {
                damageCache[start] = damage; // Cache kết quả
                return damage;
            }
            current = current.parent;
        }

        damageCache[start] = null; // Cache null result
        return null;
    }

    protected virtual void FireFromMuzzle(Transform muzzle, Vector3 forward, bool canplayVFXHit = true)
    {
        var shotRotation = Quaternion.Euler(Random.insideUnitCircle * weaponInfo.inaccuracy) * forward;
        BulletTrail bullet = SimplePool<GameConstants.ProjecttilePlayer>.Spawn<BulletTrail>(_bulletType, muzzle.position, muzzle.rotation);
        Vector3 posGizmod = GizmodCaculatorPointShoot();
        bullet.Init((posGizmod - muzzle.position).normalized, posGizmod);

        // Kết hợp cả layer masks thông thường và weak point layer để raycast có thể hit cả weak point
        int combinedMask = CombinedLayerMask();
        if (weaponInfo.WeakPointLayerMask.value != 0)
        {
            combinedMask |= weaponInfo.WeakPointLayerMask.value;
        }

        var ray = new Ray(_cameraTransform.position, shotRotation);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, combinedMask))
        {
#if UNITY_EDITOR
            //Debug.Log($"Weapon {name}: Raycast hit {hit.collider.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
#endif

            // Kiểm tra xem có trúng weak point không (dù có nằm trong layerMasksAndEffects hay không)
            bool isWeakPointHit = IsWeakPoint(hit);
            int calculatedDamage = CalculateDamage(weaponInfo.damage, hit);
            DamageType damageType = isWeakPointHit ? DamageType.Weakness : DamageType.Normal;

#if UNITY_EDITOR
            //Debug.Log($"Weapon {name}: Final damage info - Damage: {calculatedDamage}, Type: {damageType}, Target: {hit.collider.name}, IsWeakPoint: {isWeakPointHit}", gameObject);
#endif

            DamageInfo damageInfo = new DamageInfo()
            {
                damageType = damageType,
                damage = calculatedDamage,
                //name = hit.collider.name,
            };

            // Tìm take damage controller từ parent (có thể là enemy body, không phải weak point object)
            var takeDamageController = FindNearestTakeDamage(hit.transform);

            if (takeDamageController != null)
            {
                takeDamageController.OnTakeDamage(damageInfo);
                EventManager.Instance?.Publish(new GameDataChangedEvent(hitEnemy: "HitEnemy"));
            }
            else
                EventManager.Instance?.Publish(new GameDataChangedEvent(hitEnemy: "NormalHit"));

            // Apply effect dựa trên object bị hit
            bool effectApplied = false;
            for (int i = 0; i < layerMasksAndEffects.Length; i++)
            {
                if (IsInLayerIndex(hit.collider.gameObject, i))
                {
                    //                    Debug.Log($"Trúng layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, hiệu ứng: {layerMasksAndEffects[i].effectType}+ {i}");
                    if (canplayVFXHit)
                        SimplePool<GameConstants.EffectType>.Spawn<Effect>(layerMasksAndEffects[i].effectType, hit.point, Quaternion.identity).OnInit();
                    effectApplied = true;
                    break;
                }
            }

            // Nếu không áp dụng effect theo layer, có thể áp dụng effect đặc biệt cho weak point nếu hit trúng
            if (!effectApplied && isWeakPointHit && canplayVFXHit)
            {
                // Có thể áp dụng effect đặc biệt cho weak point hit ở đây nếu cần
            }

            hit.collider.gameObject.SendMessage("OnHit", SendMessageOptions.DontRequireReceiver);
        }
#if UNITY_EDITOR
        else
        {
            //            Debug.Log("Không trúng target nào trong các layer đã định.");
        }
#endif
    }

    #region Reload
    public void OnReload_Corountine()
    {
        if (_isReloading || _currentBulletCount >= weaponInfo.bulletCount)
            return;

        if (_reloadCoroutine != null)
            StopCoroutine(_reloadCoroutine);

        _reloadCoroutine = StartCoroutine(IEReload());

        _isReloading = true;
        StopGunEffect();
        //UICrosshairItem.Instance.ResetCorosshair();
    }

    protected virtual void IsReloading()
    {
        
    }
    // public void OnReloadFast(int _PlusAmount)
    // {
    //     if (_reloadCoroutine != null)
    //         StopCoroutine(_reloadCoroutine);
    //     
    //     _isReloading = false;
    //     _audioSource.PlayOneShot(_reloadFastAudio);
    //     _animation.Play("ReloadOut");
    //     _currentBulletCount = weaponInfo.bulletCount + _PlusAmount;
    //    // UICrosshairItem.Instance.ResetCorosshair();
    //     EventManager.Invoke(EventName.UpdateBulletCount, _currentBulletCount);
    // }

    private IEnumerator IEReload()
    {
        // UIManager.Instance.GetUI<Canvas_GamePlay>().ActiveReloadFast();

        StopShootingSound();
        float reloadTime = weaponInfo.reloadTime;
        EventManager.Instance?.Publish(new GameDataChangedEvent(reloadTime: reloadTime));
        //Debug.Log("Reloading...");
        _audioSource.PlayOneShot(weaponInfo.AudioReloadIn);

        _animation.Play("ReloadIn");
        yield return HelperCoroutine.GetWait(reloadTime / 3);

        _animation.Play("ReloadOn");
        yield return HelperCoroutine.GetWait(reloadTime / 3);

        _audioSource.PlayOneShot(weaponInfo.AudioReloadOut);
        _animation.Play("ReloadOut");
        yield return HelperCoroutine.GetWait(reloadTime / 3);
        _currentBulletCount = weaponInfo.bulletCount;

        _isReloading = false;
        EventManager.Instance?.Publish(new GameDataChangedEvent(bulletRemaning: _currentBulletCount));
    }
    #endregion

    public override void OnNotify(ChangeProjectileGunEvent data)
    {
        base.OnNotify(data);
        _bulletType = data.typeProjectile;
        fireRateDefault = data.fireRate;
    }
}
