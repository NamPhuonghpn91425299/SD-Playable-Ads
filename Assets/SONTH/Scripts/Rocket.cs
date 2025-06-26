
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Rocket : MonoBehaviour, IPoolObject
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private ParticleSystem _fireEffect;
    [SerializeField] private GameObject _explosion;
    [SerializeField] private int _dame;
    [SerializeField] private float _lifeTime;
    [SerializeField] private float _speed;
    [SerializeField] private float _followSpeed;
    [SerializeField] private Transform _centerColli;
    [SerializeField] private Transform _centerExplosion;
    [SerializeField] private float _radiusColli;
    [SerializeField] private float _radiusExplosion;
    [SerializeField] private LayerMask _layerHit;
    [SerializeField] private float _timeToFoward = 2;
    [SerializeField] private float _rotationSpeed = 2;
    [SerializeField] private Transform _posCamFollow;
    private Vector3 _direction;
    private float _lifeTimer;
    private bool _isHitCollider;
    private bool _isFollowTarget;
    private float _timeCounter = 0;
    private Quaternion _firstRotation;
    private Quaternion _defaultRotation;
    //[SerializeField] private bool _isMoveDone;
    public float Speed { get =>  _speed; set { _speed = value;} }
    public Vector3 Direction { get => _direction; set { _direction = value;} }

    public GameObject Prefab { get ; set ; }
    [System.NonSerialized]
    public Transform target;
    private Transform _currentTarget;
    public Transform PosCamFollow => _posCamFollow;
    
    public void Initialize(Vector3 direction)
    {
        _direction = direction;
        _direction = (_direction - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }
    // Start is called before the first frame update
    void Start()
    { 
        _isHitCollider = false;
        _timeCounter = 0;
        _defaultRotation = transform.rotation;
    }
    private void OnEnable()
    {
        _audioSource?.Play();
        _fireEffect?.Play();
    }
    private void OnDisable()
    {
        _audioSource?.Stop();
        _fireEffect?.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        _lifeTimer += Time.deltaTime;
        if(_lifeTimer > _lifeTime)
        {
            Explosion();
        }
        else
        {
            if(!_isHitCollider)
            {
                if (!target || !_isFollowTarget)
                {
                    NormalFire();
                }
                else if(target && _isFollowTarget)
                {
                    FollowFire();
                }
                CheckHitCollider();
            }
        }

    }
    public void NormalFire()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }
    public void FollowFire()
    {
        _timeCounter += Time.deltaTime;
        Vector3 dir = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
        if (_timeCounter > _timeToFoward)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, _followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position - transform.forward, _followSpeed * Time.deltaTime);
        }
    }
    public void SetupTarget(Transform target)
    {
        this.target = target;
        if (target == null || !_isFollowTarget)
        {
            return;
        }
        else
        {
            _firstRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = _firstRotation;
        }
    }
    public void Explosion()
    {
        RocketController.Instance.PlayAudioExplosion();
        var obj = ObjectPool.Instance.PopFromPool(_explosion, instantiateIfNone: true);
        obj.transform.SetPositionAndRotation(transform.position, _explosion.transform.rotation);
        ExplosionRocket ex = obj.GetComponent<ExplosionRocket>();
        ex.Explosion();
        
        RocketController.Instance.SnakeCameraRocket();
        
        transform.rotation = _defaultRotation;
        ObjectPool.Instance.PushToPool(this, gameObject);
        _lifeTimer = 0;
        _isHitCollider = false;
        target = null;
        _timeCounter = 0;
    }
    
    public void CheckHitCollider()
    {
        Collider[] cols = Physics.OverlapSphere(_centerColli.position, _radiusColli, _layerHit);
        if(cols.Length != 0 )
        {
            _isHitCollider = true;
            Explosion();
            CheckHitExplosion();
            // if (CameraFollowRocket.Instance.isMoving)
            // {
            //     CameraFollowRocket.Instance.BackToDefault();
            // }
        }
    }
    public void CheckHitExplosion()
    {
        Collider[] cols = Physics.OverlapSphere(_centerExplosion.position, _radiusExplosion, _layerHit);
        List<Transform> lstRoot = new List<Transform> ();
        foreach (Collider col in cols)
        {
            if (!lstRoot.Contains(col.gameObject.transform.root))
            {
                lstRoot.Add(col.gameObject.transform.root);
            }
        }
        foreach(var elem in lstRoot)
        {
            var takeDamageController = elem.gameObject.GetComponentInParent<ITakeDamage>();
            if(takeDamageController != null)
            {
                // damageType = elem.CompareTag("WeakPoint") ? DamageType.Weekness : DamageType.Normal;
                BotNetwork botnet = elem.gameObject.GetComponentInParent<BotNetwork>();
                if (botnet != null)
                {
                    botnet.posExplosion = transform.position;
                }
                
                var damageInfo = new DamageInfo()
                {
                    damageType = DamageType.Gas,
                    damage = _dame,
                    name = elem.gameObject.name,
                };
                takeDamageController.TakeDamage(damageInfo);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!_centerColli || !_centerExplosion) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_centerExplosion.position, _radiusExplosion);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_centerColli.position, _radiusColli);
    }
    public void SetFollowTargetCheck(bool followTarget)
    {
        _isFollowTarget = followTarget;
    }
    public void Init()
    {
    }

    public void OnPushToPool()
    {
    }
}
