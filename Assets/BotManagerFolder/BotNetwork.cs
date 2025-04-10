using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BotNetwork : MonoBehaviour, ITakeDamage
{
    [SerializeField] BotConfigSO botConfigSO;
    [SerializeField] Image healthBarUI;
    [SerializeField] Transform healthBarTransform;
    [SerializeField] List<Transform> _fireAssistCheckPos = new List<Transform>();
    [SerializeField] WayPoint _path;
    [SerializeField] private int _currentHealth;
    [SerializeField] private bool isImmortal;
    [SerializeField] private bool isDead;
    [SerializeField] private Transform _posTakeDame;
    public BotConfigSO BotConfigSO => botConfigSO;
    public bool IsDead => isDead;
    public int currentHealth => _currentHealth;
    public bool IsImmortal => isImmortal;
    public Action<int> OnTakeDamage { get; set; }
    public Action<int> OnLastTakeDamage { get; set; }
    public static Action<int> OnReceiverDamage { get; set; }
    public Action<string, int> OnWeaknessTakeDamage { get; set; }
    public Action<float> OnHealthChanged { get; set; }
    public Action OnBotDead { get; set; }
    public Action<BotNetwork> OnBotNetWorkDead { get; set; }
    public WayPoint Path => _path;
    public List<Transform> FireAssistCheckPos => _fireAssistCheckPos;

    private Coroutine hideHealthBarCoroutine; // Tham chiếu tới Coroutine
    public Transform mainCameraTranform;
    public Transform PosTakeDame => _posTakeDame;
    private void Awake()
    {
        if (mainCameraTranform == null)
        {
            mainCameraTranform = Camera.main.transform;
        }

        _currentHealth = botConfigSO.health;

        if (healthBarTransform != null)
        {
            healthBarTransform.gameObject.SetActive(false);
        }
        //healthBar.enabled = false; // Ẩn thanh máu khi khởi tạo
        isImmortal = false;

    }

    private void OnEnable()
    {
        OnBotDead += Die;
    }
    private void OnDisable()
    {
        OnBotDead -= Die;
    }
    public void Reset()
    {
        isDead = false;
        _currentHealth = botConfigSO.health;
        isImmortal = false;
        if (healthBarTransform != null)
        {
            healthBarTransform.gameObject.SetActive(false);
        }
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isDead) return;

        CacularHealth(damageInfo);
        if (healthBarTransform != null)
        {
            healthBarTransform.gameObject.SetActive(true);
            // Nếu đã có một Coroutine đang chờ ẩn thanh máu, hủy nó và tạo lại
            if (hideHealthBarCoroutine != null)
            {
                StopCoroutine(hideHealthBarCoroutine);
            }

            // Bắt đầu Coroutine để ẩn thanh máu sau 1 giây nếu không nhận thêm sát thương
            hideHealthBarCoroutine = StartCoroutine(HideHealthBarAfterDelay());
        }
    }
    private void CheckImmortalStatus()
    {
        if (_currentHealth <= botConfigSO.health * (botConfigSO.HealthThreshold / 100f))
        {
            isImmortal = true;

        }
        else
        {
            isImmortal = false;
        }
    }

    public void CacularHealth(DamageInfo damageInfo)
    {
        int damage = damageInfo.damage;
        if (isImmortal)
        {
            var damageScale = botConfigSO.GetDamageScale(damageInfo.damageType);

            if (damageScale > 0)
            {
                // Giảm damage theo phần trăm khi bất tử
                float reducedDamage = damageInfo.damage * damageScale;
                damage = Mathf.CeilToInt(reducedDamage); // Làm tròn lên 
            }


        }
        if (isImmortal && botConfigSO.isCanImmortal) return;
        _currentHealth -= damage;
        if (damageInfo.damageType == DamageType.Weekness)
        {
            OnWeaknessTakeDamage?.Invoke(damageInfo.name, damageInfo.damage);
        }

        OnReceiverDamage?.Invoke(damage);
        OnLastTakeDamage?.Invoke(damage);
        //Debug.Log(gameObject.name + " -" + damage.ToString() +" -" + damageInfo.damageType);
        SetHealthBar(_currentHealth);

        CheckImmortalStatus(); // Kiểm tra điều kiện bất tử
        if (_currentHealth <= 0)
        {
            isDead = true;
            OnBotDead.Invoke();
        }
        //StartCoroutine(HideHealthBarAfterDelay());
    }
    
    
    public void Die()
    {
        isDead = true;
        _currentHealth = 0;
        if (healthBarTransform == null) return;
        healthBarTransform.gameObject.SetActive(false);
        OnBotNetWorkDead?.Invoke(this);
    }
    public void SetPath(WayPoint path)
    {
        _path = path;
    }

    private void SetHealthBar(float currentHealth)
    {
        float healthBarValue = (currentHealth / botConfigSO.health);
        OnHealthChanged?.Invoke(healthBarValue);
        //healthBar.material.SetFloat("_Fill", healthBarValue);
        if (healthBarUI != null)
        {
            healthBarUI.fillAmount = healthBarValue;

        }

    }

    private IEnumerator HideHealthBarAfterDelay()
    {
        if (isDead)
        {
            healthBarTransform.gameObject.SetActive(false);
        }
        // Chờ 1 giây
        yield return new WaitForSeconds(2f);

        // Ẩn thanh máu nếu bot chưa chết
        if (!isDead)
        {
            healthBarTransform.gameObject.SetActive(false);
        }
        hideHealthBarCoroutine = null;
    }

    private void Update()
    {
        NUtiliti.AlignCamera(healthBarTransform, mainCameraTranform);
    }
}


