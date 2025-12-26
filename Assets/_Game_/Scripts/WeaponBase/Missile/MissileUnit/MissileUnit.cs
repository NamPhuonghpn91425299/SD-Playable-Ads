using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class MissileUnit : GameUnit<GameConstants.Missile_Player>
{
    [Header("MOVEMENT")]
    [SerializeField] protected float _speed = 10f;
    [SerializeField] private float _rotateSpeed = 95;
    [SerializeField] protected float lifeTime = 5f;
    [SerializeField] protected bool isFollow;
    protected float realTime;
    
    [Header("Damage Settings")]
    [SerializeField] protected LayerMask _layerTarget;
    [SerializeField] Transform TF_CenterExplosion;
    [SerializeField] protected float _radiusTriggerExplosion = 1;
    [SerializeField] protected float _radiusExplosion = 3;
    [SerializeField] protected int _damage = 50;
    [SerializeField] protected GameObject _explosion;
    [SerializeField] protected GameObject _body;
    
    [Header("Target")]
    [SerializeField] protected Transform _targetFollow;
    [SerializeField] protected Vector3 pointTarget;

    [Header("PREDICTION")]
    [SerializeField] private float _minDistanceFollow = 5;
    [Tooltip("Phần trăm bay thẳng 0 -> 1")][SerializeField] private float _forwardDistanceThreshold = 0.3f; // 30% of distance
    private Vector3 _offset;
    private float _timeToForward;
    private bool isDead;
    
    public virtual void OnInit(Transform _gameObjectFollow = null,Vector3 _pointTarget = default)
    {
#if UNITY_EDITOR
        if (_gameObjectFollow == null && _pointTarget == default)
            Debug.LogError("MissileUnit OnInit: GameObjectFollow and FowardDirection is null or default value");
#endif
        isDead = false;
        _body.SetActive(true);
        _targetFollow = _gameObjectFollow;
        isFollow = _targetFollow != null;
        pointTarget = isFollow ? _gameObjectFollow.position : _pointTarget;
        realTime = 0;
        
        TF.DOLookAt(GameController.Instance.CurrentWeapon.GizmodCaculatorPointShoot(), 0);
        if(isFollow) 
        {
            _offset = Vector3.zero;
            _timeToForward = Time.time + Vector3.Distance(TF.position,_targetFollow.position)*_forwardDistanceThreshold/_speed;
        }
        
        // EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData{duration = .3f,strength = .2f,vibrato = 15,randomness = 45}));
    }

    protected virtual void Update()
    {
        if(isDead)
            return;
        CheckTriggerExplosion();
        realTime += Time.deltaTime;
        if(isFollow)
            MissileFollow();
        else
            MissileForward();
        
        if(realTime >= lifeTime)
            OnExplosion();
    }
    

    #region --------- Missile Follow -----------
    private void MissileFollow()
    {
        // Calculate the direction to the target with some lag
        Vector3 targetDirection = (_targetFollow.position - TF.position).normalized;
        Vector3 smoothedDirection = Vector3.Lerp(TF.forward, targetDirection, Time.deltaTime * _rotateSpeed);
    
        // Update position with forward motion
        float step = _speed * Time.deltaTime;
        TF.position += smoothedDirection * step;
    
        // Smoothly rotate towards the target
        Quaternion targetRotation = Quaternion.LookRotation(smoothedDirection);
        TF.rotation = Quaternion.Slerp(TF.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
    
        // Stop following if close enough to the target
        if (Vector3.Distance(TF.position, _targetFollow.position) < _minDistanceFollow)
            StopFollowing();
    }
    
    public void StopFollowing()
    {
        isFollow = false;
    
        // Smoothly align the missile's direction to the target's last known position
        Quaternion targetRotation = Quaternion.LookRotation(_targetFollow.position - TF.position);
        TF.DORotateQuaternion(targetRotation, 0.5f);
    }
    
    #endregion

    #region Check Layer Target And Take Damage

    void CheckTriggerExplosion()
    {
        Collider[] colliders = Physics.OverlapSphere(TF_CenterExplosion.position, _radiusTriggerExplosion, _layerTarget);
        if(colliders.Length <= 0)
            return;
        OnExplosion();
        CheckTakeDamage();
    }

    void CheckTakeDamage()   
    {
        Collider[] cols = Physics.OverlapSphere(TF.position, _radiusExplosion, _layerTarget);
        HashSet<ITakeDamage> damagedTargets = new HashSet<ITakeDamage>();

        foreach (Collider col in cols)
        {
            ITakeDamage iTakeDamage = col.GetComponentInParent<ITakeDamage>();
            if (iTakeDamage == null)
                iTakeDamage = col.GetComponent<ITakeDamage>();

            if (iTakeDamage != null && !damagedTargets.Contains(iTakeDamage))
            {
                damagedTargets.Add(iTakeDamage);

                var damageInfo = new DamageInfo()
                {
                    damageType = DamageType.Explosion,
                    damage = _damage,
                    posExplosion = TF_CenterExplosion.position,
                };

                Transform transformEnemyThis = iTakeDamage.GetTransformThis();
                if (transformEnemyThis != null && transformEnemyThis.parent != null)
                {
                    damageInfo.damageType = DamageType.Normal;
                }

                Debug.Log(iTakeDamage.ToString() + " takes damage");
                iTakeDamage.OnTakeDamage(damageInfo);
            }
        }
    }
    
    public virtual void OnExplosion()
    {
        //EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData{duration = .3f,strength = 1f,vibrato = 15,randomness = 45}));
        DOTween.Kill(TF);
        isDead = true;
        _body.SetActive(false);
        _explosion.SetActive(true);
        StartCoroutine(IEOnDespawn());
    }
    #endregion
    
    protected virtual void MissileForward()
    {
        TF.position += TF.forward * _speed * Time.deltaTime;
    }

    public virtual IEnumerator IEOnDespawn()
    {
        yield return HelperCoroutine.GetWait(1.5f);
        _explosion.SetActive(false);
        SimplePool<GameConstants.Missile_Player>.Despawn(this);
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(TF_CenterExplosion.position, _radiusExplosion);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(TF_CenterExplosion.position, _radiusTriggerExplosion);
    }
}