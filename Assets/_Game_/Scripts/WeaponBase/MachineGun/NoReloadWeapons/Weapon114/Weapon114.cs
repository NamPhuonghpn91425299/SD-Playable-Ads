using System;
using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon114 : NoReloadWeapons
{
    [SerializeField] private Transform _muzzleTrans_2;

    [Header("Plasma Body Effect")]
    public float strengSnakeGun;
    public float speedMoveBody;
    public Vector3 _bodyMoveEnd;
    private Coroutine _corutineMoveBodyToEnd;
    private Coroutine _corutineMoveBodyToStart;


    [Header("Shooting")]
    public GameObject LaserPrefab;
    public bool IsShooting;
    public bool CanShoot;
    public bool CanPlayAnim;
    private Transform[] laser = new Transform[2]; // 0: left, 1: right, 
    private float _timeSinceLastShoot = 0f; // Thời gian từ lần bắn cuối cùng
    public Transform vfxLaserShoot;
    public float magnitudeShoot;

    public override void OnInit()
    {
        base.OnInit();
        laser[0] = Instantiate(LaserPrefab, Vector3.zero, Quaternion.identity).transform;
        laser[1] = Instantiate(LaserPrefab, Vector3.zero, Quaternion.identity).transform;
        foreach (Transform VARIABLE in laser)
            VARIABLE.localScale = Vector3.zero;
    }

    protected override void Update()
    {
        base.Update();
        UpdateLaser(IsShooting);
    }

    protected override void LogicPlayGun()
    {
        base.LogicPlayGun();
        if (!CanShoot)
        {
            if (CanPlayAnim)
                return;
            if (_corutineMoveBodyToEnd != null)
                StopCoroutine(_corutineMoveBodyToEnd);
            _corutineMoveBodyToEnd = StartCoroutine(MoveToPoint(_bodyMoveEnd, speedMoveBody, true));
            _audioSource.clip = weaponInfo.AudioStartBarrel;
            _audioSource.Play();
            CanPlayAnim = true;
        }
        else
        {
            Shoot();
        }
    }

    protected override void Shoot()
    {
        base.Shoot();
        if (_audioSource.clip != weaponInfo.audioClip || !_audioSource.isPlaying)
        {
            _audioSource.clip = weaponInfo.audioClip;
            _audioSource.Play();
        }
        _timeSinceLastShoot += Time.deltaTime;
        if (_timeSinceLastShoot >= weaponInfo.FireRate)
        {
            TakeDamageShoot(_cameraTransform.forward);
            IsShooting = true;
            _timeSinceLastShoot = 0f;
        }
    }

    void TakeDamageShoot(Vector3 forward)
    {
        // Kết hợp cả layer masks thông thường và weak point layer để raycast có thể hit cả weak point
        int combinedMask = CombinedLayerMask();
        if (weaponInfo.WeakPointLayerMask.value != 0)
        {
            combinedMask |= weaponInfo.WeakPointLayerMask.value;
        }

        var ray = new Ray(_cameraTransform.position, forward);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, combinedMask))
        {

            // Kiểm tra xem có trúng weak point không (dù có nằm trong layerMasksAndEffects hay không)
            bool isWeakPointHit = IsWeakPoint(hit);
            int calculatedDamage = CalculateDamage(weaponInfo.damage, hit);
            DamageType damageType = isWeakPointHit ? DamageType.Weakness : DamageType.Normal;

            DamageInfo damageInfo = new DamageInfo()
            {
                damageType = damageType,
                damage = calculatedDamage,
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
                    if (layerMasksAndEffects[i].effectType != GameConstants.EffectType.None)
                        SimplePool<GameConstants.EffectType>.Spawn<Effect>(layerMasksAndEffects[i].effectType, hit.point, Quaternion.identity).OnInit();
                    effectApplied = true;
                    break;
                }
            }

            // Nếu không áp dụng effect theo layer, có thể áp dụng effect mặc định cho weak point nếu hit trúng
            if (!effectApplied && isWeakPointHit)
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

    private Vector3 pointToHit;
    private void UpdateLaser(bool _Shoot)
    {
        if (_Shoot)
        {
            PlayGunEffect();
            pointToHit = GizmodCaculatorPointShoot();
            UpdateSingleBeam(laser[0], _muzzleTrans_1.position, pointToHit);
            UpdateSingleBeam(laser[1], _muzzleTrans_2.position, pointToHit);
            vfxLaserShoot.transform.position = pointToHit;
            TF.localPosition = Vector3.forward * TF.localPosition.z +
                               new Vector3(Random.Range(-1f, 1f) * magnitudeShoot,
                                   Random.Range(-1f, 1f) * magnitudeShoot, 0f);
        }
        else
        {
            if (pointToHit != Vector3.zero)
            {
                pointToHit = Vector3.zero;
                laser[0].localScale = Vector3.zero;
                laser[1].localScale = Vector3.zero;
            }
        }
    }

    private void UpdateSingleBeam(Transform beam, Vector3 startPoint, Vector3 visualEndPoint)
    {
        if (beam == null) return;
        Vector3 direction = visualEndPoint - startPoint;
        float length = direction.magnitude;

        beam.position = startPoint + (direction / 2.0f);
        beam.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0);
        beam.localScale = new Vector3(length, .5f, 1f);
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

    protected override void LogicStopGun()
    {
        base.LogicStopGun();
        if (CanPlayAnim)
        {
            IsShooting = false;
            CanPlayAnim = false;
            CanShoot = false;
            StopGunEffect();
            StopShootingSound();
            if (_corutineMoveBodyToEnd != null)
                StopCoroutine(_corutineMoveBodyToEnd);
            _corutineMoveBodyToEnd = StartCoroutine(MoveToPoint(Vector3.zero, .5f, false));
        }
    }

    IEnumerator MoveToPoint(Vector3 _end, float _speedMove, bool _canShootPlayDont)
    {
        while (Vector3.Distance(TF.localPosition, _end) > .01f)
        {
            TF.localPosition = Vector3.MoveTowards(TF.localPosition, _end, _speedMove * Time.deltaTime);
            yield return null;
        }
        TF.localPosition = _end;
        if (_canShootPlayDont)
        {
            TF.DOShakePosition(.3f, strengSnakeGun, 15, 45)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => { TF.localPosition = _bodyMoveEnd; });
        }
        CanShoot = _canShootPlayDont;
        _corutineMoveBodyToEnd = null;
    }
}