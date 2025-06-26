//using UnityEditor.Presets;
using UnityEngine;

public class BulletTrail : MonoBehaviour, IPoolObject
{
    [SerializeField] protected Transform _trail;
    [SerializeField] protected MeshRenderer[] meshRenderers;
    [SerializeField] protected Material[] materials;
    [SerializeField] protected Vector3 _trailStartScale;
    [SerializeField] protected Vector3 _trailMaxScale;
    [SerializeField] protected float _trailLengthAtMaxScale;
    public float Speed;
    protected Vector3 _direction;
    protected float _traveledDistance;
    protected float _maxDistance;
    public float LifeTime = 5f;
    protected float _lifeTimer;
    protected Transform _transform;
    protected bool _alignWithPlayerView;

    public Vector3 posE;

    public GameObject Prefab { get; set; }

    private void Awake()
    {
        _transform = transform;
        posE = transform.forward * 1;
    }

    public void Init()
    {
    }
 

    public void OnPushToPool()
    {
        _trail.localScale = _trailStartScale;
        Speed = 300;
    }
    public void InitType2(Vector3 direction, Vector3 posE)
    {
        this.posE = posE;
        _lifeTimer = 0;
        if (_trail) _trail.localScale = _trailStartScale;
        gameObject.SetActive(true);
        _direction = direction;
        _transform.rotation = Quaternion.LookRotation(_direction);
        _maxDistance = Speed * LifeTime;
        _traveledDistance = 0;
        Update();
    }
    public void Init(Vector3 direction)
    {
        _lifeTimer = 0;
        if (_trail) _trail.localScale = _trailStartScale;
        gameObject.SetActive(true);
        _direction = direction;
        _transform.rotation = Quaternion.LookRotation(_direction);
        _maxDistance = Speed * LifeTime;
        _traveledDistance = 0;
        Update();
    }

    // Update is called once per frame
    protected void Update()
    {
        if(transform.position.z > posE.z)
            Despawn();
        
        if (_traveledDistance >= _maxDistance || _lifeTimer > LifeTime)
        {
            ObjectPool.Instance.PushToPool(this, gameObject);
        }
        else
        {
            _lifeTimer += Time.deltaTime;
            var movement = Speed * Time.deltaTime;
            _transform.position += _direction * movement;
            _traveledDistance += movement;
            if (_trail)
                _trail.localScale = Vector3.Lerp(_trailStartScale, _trailMaxScale,
                    _traveledDistance / _trailLengthAtMaxScale);
        }
    }
    
    public void Despawn()
    {
        ObjectPool.Instance.PushToPool(this, gameObject);
    }
}