using System.Collections;
using UnityEngine;

public class WeaknessExplosionController : MonoBehaviour
{
    [Header("Weakness Settings")]
    [SerializeField] private Transform weaknessPoint; // Điểm yếu duy nhất
    [SerializeField] private int explosionThreshold = 400; // Damage cần để vỡ điểm yếu
    
    [Header("Scale Animation")]
    [SerializeField] private float maxScale = 1.5f; // Scale tối đa khi sắp vỗ
    [SerializeField] private float scaleSpeed = 3f; // Tốc độ scale
    [SerializeField] private bool enableScaleAnimation = true; // Có animate scale không

    [Header("Effects")] 
    [SerializeField] private Transform bakeRightHandParent; // Parent của bakeRightHand
    [SerializeField] private GameObject bakeRightHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private ParticleSystem explosionVFX;
    [SerializeField] private AudioSource explosionSound;
    [SerializeField] private bool instantKillOnExplosion = true; // Giết bot ngay khi vỡ điểm yếu
    
    // Thông tin điểm yếu
    private int currentDamage = 0;
    private Vector3 originalScale;
    private bool isExploded = false;
    
    private BotNetwork botNetwork;
    
    private void Awake()
    {
        botNetwork = GetComponent<BotNetwork>();
        
        // Lưu scale gốc của điểm yếu
        if (weaknessPoint != null)
        {
            originalScale = weaknessPoint.localScale;
        }
        else
        {
            Debug.LogWarning($"Chưa gán Weakness Point cho bot {gameObject.name}");
        }
    }
    
    private void OnEnable()
    {
        if (botNetwork != null)
        {
            botNetwork.OnWeaknessTakeDamage += HandleWeaknessDamage;
        }
        
        // Reset khi bot respawn
        ResetWeakness();
    }
    
    private void OnDisable()
    {
        if (botNetwork != null)
        {
            botNetwork.OnWeaknessTakeDamage -= HandleWeaknessDamage;
        }
    }
    
    private void Update()
    {
        // Animate scale nếu được bật
        if (enableScaleAnimation && !isExploded)
        {
            UpdateWeaknessScale();
        }
    }
    
    // Xử lý khi điểm yếu nhận damage
    private void HandleWeaknessDamage(string weaknessName, int damage)
    {
        if (isExploded) return; // Đã nổ rồi thì không xử lý nữa
        
        // Cộng damage
        currentDamage += damage;
        
        Debug.Log($"Điểm yếu nhận {damage} damage. Tổng: {currentDamage}/{explosionThreshold}");
        
        // Kiểm tra có đủ để vỡ không
        if (currentDamage >= explosionThreshold)
        {
            ExplodeWeakness();
        }
    }
    
    // Cập nhật scale của điểm yếu
    private void UpdateWeaknessScale()
    {
        if (weaknessPoint == null) return;
        
        // Tính scale dựa trên damage hiện tại
        float damagePercent = (float)currentDamage / explosionThreshold;
        damagePercent = Mathf.Clamp01(damagePercent);
        
        // Scale từ 1.0 đến maxScale
        float targetScale = Mathf.Lerp(1f, maxScale, damagePercent);
        Vector3 targetScaleVector = originalScale * targetScale;
        
        // Lerp smooth đến target scale
        weaknessPoint.localScale = Vector3.Lerp(
            weaknessPoint.localScale, 
            targetScaleVector, 
            Time.deltaTime * scaleSpeed
        );
    }
    
    // Vỡ điểm yếu
    private void ExplodeWeakness()
    {
        if (isExploded) return;
        
        isExploded = true;
        
        Debug.Log($"💥 Điểm yếu đã vỡ! Bot {gameObject.name} sẽ chết!");
        
        // Phát hiệu ứng nổ
        PlayExplosionEffects();
        
        // Giết bot ngay lập tức
        if (instantKillOnExplosion)
        {
            KillBotInstantly();
        }
        
        // Ẩn điểm yếu
        if (weaknessPoint != null)
        {
            rightHand.SetActive(false);
            //bakeRightHand.SetActive(true);
            GameObject bakeHand = ObjectPool.Instance.PopFromPool(bakeRightHand, instantiateIfNone: true);
            
            bakeHand.transform.SetPositionAndRotation(bakeRightHandParent.position, bakeRightHandParent.rotation);
            
            bakeHand.SetActive(true);
            weaknessPoint.gameObject.SetActive(false);
        }
    }
    
    // Giết bot ngay lập tức
    private void KillBotInstantly()
    {
        if (botNetwork != null && !botNetwork.IsDead)
        {
            
            // Hoặc gọi trực tiếp logic chết
            var killDamage = new DamageInfo()
            {
                damageType = DamageType.Normal,
                damage = 99999, // Damage cực lớn để chắc chắn chết
                name = "WeaknessExplosion"
            };
            
            botNetwork.CacularHealth(killDamage);
            
            Debug.Log($"Bot {gameObject.name} đã chết do điểm yếu vỡ!");
        }
    }
    
    // Phát hiệu ứng nổ
    private void PlayExplosionEffects()
    {
        Vector3 explosionPosition = weaknessPoint != null ? weaknessPoint.position : transform.position;
        
        // Particle effect
        if (explosionVFX != null)
        {
            explosionVFX.transform.position = explosionPosition;
            explosionVFX.Play();
        }
        
        // Sound effect
        if (explosionSound != null)
        {
            explosionSound.Play();
        }
        
    }
    
    // Reset điểm yếu về trạng thái ban đầu
    public void ResetWeakness()
    {
        currentDamage = 0;
        isExploded = false;
        
        if (weaknessPoint != null)
        {
            weaknessPoint.localScale = originalScale;
            weaknessPoint.gameObject.SetActive(true);
        }
        rightHand.SetActive(true);
        bakeRightHand.SetActive(false);
        Debug.Log($"Đã reset điểm yếu cho bot {gameObject.name}");
    }
    
    // === GETTER METHODS ===
    
    // Lấy % damage hiện tại
    public float GetDamagePercent()
    {
        return (float)currentDamage / explosionThreshold * 100f;
    }
    
    // Lấy damage còn thiếu để vỡ
    public int GetRemainingDamage()
    {
        return Mathf.Max(0, explosionThreshold - currentDamage);
    }
    
    // Kiểm tra đã vỡ chưa
    public bool IsExploded()
    {
        return isExploded;
    }
    
    // Set ngưỡng vỡ từ bên ngoài
    public void SetExplosionThreshold(int threshold)
    {
        explosionThreshold = threshold;
        Debug.Log($"Ngưỡng vỡ điểm yếu: {explosionThreshold}");
    }
}