using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using static ParameterManagers;
using UnityEngine.EventSystems;

public class WeaponController : MonoBehaviour
{
    public static WeaponController instance;
    [SerializeField] public WeaponInfo weaponInfo;
    [SerializeField] private BulletAndEffect bulletAndEffect;
    [SerializeField] private LayerMask botLayerMask;
    [SerializeField] private LayerMask botTankLayerMask;
    [SerializeField] private LayerMask rewardLayerMask;
    [SerializeField] private Transform _muzzleTrans;
    [SerializeField] private Transform _muzzleTrans2;
    [SerializeField] public Transform[] Gunbarrel; // Nòng súng xoay (dùng cho súng 6 nòng)
    [SerializeField] private Animation _animation;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private ParticleSystem[] _fireEffect;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private GameObject _effect;
    [SerializeField] private bool _isShowCard;
    [SerializeField] private bool shootBasedOnGunDirection = false; // Chế độ bắn: true = bắn theo hướng súng, false = bắn theo hướng camera
    [SerializeField] private bool isDoubleMuzzle = false;
    [SerializeField] private Transform shakeCam; // Biến để tham chiếu đến MainCamera
    [SerializeField] private float shakeCamMin;
    [SerializeField] private float shakeCamMax;
    [SerializeField] private bool IsShowLunaEndGame;

    private Transform _cameraTransform;
    private Camera _camera;
    private float _timeSinceLastShoot = 0f; // Thời gian từ lần bắn cuối cùng
    private int _currentBulletCount; // Số lượng đạn hiện tại trong băng
    public int CurrentBulletCount => _currentBulletCount;
    private bool _isReloading = false; // Trạng thái đang nạp đạn
    private float currentRotationSpeed = 0f; // Tốc độ quay hiện tại của nòng súng
    private bool isShooting = false; // Trạng thái đang bắn
    private bool canShoot = false; // Trạng thái có thể bắn
    private bool isBarrelSpinning = false; // Trạng thái nòng súng đang quay
    private Coroutine shootingCoroutine;

    private void Awake()
    {
        instance = this;
        _bullet = bulletAndEffect._bulletPrefab[0];
        _camera = Camera.main;
        _cameraTransform = _camera.transform;
        _currentBulletCount = weaponInfo.bulletCount; // Khởi tạo số lượng đạn
        //Debug.Log("Initial bullet count: " + _currentBulletCount);
        EventManager.Invoke(EventName.UpdateBulletCount, _currentBulletCount); // Gửi thông báo về số lượng đạn ban đầu
        AssignAnimationClips();
    }

    private void OnEnable()
    {
        EventManager.AddListener<bool>(EventName.OnShowLunaEndGame, OnShowLunaEndGame);
        EventManager.AddListener<bool>(EventName.OnChangeFireRate, OnChangeFireRate);
    }
    private void OnDisable()
    {
        _bullet = bulletAndEffect._bulletPrefab[0];
        EventManager.RemoveListener<bool>(EventName.OnShowLunaEndGame, OnShowLunaEndGame);
        EventManager.RemoveListener<bool>(EventName.OnChangeFireRate, OnChangeFireRate);
    }

    private void OnShowLunaEndGame(bool IsShow)
    {
        IsShowLunaEndGame = IsShow;
    }

    public bool IsReloadFull()
    {
        return _currentBulletCount >= weaponInfo.bulletCount;
    }

    private void Update()
    {

            HandleGatlingGunRotation();
            if (!UIEndGame.Instance.IsShowEndGame)
            {
                OnShooting();
            }
            if (UIEndGame.Instance.IsShowEndGame )
            {
                isShooting = false;
                StopGunEffect();
            }




        // Thêm phím tắt để chuyển đổi chế độ bắn
        if (Input.GetKeyDown(KeyCode.T)) // Ví dụ: nhấn phím T để chuyển đổi
        {
            shootBasedOnGunDirection = !shootBasedOnGunDirection;
            Debug.Log("Shoot based on gun direction: " + shootBasedOnGunDirection);
        }
    }

    private void AssignAnimationClips()
    {
        if (_animation != null && weaponInfo != null)
        {
            _animation.AddClip(weaponInfo.Fire, "Fire");
            _animation.AddClip(weaponInfo.Idle, "Idle");
            _animation.AddClip(weaponInfo._reloadAnimIn, "ReloadIn");
            _animation.AddClip(weaponInfo._reloadAnimOn, "ReloadOn");
            _animation.AddClip(weaponInfo._reloadAnimOut, "ReloadOut");
        }
    }

