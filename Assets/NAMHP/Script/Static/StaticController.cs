using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticController : MonoBehaviour, IPoolObject
{
    public BotNetwork botNetwork; // BotNetwork của chính StaticController này
    public GameObject explosionPrefab; // Đổi tên cho rõ ràng, vì nó là prefab
    public GameObject model;
    public GameObject deathStep;

    [Header("Cài Đặt Nổ")]
    public float explosionRadius = 5f;
    public int explosionDamage = 50;
    public LayerMask botLayer;
    bool isStaticBotDead = false; // Cờ cho biết StaticController này đã chết và nổ

    // IPoolObject properties
    public GameObject Prefab { get; set; }

    private void OnEnable()
    {
        model.SetActive(true);
        deathStep.SetActive(false);
        isStaticBotDead = false; // Reset khi được lấy từ pool

        // Đăng ký vào event chết của BotNetwork liên kết với StaticController này
        if (botNetwork != null)
        {
            botNetwork.OnBotDead += HandleAssociatedBotDeath;
        }
        else
        {
            Debug.LogError("StaticController không có BotNetwork được gán!", this);
        }
    }

    private void OnDisable()
    {
        if (botNetwork != null)
        {
            botNetwork.OnBotDead -= HandleAssociatedBotDeath;
        }
    }

    // Hàm này được gọi khi BotNetwork của chính StaticController này chết
    private void HandleAssociatedBotDeath()
    {
        if (isStaticBotDead) return; // Đã xử lý rồi

        isStaticBotDead = true;
        model.SetActive(false);
        deathStep.SetActive(true);

        //EventManager.Invoke(EventName.OnStaticBotDead, true); // Thông báo StaticBot này đã chết

        // Gây damage và hiệu ứng nổ
        ExplodeTargets();

        // Hiệu ứng nổ của chính StaticController
        if (explosionPrefab != null)
        {
            GameObject expInstance = ObjectPool.Instance.PopFromPool(explosionPrefab, instantiateIfNone: true);
            expInstance.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
            // ParticleSystem ps = expInstance.GetComponent<ParticleSystem>();
            // if (ps != null) ps.Play();
            // TODO: Trả lại expInstance vào pool sau khi hiệu ứng kết thúc
        }
        else
        {
            Debug.LogWarning("Explosion prefab chưa được gán cho StaticController.", this);
        }


        StartCoroutine(ReturnToPoolAfterDelay(2f));
    }

    private void ExplodeTargets()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, botLayer);
        HashSet<BotNetwork> affectedBots = new HashSet<BotNetwork>();

        foreach (Collider col in hitColliders)
        {
            // Lấy BotNetwork của mục tiêu
            var targetBotNetwork = col.GetComponentInParent<BotNetwork>();

            // Bỏ qua nếu không có BotNetwork, hoặc là chính StaticController này, hoặc đã xử lý
            if (targetBotNetwork == null || targetBotNetwork.gameObject == this.gameObject || affectedBots.Contains(targetBotNetwork))
            {
                continue;
            }

            affectedBots.Add(targetBotNetwork);

            // Lưu trạng thái isDead của targetBotNetwork TRƯỚC KHI gây damage
            bool targetWasAlreadyDead = targetBotNetwork.IsDead; // Giả định BotNetwork có cờ isDead

            // Gây damage
            var damageInfo = new DamageInfo()
            {
                damageType = DamageType.Normal, // Nên là Explosion
                damage = explosionDamage,

            };
            targetBotNetwork.TakeDamage(damageInfo);

            // Kiểm tra xem targetBotNetwork có chết SAU KHI nhận damage không
            // và nó chưa chết trước đó
            if (!targetWasAlreadyDead && targetBotNetwork.IsDead)
            {
                var targetAnimator = targetBotNetwork.GetComponentInChildren<Animator>(); // Hoặc GetComponent<Animator>()
                if (targetAnimator != null)
                {
                    // Kích hoạt trạng thái chết (ví dụ: bằng Trigger hoặc Bool)
                    // Nếu dùng Bool "isDead" như bạn đề cập:
                    targetAnimator.SetBool("isDead", true); // Đảm bảo Animator Controller của bot có parameter này
                                                          // và có transition từ Any State đến Death BlendTree State dựa trên isDead = true

                    // Hoặc nếu dùng Trigger "DieTrigger" (khuyến nghị hơn cho sự kiện một lần):
                    // targetAnimator.SetTrigger("DieTrigger");

                    // Cập nhật hướng cho blend tree TRÊN BOT MỤC TIÊU
                    // Hệ quy chiếu phải là của BOT MỤC TIÊU
                    ExplosionAnimatorHelper.PlayExplosionBlendForTarget(targetAnimator, transform, targetBotNetwork.transform);

                    Debug.Log($"💥 {targetBotNetwork.name} killed by explosion. Anim params updated. Origin: {transform.position}, Target: {targetBotNetwork.transform.position}");
                }
                else
                {
                    Debug.LogWarning($"{targetBotNetwork.name} died but has no Animator.", targetBotNetwork);
                }
            }
        }
    }

    // Debug
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && botNetwork != null && !isStaticBotDead)
        {
            HandleAssociatedBotDeath();
        }
    }

    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPool.Instance.PushToPool(this, gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void Init() { /* Khởi tạo nếu cần */ }

    public void OnPushToPool()
    {
        // Reset trạng thái khi trả về pool
        isStaticBotDead = false;
        model.SetActive(true);
        deathStep.SetActive(false);
        // Bất kỳ reset nào khác cần thiết
    }
}