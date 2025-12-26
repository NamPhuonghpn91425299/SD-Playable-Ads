using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MechOneHand_Network : VehicleNetwork
{
#if UNITY_EDITOR
    [Header("Editor Only")]
    [SerializeField] [Range(0, 10)] int percentHPDestroyGunFireBall = 7;
    [SerializeField] [Range(0, 10)] int percentHPDestroyGunRocket = 5;
    [SerializeField] [Range(0.05f, .4f)] float HPShield;
    
    protected override void OnValidate()
    {
        base.OnValidate();
        HPDestroyGunFireBall = botConfigSO.health * 10 * percentHPDestroyGunFireBall / 100;
        HPDestroyGunRocket = botConfigSO.health * 10 * percentHPDestroyGunRocket / 100;
    }
#endif
    [Header("References")] 
    [SerializeField] private MOH_Attack attackState;
    
    [Header("Equal Percent HP Don't Edit")]
    [SerializeField] private int HPDestroyGunFireBall;
    [SerializeField] private int HPDestroyGunRocket;
    private bool canDestroyGunFireBall = true; // nếu bằng false thì không thể phá hủy nữa đông thời có thể bắn roket
    private bool canDestroyRocket = true;

    [Header("Destroy Gun Fire Ball")] 
    [SerializeField] private GameObject[] GunFireBallRiew;
    [SerializeField] SkinnedMeshRenderer _skinnedMeshRendererFireBall;
    [SerializeField][Tooltip("Cái này sẽ bật lên và bay ra")] private Transform GunFireBallFake;
    [SerializeField] private ParticleSystem[] vfxDestroyGunFireBall;


    [Header("Explosion Gun Rocket")]
    [SerializeField] private MeshRenderer[] _meshRendererRocket;
    [SerializeField] private ParticleSystem[] vfxDestroyGunRocket;
    
    [Header("Shield Object")]
    [SerializeField] MeshRenderer _shieldObjectRenderer;    
    
    [Header("Dead Material")]
    [SerializeField] SkinnedMeshRenderer _skinnedMeshRendererDead;
    [SerializeField] private Material _materialDead;
    [SerializeField] private Material _materialDefault;

    [Header("Audio")] public AudioSource _audioSourceGetHit;
    public AudioClip _audioclipGetHit;

    public AudioSource audioBGCombat;

    private void OnEnable()
    {
        audioBGCombat.Play();
    }

    private void OnDisable()
    {
        
        audioBGCombat.Stop();
    }

    private void FixedUpdate()
    {
        if (audioBGCombat.enabled && GameController.Instance?.CurrentGameState != GameConstants.GameState.InGame)
            audioBGCombat.enabled = false;
    }

    public override void OnInit()
    {
        base.OnInit();
        _skinnedMeshRendererDead.material = _materialDefault;
        foreach (MeshRenderer VARIABLE in _meshRendererRocket)
            VARIABLE.material = _materialDefault;
        foreach (GameObject VARIABLE in GunFireBallRiew)
            VARIABLE.SetActive(true);
        _skinnedMeshRendererFireBall.material.SetFloat("_IsWeakness", 1f);
        GunFireBallFake.gameObject.SetActive(false);
    }

    public override void CacularHealth(DamageInfo damageInfo)
    {
        base.CacularHealth(damageInfo);
        if (canDestroyGunFireBall && _currentHealth <= HPDestroyGunFireBall) 
            DestroyGunFireBall();
        else if (canDestroyRocket && _currentHealth <= HPDestroyGunRocket) 
            DestroyGunRocket();
        else if (damageInfo.damageType == DamageType.Explosion && Random.Range(0, 10) < 5 && !isDead) 
            stateController.ChangeState(GameConstants.EnemyState.Stun);
    }

    private void DestroyGunRocket()
    {
        _audioSourceGetHit.PlayOneShot(_audioclipGetHit);
        canDestroyRocket = false;
        foreach (MeshRenderer VARIABLE in _meshRendererRocket)
            VARIABLE.material = _materialDead;
        
        foreach (ParticleSystem VARIABLE in vfxDestroyGunRocket) 
            VARIABLE.Play();
        if (!isDead) 
            stateController.ChangeState(GameConstants.EnemyState.Shield);
    }

    private void DestroyGunFireBall()
    {
        _audioSourceGetHit.PlayOneShot(_audioclipGetHit);
        attackState.StopFireBallVFX();
        canDestroyGunFireBall = false;
        foreach (ParticleSystem VARIABLE in vfxDestroyGunFireBall) 
            VARIABLE.Play();
        foreach (MeshRenderer VARIABLE in _meshRendererRocket)
            VARIABLE.material.SetFloat("_IsWeakness", 1f);
        foreach (GameObject VARIABLE in GunFireBallRiew) 
            VARIABLE.SetActive(false);
        GunFireBallFake.gameObject.SetActive(true);
        stateController.ChangeState(GameConstants.EnemyState.Stun);
        StartCoroutine(IEMoveGunFireBallFake(GunFireBallFake, 30, 50, TF.position.y, 500));
    }
    
    private IEnumerator IEMoveGunFireBallFake(Transform _bodyMove, float initialForce, float _gravity, float _groundY, float rotationSpeed)
    {
        _bodyMove.parent = null;
        Vector3 velocity = (-_bodyMove.right + Vector3.up * 0.5f).normalized * initialForce;

        // Tạo hướng quay ngẫu nhiên
        Vector3 randomRotation = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * rotationSpeed;

        while (_bodyMove.position.y > _groundY)
        {
            // Di chuyển theo velocity
            _bodyMove.position += velocity * Time.deltaTime;

            // Áp dụng trọng lực
            velocity += Vector3.down * _gravity * Time.deltaTime;

            // Quay
            _bodyMove.Rotate(randomRotation * Time.deltaTime);

            yield return null;
        }

        // Chạm đất: snap về đúng mặt đất nếu vượt qua
        Vector3 pos = _bodyMove.position;
        pos.y = _groundY;
        _bodyMove.position = pos;

        // Ngừng chuyển động
        // (Không cần làm gì thêm vì coroutine đã kết thúc)
    }
    
    public override void Other(int _type)
    {
        if(_type == 1)// xong sửa chữa rocket, tắt shield chuyển lại material + hồi máu
        {
            _currentHealth = HPDestroyGunRocket + 1000;
            // OnTakeDamage(new DamageInfo { damage = -(), damageType = DamageType.Normal });
            canDestroyRocket = true;
            foreach (MeshRenderer VARIABLE in _meshRendererRocket)
            {
                VARIABLE.material = _materialDefault;
                VARIABLE.material.SetFloat("_IsWeakness", 1f);
            }

        }else if (_type == 2) //cheets thay material
        {
            _skinnedMeshRendererDead.material = _materialDead;
        }
    }

    public override bool GetBool(int _NameOrType)
    {
        if(_NameOrType == 0)//lấy bool của canDestroyGunFireBall
            return canDestroyGunFireBall;
        else//lấy bool của canDestroyRocket
            return canDestroyRocket;
    }
}