    private void OnShooting()
    {
        if (IsIngameGUI || _isReloading)
            return;

        _timeSinceLastShoot += Time.deltaTime;
        UICrosshairItem.Instance.Narrow_Crosshair();

        if (Input.GetMouseButton(0))
        {
            if (!isShooting)
            {
                isShooting = true;
                if (shootingCoroutine == null)
                {
                    shootingCoroutine = StartCoroutine(StartShootingAfterDelay());
                }
                if (!isBarrelSpinning && !EventSystem.current.IsPointerOverGameObject())
                {
                    _audioSource.clip = weaponInfo.AudioStartBarrel;
                    _audioSource.Play();
                    isBarrelSpinning = true;
                }
            }

            if (canShoot && _timeSinceLastShoot >= weaponInfo.FireRate)
            {
                if (_currentBulletCount <= 0 && !weaponInfo.infiniteBullet)
                {
                    OnReload();
                }
                else
                {
                    Shoot();
                    _timeSinceLastShoot = 0f;

                    if (!weaponInfo.infiniteBullet)
                    {
                        _currentBulletCount--;
                        //Debug.Log("Bullet fired. Remaining bullets: " + _currentBulletCount);
                        EventManager.Invoke(EventName.UpdateBulletCount, _currentBulletCount);
                    }
                    PlayGunEffect(); // Kích hoạt hiệu ứng nổ súng
                }
            }
        }
        else
        {
            if (isShooting)
            {
                StopShootingSound();
                isShooting = false;
                canShoot = false; // Reset canShoot when stopping shooting
                if (shootingCoroutine != null)
                {
                    StopCoroutine(shootingCoroutine);
                    shootingCoroutine = null;
                }
                if (isBarrelSpinning)
                {
                    _audioSource.clip = weaponInfo.AudioEndBarrel;
                    _audioSource.Play();
                    isBarrelSpinning = false;
                }
                StopGunEffect(); // Dừng hiệu ứng nổ súng
            }
        }
    }
    public void OnReload()
    {
        StartCoroutine(Reload());
        StopGunEffect();
    }
    public int GetCurrentAmmo()
    {
        return _currentBulletCount;
    }
    private void HandleGatlingGunRotation()
    {
        if (weaponInfo.isGatlingGun)
        {
            if (isShooting)
            {
                currentRotationSpeed += (weaponInfo.MaxSpeedRotaBarrel / Mathf.Max(1, weaponInfo.WaitToShoot)) * Time.deltaTime;
                if (currentRotationSpeed >= weaponInfo.MaxSpeedRotaBarrel)
                {
                    currentRotationSpeed = weaponInfo.MaxSpeedRotaBarrel;
                }
            }
            else if (currentRotationSpeed > weaponInfo.MinSpeedRotaBarrel)
            {
                currentRotationSpeed -= (weaponInfo.MaxSpeedRotaBarrel / weaponInfo.TimeMinSpeed) * Time.deltaTime;
                if (currentRotationSpeed <= weaponInfo.MinSpeedRotaBarrel)
                {
                    currentRotationSpeed = weaponInfo.MinSpeedRotaBarrel;
                }
            }

            RotateGunbarrels();
        }
    }

    private IEnumerator StartShootingAfterDelay()
    {
        yield return new WaitForSeconds(weaponInfo.WaitToShoot);
        canShoot = true;
        shootingCoroutine = null;
    }

