using Luna.Unity.FacebookInstantGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketOnBot : MonoBehaviour, IPoolObject
{
    [SerializeField] private float damage;
    [SerializeField] private BotConfigSO botConfigSO;
    //[SerializeField] private float rotationSpeed =350f;
    [SerializeField] private AudioSource audioSource;
    private Vector3 direction;
    [SerializeField] private GameObject explosionPrb;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 explosionPosition ;
    //private Vector3 vectorCam;
    public GameObject Prefab { get ; set ; }

    public void Init()
    {

        target = LocalPlayer.Instance.GetTranformPlayer();
        explosionPosition = target.position;
    }

    private void OnEnable()
    {
        this.Initialize(damage,direction);
        //direction = (explosionPosition - transform.position).normalized;
        gameObject.SetActive(true);
        audioSource?.Play();
    }
    private void OnDisable()
    {
        audioSource?.Stop();
    }
    public void Initialize(float damage, Vector3 direction)
    {
        this.damage = damage;
        this.direction = direction.normalized;

    }

    void Update()
    {
        // Di chuyển đạn
        transform.position += direction * (botConfigSO.rocketSpeed * Time.deltaTime);
        float sqrDistance = (transform.position - explosionPosition).sqrMagnitude;
        if (sqrDistance < 1f) // (0.1 * 0.1)
        {
            EventManager.Invoke(EventName.OnTakeDamagePlayer, damage);
            var spawnExplosion = GetExplosion(LocalPlayer.Instance._posExplosion.position, target.rotation);
            gameObject.SetActive(false);
            ObjectPool.Instance.PushToPool(this,gameObject);
        }
        //transform.Rotate(0, 0, rotationSpeed * Time.deltaTime*10f);
    }
    private GameObject GetExplosion(Vector3 position, Quaternion rotation)
    {
        var explosion = ObjectPool.Instance.PopFromPool(explosionPrb, instantiateIfNone: true);
        explosion.transform.SetPositionAndRotation(new Vector3(position.x, position.y, position.z), rotation);
        return explosion;
    }
    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))  // Đảm bảo tag phù hợp với player của bạn
    //    {
    //        EventManager.Invoke(EventName.OnTakeDamagePlayer, damage);
    //        Debug.Log($"Đạn trigger với player, gây {damage} sát thương");
    //        //ObjectPool.Instance.PushToPool(this, gameObject);
    //         // Hủy đạn sau khi trigger
    //        //var explosion = ObjectPool.Instance.PopFromPool(explosionPrb, instantiateIfNone:true);
    //        //explosion.transform.SetPositionAndRotation(transform.position, transform.rotation);
    //    }
    //}

    public void OnPushToPool()
    {

    }
}
