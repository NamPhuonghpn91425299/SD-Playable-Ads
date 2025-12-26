using System;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using static GameConstants;
public class MechRoninAnimatorControll : MonoBehaviour
{
    [SerializeField] public VehicleNetwork botContext;
    [SerializeField] Animator ator;
    [SerializeField] Mech_SoundManager soundManager;
    [SerializeField] RocketAttackPhase3Standalone rocketAttackPhase3Standalone;
    [SerializeField] GameObject shieldSound;

    [SerializeField]
    private GameObject m_explosion;
    [Space]
    [Header("EFFECTS")]
    [SerializeField] GameObject shield;
    [SerializeField] GameObject fireEffect;
    [SerializeField] GameObject[] pullEffects;
    [SerializeField] Texture _liveTexture;
    [SerializeField] Texture _deadTexture;
    [SerializeField] Renderer[] _rendererBodyPart;
    [SerializeField] GameObject _landingExplosionObj;
    [SerializeField] GameObject _explosionNoEffect;          // dùng để shake cam

    [Space]
    [Header("SPAWN PARTICLES SMOKE")]
    [SerializeField] ParticleSystem[] _particlesParents;
    [Tooltip("Transform của object (nếu không set sẽ tự lấy)")]
    [SerializeField] private Transform m_transform;
    [SerializeField] private Transform m_muzzle;
    [Tooltip("Khoảng cách tối đa để xác định đã đến độ cao bay lên")]
    [SerializeField] private float m_distanceFlyUp = 30f;
    [SerializeField] private int m_damage = 50;
    [SerializeField] private float m_moveSpeed = 5f;
    public float StrengSnakeCam = .5f;
    private Transform _mytrans;
    private Transform _target;
    private bool _isMoving = false;
    private static readonly Vector3 _saveShieldPos = new Vector3(0, 3.1f, 7);


    private void OnEnable()
    {
        m_explosion.SetActive(false);
        _target = PlayerInstant.Instance?.TF.transform;
        _mytrans = this.transform.root;
        TurnOffEffectLeg();
        TurnOnOffPullEffects(0);
        foreach (var item in _rendererBodyPart)
        {
            item.material.mainTexture = _liveTexture;
        }

        if (pullEffects != null)
        {
            foreach (var item in pullEffects)
            {
                item.SetActive(false);
            }
        }


    }
    void Start()
    {
        m_damage = botContext.Damage;
    }
    public void SetupSound(Mech_SoundManager soundMng)
    {
        soundManager = soundMng;
    }

