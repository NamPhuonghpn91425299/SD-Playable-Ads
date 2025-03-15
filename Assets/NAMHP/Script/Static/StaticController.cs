using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticController : MonoBehaviour, IPoolObject
{
    public BotNetwork botNetwork;
    public GameObject explosion;
    public GameObject model;
    public GameObject deathStep;
    //public ParachuteStatic parachuteStatic;
    [Header("Cài Đặt Nổ")]
    public float explosionRadius = 5f;
    public int explosionDamage = 50;
    public LayerMask botLayer; // Chọn layer của bot
    bool isDead = false;

    private void Start()
    {
        model.SetActive(true);
        deathStep.SetActive(false);
        botNetwork.OnBotDead += BotDead;
    }

    private void OnDisable()
    {
        botNetwork.OnBotDead -= BotDead;
    }

    private void BotDead()
    {
        isDead = true;
        model.SetActive(false);
        deathStep.SetActive(true);
        EventManager.Invoke(EventName.OnStaticBotDead, isDead);
        //OnStaticExplode?.Invoke();
        // Gây damage cho bot xung quanh
        Explode();
        // Hiệu ứng nổ
        GameObject exp = ObjectPool.Instance.PopFromPool(explosion, instantiateIfNone: true);
        exp.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        StartCoroutine(HideBotOnDie());
    }

    private void Explode()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, botLayer);
        //Debug.Log($"🔴 Số collider trong vùng nổ: {hitColliders.Length}");
        HashSet<BotNetwork> affectedBots = new HashSet<BotNetwork>(); // Để tránh gây damage trùng

        foreach (Collider col in hitColliders)
        {
            var botNetwork = col.GetComponentInParent<BotNetwork>(); // Tìm BotNetwork trên object cha
            if (botNetwork != null && !affectedBots.Contains(botNetwork))
            {
                affectedBots.Add(botNetwork); // Thêm bot vào danh sách (tránh trùng lặp)

                var damageInfo = new DamageInfo()
                {
                    damageType = DamageType.Normal,
                    damage = explosionDamage,
                };

                botNetwork.TakeDamage(damageInfo);
                Debug.Log($"💥 Gây {explosionDamage} damage lên bot: {botNetwork.gameObject.name}");
            }
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            BotDead();
        }
    }

    private IEnumerator HideBotOnDie()
    {
        yield return new WaitForSeconds(2f);
        ObjectPool.Instance.PushToPool(this, gameObject);
    }

    private void OnDrawGizmos()
    {
        // Vẽ phạm vi nổ trong Scene View
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public GameObject Prefab { get; set; }
    public void Init() { }

    public void OnPushToPool() { }
}