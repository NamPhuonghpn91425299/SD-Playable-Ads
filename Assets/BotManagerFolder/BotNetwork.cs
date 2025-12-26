using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BotNetwork : MonoBehaviour,ITakeDamage
{
    [SerializeField] private bool isBoss;
    [SerializeField] BotConfigSO botConfigSO;
    [SerializeField] Image healthBarUI;
    [SerializeField] Transform healthBarTransform;
    [SerializeField] List<Transform> _fireAssistCheckPos = new List<Transform>();
    [SerializeField] WayPoint _path;
    [SerializeField] private int _currentHealth;
    [SerializeField] private bool isImmortal;
    public bool BatTu;
    [SerializeField] private bool isDead;
    
    [Header("Change Anim")]
    [SerializeField] private string currentAnimName;

    [SerializeField] private Animator anim;
    
    public BotConfigSO BotConfigSO => botConfigSO;
    public bool IsDead => isDead;
    public int currentHealth => _currentHealth;
    public bool IsImmortal => isImmortal;
    public Action<int> OnTakeDamage { get; set; }
    
    public Action<int> OnTakeDamagePlayer { get; set; }
    public Action<string,int> OnWeaknessTakeDamage { get; set; }

    public Action<float> OnHealthChanged { get; set; }
    public Action OnBotDead { get; set; }
    public Action<BotNetwork> OnBotNetWorkDead { get; set; }
    public WayPoint Path => _path;
    public List<Transform> FireAssistCheckPos=> _fireAssistCheckPos;

    private Coroutine hideHealthBarCoroutine; // Tham chiếu tới Coroutine
    public Transform mainCameraTranform;

    public ParticleSystem vfxGiatDien;
    
    [Header("Add Explosion death")]
    [SerializeField] botZomNorsuit botZomNorsuit;

    public Vector3 posExplosion;

    public bool DeadExplosion;
    
    [Header("Giật điện lan rộng")]
    [SerializeField] private float _radius = 1f;
    [SerializeField] private Transform posCenter;
    [SerializeField] LayerMask _layerMaskBot;
    [SerializeField] GameObject _lightingSettings;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (posCenter != null)
            Gizmos.DrawWireSphere(posCenter.position, _radius);
    }

    public void GetBotBiGiatDienTheo(int _dame)
    {
        Collider[] cols = Physics.OverlapSphere(posCenter.position, _radius, _layerMaskBot);
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
            var takeDamageController = elem.gameObject.GetComponentInParent<BotNetwork>();
            var damageType = elem.CompareTag("WeakPoint") ? DamageType.Weakness : DamageType.Normal;

            if(takeDamageController != null)
            {
                if (Random.Range(0, 50) % 2 == 0)
                {
                    LightningBeamEffect lightingEff = ObjectPool.Instance.PopFromPool(_lightingSettings, instantiateIfNone: true).GetComponent<LightningBeamEffect>();
                    lightingEff.Init(posCenter.position, takeDamageController.posCenter.position);
                }
                var damageInfo = new DamageInfo()
                {
                    damageType = damageType,
                    damage = _dame,
                    name = elem.gameObject.name,
                };
                
                takeDamageController.TakeDamageDienGiat(damageInfo);
            }
        }
    }
    
    private void Awake()
    {
        if (mainCameraTranform == null)
        {
            mainCameraTranform = Camera.main.transform;
        }
        OnBotDead+= Die;

    }

    private void OnEnable()
    {
        _currentHealth = botConfigSO.health;
        if (healthBarTransform != null)
        {
             healthBarTransform.gameObject.SetActive(false); 
        }    
        //healthBar.enabled = false; // Ẩn thanh máu khi khởi tạo
        isImmortal = false;
        isDead = false;
        DeadExplosion = false;
        
    }

    public void TakeDamage(DamageInfo damageInfo)
    {   
        if(isDead) return;
        if(BatTu) return;
        
        if (damageInfo.damageType == DamageType.Explosion && !isBoss)
        {
            DeadExplosion = true;
        }
        
        if (GamePlayManager.Instance.CanPlayEffectGiatDien && damageInfo.damageType != DamageType.Explosion)
        {
            if (vfxGiatDien != null && posCenter != null)
            {
            	vfxGiatDien.Play();
                GetBotBiGiatDienTheo((int)((float)damageInfo.damage*(70f/100f)));
             	
			}
        }
        
        OnTakeDamagePlayer?.Invoke(damageInfo.damage);
        
        CacularHealth(damageInfo);
        
        if(healthBarTransform != null && damageInfo.damageType != DamageType.Explosion||healthBarTransform != null && isBoss && damageInfo.damageType == DamageType.Explosion)
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
    
    public void TakeDamageDienGiat(DamageInfo damageInfo)
    {   
        if(isDead) return;
        if(BatTu) return;
        
        if (damageInfo.damageType == DamageType.Explosion && !isBoss)
        {
            DeadExplosion = true;
        }
        
        if(GamePlayManager.Instance.CanPlayEffectGiatDien && damageInfo.damageType != DamageType.Explosion)
            vfxGiatDien.Play();
        
        OnTakeDamagePlayer?.Invoke(damageInfo.damage);
        
        CacularHealth(damageInfo);
        
        if(healthBarTransform != null)
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
        var damageScale = botConfigSO.GetDamageScale(damageInfo.damageType);
        if (isImmortal)
        {

            if (damageScale > 0)
            {
                // Giảm damage theo phần trăm khi bất tử
                float reducedDamage = damageInfo.damage * damageScale;
                damage = Mathf.CeilToInt(reducedDamage); // Làm tròn lên 
            }


        }
        
        if (isImmortal && botConfigSO.isCanImmortal) return;
        if(damageInfo.damageType == DamageType.Weakness)
        {
            float reducedDamage = damageInfo.damage * damageScale;
            damage = Mathf.CeilToInt(reducedDamage); // Làm tròn lên 
            OnWeaknessTakeDamage?.Invoke(damageInfo.name, damage);
            //Debug.Log($"Bot {gameObject.name} bị yếu điểm: {damageInfo.name} - HP Bot Giảm: {damage} - HP Hiện Tại: {_currentHealth} {damageInfo.damage}");
        }
        _currentHealth -= damage;
        //Debug.Log(gameObject.name + " -" + damage.ToString());
        SetHealthBar(_currentHealth);
        

        CheckImmortalStatus(); // Kiểm tra điều kiện bất tử
        if (_currentHealth <= 0 && !DeadExplosion)
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

    public void ChangeAnim(string _name)
    {
        if (anim == null)
        {
            Debug.LogError("Null anim");
            return;
        }
        
        if (currentAnimName != _name)
        {
            anim.ResetTrigger(_name);
            currentAnimName = _name;
            anim.SetTrigger(currentAnimName);
        }
    }
}


