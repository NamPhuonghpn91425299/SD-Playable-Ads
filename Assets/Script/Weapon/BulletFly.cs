using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class BulletFly : MonoBehaviour,IDamageDealt
{
    float simulationSpeed;

    Vector3 startPoint;
    Vector3 endPoint;
    public IDamageHit damageHit;
    public float damage;
    float remainingDistance, distance;
    [SerializeField]
    private PoolType bulletPool;

    public void Init(Vector3 startPoint, Vector3 endPoint, float simulationSpeed)
    {
        this.simulationSpeed = simulationSpeed;
        this.startPoint = startPoint;
        this.endPoint = endPoint;

        distance = Vector3.Distance(endPoint, startPoint);
        remainingDistance = 0;
    }

    public void Init(int damage, IDamageHit target)
    {
       damageHit = target;
       this.damage = damage;
    }

    public void TryHit()
    {
        Debug.Log($"dang ban ne  {damageHit == null}");
        damageHit?.OnHit((int)damage);
        DespawnBullet(this);
    }
    public void DespawnBullet(BulletFly bullet)
    {
        ObjectPool1.Instance.ReturnToPool(this.bulletPool, bullet.gameObject);
    }
    private void Update()
    {
      
        remainingDistance += simulationSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01((remainingDistance / distance)));

        if (remainingDistance >= distance)
        {
            TryHit();

        }
    }
}
