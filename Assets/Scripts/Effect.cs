using System;
using UnityEngine;

[Serializable]
public class Effect : MonoBehaviour, IPoolObject
{
    private float _timer;

    [SerializeField] private double _lifeTime;

    [SerializeField] public ParticleSystem[] particles;
    public GameObject Prefab { get; set; }
    private Transform _transform;
    private Transform _parent;
    private Vector3 _localPosition;

    private bool haveParent = false;
    
    private void Awake()
    {
        _transform = transform;
    }

    public void Init()
    {
        _timer = 0;

        foreach (var item in particles)
        {
            item.Play();
        }
    }

    public void Init(Vector3 pos)
    {
        transform.position=pos;
        Update();
    }

    public void OnPushToPool()
    {
    }

    private void Update()
    {
        if (_timer > _lifeTime)
        {
            ObjectPool.Instance.PushToPool(this, gameObject);
        }
        else
        {
            _timer += Time.deltaTime;
            if (haveParent)
            {
                _transform.position = _parent.TransformPoint(_localPosition);   
            }
        }
    }
}