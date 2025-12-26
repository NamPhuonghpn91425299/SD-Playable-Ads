using System;
using UnityEngine;
using static GameConstants;

public class BulletTrail : GameUnit<ProjecttilePlayer>
{
    [SerializeField] protected Vector3 _trailStartScale;
    [SerializeField] protected Vector3 _trailMaxScale;
    [SerializeField] protected float _trailLengthAtMaxScale;
    public float Speed;
    private Vector3 _direction;
    private float _traveledDistance;
    public float LifeTime = 2f;
    private float _lifeTimer;
    private Vector3 posE;

    private void Awake()
    {
        posE = transform.forward * 1;
    }

    public void Init(Vector3 direction, Vector3 posE)
    {
        this.posE = posE;
        _lifeTimer = 0;
        _traveledDistance = 0;
        TF.localScale = _trailStartScale;
        _direction = direction;
        TF.rotation = Quaternion.LookRotation(_direction);
    }

    // Update is called once per frame
    protected void Update()
    {
        if (HasPassedTarget(TF.position, _direction, posE) || _lifeTimer > LifeTime)
            OnDespawn();
        else
        {
            _lifeTimer += Time.deltaTime;
            var movement = Speed * Time.deltaTime;
            TF.position += _direction * movement;
            _traveledDistance += movement;
            TF.localScale = Vector3.Lerp(_trailStartScale, _trailMaxScale, _traveledDistance / _trailLengthAtMaxScale);
        }

    }
    bool HasPassedTarget(Vector3 currentPos, Vector3 direction, Vector3 targetPos)
    {
        Vector3 toTarget = targetPos - currentPos;
        return Vector3.Dot(toTarget, direction) < 0f;
    }
    public void OnDespawn()
    {
        TF.localScale = _trailStartScale;
        SimplePool<ProjecttilePlayer>.Despawn(this);
    }
}