    /// <summary>
    /// gọi trong anim landing_new
    /// </summary>
    public void LandingDone()
    {
        if (_landingExplosionObj) _landingExplosionObj.SetActive(true);
        EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData { duration = .3f, strength = StrengSnakeCam, vibrato = 15, randomness = 45 }));
    }

    public void ShowOffDone()
    {
        //OnShowOffDone.Publish();
    }


    /// <summary>
    /// gọi trong anim bắn súng
    /// </summary>
    public void AttackShootGun()
    {
        //AttackingShootGun.Publish();
        if (soundManager) soundManager.PlayOneShot(2);
        if (fireEffect) fireEffect.SetActive(true);
        var bullet = SimplePool<ProjectileEnemy>.Spawn<Rocket>(ProjectileEnemy.BulletSourceForRonin, m_muzzle.position, m_muzzle.rotation);
        bullet.Init(m_damage);

    }

    /// <summary>
    /// gọi đầu anim Idle
    /// </summary>
    public void SetStateAction()
    {
        //SetState.Publish();
    }

    /// <summary>
    /// gọi cuối anim tấn công
    /// </summary>
    public void OnAttackDone()
    {
        //AttackingDone.Publish();
    }

    /// <summary>
    /// gọi cuối anim ultimate
    /// </summary>
    public void UltimateDone()
    {
        // OnUltimateDone.Publish();
    }

    /// <summary>
    /// gọi trong anim đổi kiếm (isOn = 1 : true)
    /// </summary>
    public void TurnOnShield(int isOn)
    {
        botContext.SetIsImmortal(true);    // khi bật khiên thì bot ko nhận dmg
        var offsetY = (_mytrans.position.y - _target.position.y) * 0.12f;
        shield.transform.localPosition = new Vector3(_saveShieldPos.x, _saveShieldPos.y - offsetY, _saveShieldPos.z);
        if (shield != null)
        {
            shield.transform.DOScale(isOn == 1 ? new Vector3(1.3f, 1.3f, 1.3f) : Vector3.zero, 0.15f).OnComplete(() =>
            {
                if (this == null) return;
                // SetDamageScale.Publish(isOn == 1 ? 0 : 1);              // khi bật khiên thì bot ko nhận dmg
            });
        }
        if (isOn == 0)
        {
            botContext.SetIsImmortal(false);   // khi tắt khiên thì bot nhận dmg
            if (shieldSound) shieldSound.SetActive(false);
            if (soundManager) soundManager.PlayOneShot(10);
        }
    }

    public void PlayShieldSound()
    {
        if (shieldSound) shieldSound.SetActive(true);
    }

    /// <summary>
    /// gọi cuối anim đổi kiếm và cuối anim đi bộ (status = 1: di chuyển, 0: đứng lại)
    /// </summary>
    public void MoveToWards(int status)
    {
        if (status == 1)
        {
            _isMoving = true;
            StartCoroutine(MoveTowardsTargetCoroutine());
        }
        else
        {
            _isMoving = false;
        }
    }

    private IEnumerator MoveTowardsTargetCoroutine()
    {
        while (_isMoving && _target != null)
        {
            // Tính hướng di chuyển về phía target
            Vector3 direction = transform.forward.normalized;  //(_target.position - _mytrans.position).normalized;
            //direction.y = 0; // Giữ nguyên chiều cao, chỉ di chuyển trên mặt phẳng XZ

            // Di chuyển về phía trước
            _mytrans.position += direction * m_moveSpeed * Time.deltaTime;

            // // Quay mặt về phía target
            // if (direction != Vector3.zero)
            // {
            //     _mytrans.forward = Vector3.Slerp(_mytrans.forward, direction, Time.deltaTime * 3f);
            // }

            yield return null;
        }
    }

    /// <summary>
    /// gọi trong anim sword attack
    /// </summary>
    public void TwelveRocketsAttack()
    {
        rocketAttackPhase3Standalone.TriggerRealRockets();
    }

    /// <summary>
    /// gọi trong anim dash, idLeg 0: cả 2 chân, 1: trái, 2 phải
    /// </summary>
    public void TurnOnEffectLeg()
    {
        foreach (var item in _particlesParents)
        {
            item.Play();
        }

    }
    public void TurnOffEffectLeg()
    {
        foreach (var item in _particlesParents)
        {
            item.Stop();
            item.Clear();
        }
    }

    /// <summary>
    /// gọi trong anim dash, bật phản lực (1: on, 0: off)
    /// </summary>
    public void TurnOnOffPullEffects(int id)
    {
        if (pullEffects != null)
        {
            foreach (var item in pullEffects)
            {
                item.SetActive(id == 1);
            }
        }
    }

    /// <summary>
    /// gọi cuối anim dash lần 1 của phase 3, truyền vào index 2 để dash lần 2 luôn
    /// </summary>
    public void DashAttackPhase3(int index)
    {
        //DashAttackCall.Publish(index);
    }

    /// <summary>
    /// gọi trong anim bay lên
    /// </summary>
    public void OnFlyUp()
    {
        transform.root.DOMoveY(transform.root.position.y + m_distanceFlyUp, 1f);
        if (soundManager) soundManager.PlayOneShot(9);
    }

    /// <summary>
    /// gọi trong anim chết
    /// </summary>
    public void SetupBodyDead()
    {
        foreach (var item in _rendererBodyPart)
        {
            item.material.mainTexture = _deadTexture;
        }
        m_explosion.SetActive(true);
    }

    public void OnBotDead()
    {
        //m_explosion.SetActive(true);
        TurnOffEffectLeg();
        TurnOnOffPullEffects(0);
    }

    public void ReturnPool()
    {
        botContext.OnDespawn(1f);
    }
    private void OnDisable()
    {
        _isMoving = false;
        StopAllCoroutines();

        if (shield)
        {
            shield.transform.DOKill();
            shield.transform.localScale = Vector3.zero;
            _landingExplosionObj.SetActive(false);
        }
    }

}