    private void RotateGunbarrels()
    {
        if (IsShowLunaEndGame) return;
        foreach (var barrel in Gunbarrel)
        {
            var currentRotation = barrel.localRotation.eulerAngles;
            var newRotationZ = currentRotation.z + currentRotationSpeed * Time.deltaTime;
            // Debug.Log(currentRotation);
            // Debug.Log(newRotationZ);
            // Debug.Log(currentRotationSpeed);
            var newRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, newRotationZ);
                
            
            barrel.localRotation = newRotation;
        }
    }

    private void Shoot()
    {
        if (this == null || _cameraTransform == null) return;

        Vector3 forward;
        if (shootBasedOnGunDirection)
        {
            forward = _cameraTransform.forward;
            StartCoroutine(ShakeCamera(0.1f, 0.05f));
        }
        else
        {
            forward = _cameraTransform.forward;
            var targetPoint = FindPointedTransform();
            if (targetPoint != null)
            {
                if (Vector3.SqrMagnitude(targetPoint.position - _cameraTransform.position) > 0)
                    forward = (targetPoint.position - _cameraTransform.position).normalized;
            }
        }

        forward += new Vector3(
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount),
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount),
            Random.Range(-weaponInfo.recoilAmount, weaponInfo.recoilAmount)
        );

        // Bắn từ nòng đầu tiên
        FireFromMuzzle(_muzzleTrans, forward);

        // Nếu isDoubleMuzzle là true, bắn thêm từ nòng thứ hai
        if (isDoubleMuzzle)
        {
            FireFromMuzzle(_muzzleTrans2, forward);
        }

        _animation.Play("Fire");
        _animation["Fire"].speed = 2.0f;
        _audioSource.clip = weaponInfo.audioClip;
        _audioSource.Play();

        UICrosshairItem.Instance.Expand_Crosshair(15);

        PlayGunEffect();
    }

    private bool IsInBotLayer(GameObject obj)
    {
        return ((1 << obj.layer) & botLayerMask) != 0;
    }

    private bool IsInBotTankLayer(GameObject obj)
    {
        return ((1 << obj.layer) & botTankLayerMask) != 0;
    }

    private bool IsInRewardLayer(GameObject obj)
    {
        return ((1 << obj.layer) & rewardLayerMask) != 0;
    }

    private void FireFromMuzzle(Transform muzzle, Vector3 forward)
    {
        var shotRotation = Quaternion.Euler(Random.insideUnitCircle * weaponInfo.inaccuracy) * forward;
        var ray = new Ray(muzzle.position, shotRotation);

        _bullet.SetActive(true);

        var bullet = ObjectPool.Instance.PopFromPool(_bullet, instantiateIfNone: true);
        bullet.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
        bullet.GetComponent<BulletTrail>().Init(ray.direction);

        bool CheckRayCast = Physics.Raycast(ray, out var hit, Mathf.Infinity, botLayerMask| rewardLayerMask| botTankLayerMask);
        if (CheckRayCast)
        {
            var damageType = hit.collider.CompareTag("WeakPoint")? DamageType.Weekness:DamageType.Normal;
            var damageInfo = new DamageInfo() 
            {
                damageType = damageType,
                damage = weaponInfo.damage,
                name = hit.collider.name,
            };
            if (IsInBotLayer(hit.collider.gameObject))
            {
                var takeDamageController = hit.transform.gameObject.GetComponent<ITakeDamage>();
                if (takeDamageController == null)
                {
                    takeDamageController = hit.transform.root.gameObject.GetComponent<ITakeDamage>();
                }
                if (takeDamageController != null) takeDamageController.TakeDamage(damageInfo);
                _effect = bulletAndEffect.EffectBullet[0];

            }
            else if (IsInRewardLayer(hit.collider.gameObject))
            {
                var rewardController = hit.transform.gameObject.GetComponent<IReward>();
                if (rewardController == null)
                {
                    rewardController = hit.transform.root.gameObject.GetComponent<IReward>();
                }
                if (rewardController != null) rewardController.TakeCollect(weaponInfo.damage);
                PlayRandomAttackSound();
                _effect = bulletAndEffect.EffectBullet[1];
            } 
            else if(IsInBotTankLayer(hit.collider.gameObject))
            {
                var takeDamageController1 = hit.transform.gameObject.GetComponent<ITakeDamage>();
                if (takeDamageController1 == null)
                {
                    takeDamageController1 = hit.transform.root.gameObject.GetComponent<ITakeDamage>();
                }
                if (takeDamageController1 != null) takeDamageController1.TakeDamage(damageInfo);
                //Debug.Log(damageType.ToString() + " " + weaponInfo.damage);
                PlayRandomAttackSound();
                _effect = bulletAndEffect.EffectBullet[1];
            }

            // Tạo hiệu ứng va chạm
            var effect = ObjectPool.Instance.PopFromPool(_effect, instantiateIfNone: true);
            effect.GetComponent<Effect>().Init(hit.point);
        }
        EventManager.Invoke(EventName.OnCheckBotTakeDamage, CheckRayCast);
    }

    
    void PlayRandomAttackSound()
    {
        AudioClip clip = AudioManager.Instance.GetAudioAttackClip();
        if (clip != null)
        {
            //_audioSource.clip = clip;
            _audioSource.PlayOneShot(clip);
        }
    }
    private void OnChangeFireRate(bool IsChange)
    {
        if (IsChange)
        {
            _bullet = bulletAndEffect._bulletPrefab[1];
        }
        else
        {
            _bullet = bulletAndEffect._bulletPrefab[0];
        }
    }


    private IEnumerator Reload()
    {
        StopShootingSound();
        _isReloading = true;
        EventManager.Invoke(EventName.OnReloading, _isReloading);
        //Debug.Log("Reloading...");
        _audioSource.PlayOneShot(weaponInfo.AudioReloadIn);

        _animation.Play("ReloadIn");
        yield return new WaitForSeconds(weaponInfo.reloadTime / 3);

        _animation.Play("ReloadOn");
        yield return new WaitForSeconds(weaponInfo.reloadTime / 3);

        _audioSource.PlayOneShot(weaponInfo.AudioReloadOut);
        _animation.Play("ReloadOut");
        yield return new WaitForSeconds(weaponInfo.reloadTime / 3);
        _currentBulletCount = weaponInfo.bulletCount;
        _isReloading = false;
        //Debug.Log("Reloaded. Current bullet count: " + _currentBulletCount);
        EventManager.Invoke(EventName.UpdateBulletCount, _currentBulletCount);

        // Phát âm thanh khi súng ngừng xoay nếu đang nạp đạn
        if (isBarrelSpinning)
        {
            _audioSource.clip = weaponInfo.AudioEndBarrel;
            _audioSource.Play();
            isBarrelSpinning = false;
        }
    }

    private IEnumerator DecreaseRotationSpeed()
    {
        while (currentRotationSpeed > weaponInfo.MinSpeedRotaBarrel)
        {
            currentRotationSpeed -= (weaponInfo.MaxSpeedRotaBarrel / weaponInfo.TimeMinSpeed) * Time.deltaTime;
            if (currentRotationSpeed < weaponInfo.MinSpeedRotaBarrel)
            {
                currentRotationSpeed = weaponInfo.MinSpeedRotaBarrel;
            }
            yield return null;
        }
    }

    private Transform FindPointedTransform()
    {
        var minCrossHairDistance = float.MaxValue;
        Transform pointedTransform = null;

        var bots = BotManager.Instance.BotNetworks;
        foreach (var bot in bots.Where(bot => bot != null && !bot.IsDead))
        {
            CheckFireAssistCheckPos(bot.FireAssistCheckPos, ref minCrossHairDistance, ref pointedTransform);
        }

        var rewards = RewardManager.Instance.RewardNetworks;
        foreach (var reward in rewards.Where(reward => reward != null && !reward.IsCollected))
        {
            CheckFireAssistCheckPos(reward.FireAssistCheckPos, ref minCrossHairDistance, ref pointedTransform);
        }

        return pointedTransform;
    }

    private void CheckFireAssistCheckPos(List<Transform> fireAssistCheckPos, ref float minCrossHairDistance, ref Transform pointedTransform)
    {
        foreach (var checkPoint in fireAssistCheckPos)
        {
            var checkPosition = checkPoint.position;

            if (!SatisfyAutoFireCondition(checkPosition, out var crossHairDistance) ||
                crossHairDistance > minCrossHairDistance) continue;

            minCrossHairDistance = crossHairDistance;
            pointedTransform = checkPoint;
        }
    }

    [SerializeField] private float radius = 33f;
    private const float ReferenceWidth = 887;

    private bool SatisfyAutoFireCondition(Vector3 target, out float distance)
    {
        var viewPosition = _camera.WorldToScreenPoint(target);
        if (viewPosition.z < 0)
        {
            distance = float.MaxValue;
            return false;
        }
        viewPosition.x -= Screen.width / 2f;
        viewPosition.y -= Screen.height / 2f;

        viewPosition *= ReferenceWidth / Screen.width;

        distance = Mathf.Sqrt(viewPosition.x * viewPosition.x + viewPosition.y * viewPosition.y);
        return distance < radius && IsClearShot(_cameraTransform.position, target);
    }

    private bool IsClearShot(Vector3 origin, Vector3 target)
    {
        var distance = Vector3.Distance(origin, target);
        var ray = new Ray(origin, target - origin);
        return !Physics.Raycast(ray, out _, distance, botLayerMask | rewardLayerMask | botTankLayerMask);
    }

    // Thêm phương thức dừng âm thanh bắn
  
    private void StopShootingSound()
    {
        if (_audioSource.isPlaying && _audioSource.clip == weaponInfo.audioClip)
        {
            _audioSource.Stop();
        }
    }

    // Thêm phương thức nhận AnimationEvent
    public void AnimationAudioEvent()
    {
        // Thực hiện hành động khi sự kiện AnimationAudioEvent được gọi
        //Debug.Log("AnimationAudioEvent called");
    }

    // Thêm hàm rung lắc camera
    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Quaternion originalRot = shakeCam.localRotation;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(shakeCamMin, shakeCamMax) * magnitude;
            float y = Random.Range(shakeCamMin, shakeCamMax) * magnitude;

            shakeCam.localRotation = originalRot * Quaternion.Euler(x, y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        shakeCam.localRotation = originalRot;
        EventManager.Invoke(EventName.OnCheckShakeCam, shakeCam.localEulerAngles);
    }


    private void PlayGunEffect()
    {
        foreach (ParticleSystem fireEffect in _fireEffect)
        {
            if (fireEffect != null && !fireEffect.isPlaying)
            {
                fireEffect.Play();
            }    
        }
    }

    public void StopEffect()
    {
        StopGunEffect();
    }
    private void StopGunEffect()
    {
        foreach (ParticleSystem fireEffect in _fireEffect)
        {
            if (fireEffect != null && fireEffect.isPlaying)
            {
                fireEffect.Stop();
            }
        }
    }
}
