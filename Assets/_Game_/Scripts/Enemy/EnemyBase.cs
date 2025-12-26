using System;
using System.Collections;
using System.Security.Cryptography;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBase : GameUnit<BotType>, ITakeDamage
{
    [Header("Reference")]
    public StateControllerBase stateController;
    public BotIdentity botIdentity;
    [Range(0f, 10f)] public float PointKillCaculatorMeldal;// Điểm dùng để tính toán huy chương

    [Header("Data Bot")]
    [SerializeField] public BotConfigSO botConfigSO;
    [SerializeField] private Transform _centerBot;
    [SerializeField] protected bool runOninitInAwake;
    [SerializeField] protected int damage;
    public bool isBoss;

    [Header("Health Bar")]
    [SerializeField] protected Transform mainCameraTranform;
    [SerializeField] protected Transform healthBarTransform;
    [SerializeField] private Image healthBarUI;
    [SerializeField] protected int _currentHealth;
    [SerializeField] protected bool isImmortal;
    [SerializeField] protected bool isDead;// chết thường có thể là mất kiểm soát đối với máy bay
    [SerializeField] protected int _armor;
    [HideInInspector] public Vector3 posExplosion;// Vị trí nổ của bot, dùng để xác định hướng nổ khi chết nổ
    public bool IsDeadExplosion;// Chết nổ
    [SerializeField] private AudioPlayable bot_Audio;


    // [Header("Point To Move")]
    //[SerializeField] protected WayPoint wayPoint;
    #region Get - Set
    //public BotConfigSO BotConfigSO => botConfigSO;
    public bool IsDead => isDead;
    public int currentHealth => _currentHealth;
    public bool IsImmortal => isImmortal;
    public bool SetIsImmortal(bool _bool) => isImmortal = _bool;
    public int Damage => damage;
    public AudioPlayable BotAudio => bot_Audio;
    public int MaxHealth => botConfigSO.health;
    public int SetArmor(int Armor) => _armor = Armor;
    public int Armor => _armor;

    //public WayPoint GetWayPoint=> wayPoint;
    #endregion

    #region Actions
    public Action<bool> ACBotDeadExplosion;
    public Action<DamageInfo> ACOnTakeDamage;
    public Action<int> ACOnHealChange;
    public Action<bool> ACBotDead;
    public Action ACDeadExplosion;// Chết nổ chỉ sử dụng cho character
    #endregion

    protected Coroutine hideHealthBarCoroutine;
    #region Init - Despawn
    /// <summary>
    /// Dùng để khởi tạo thông số ban đâu của bot. và Init luôn stateController
    /// </summary>
    public virtual void OnInit()
    {
        damage = botConfigSO.damage;
        _armor = botConfigSO.armor;
        _currentHealth = botConfigSO.health;
        isImmortal = botConfigSO.isImportant;
        isDead = false;
        IsDeadExplosion = false;
    }
    // public virtual void OnInit(WayPoint _wayPoint)
    // {
    //     wayPoint = _wayPoint;
    //     TF.position = _wayPoint.WayPoints[0].transform.position;
    //     _currentHealth = botConfigSO.health;
    //     isImmortal = false;
    //     isDead = false;
    // }
    public virtual void OnDespawn(float _delay)
    {
        StartCoroutine(IEDespawn(_delay));
    }

    IEnumerator IEDespawn(float _delay)
    {
        yield return HelperCoroutine.GetWait(_delay);
        SimplePool<BotType>.Despawn(this);
    }
    #endregion

    #region Base Unity

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        stateController = GetComponent<StateControllerBase>();
        botIdentity = GetComponent<BotIdentity>();
        bot_Audio = GetComponent<AudioPlayable>();
    }
#endif

    protected virtual void Awake()
    {
#if UNITY_EDITOR
        if (botConfigSO == null)
            Debug.LogError($"{nameof(botConfigSO)} is null");
#endif
        if (mainCameraTranform == null)
            mainCameraTranform = Camera.main.transform;

        if (healthBarTransform != null)
            healthBarTransform.gameObject.SetActive(false);


        if (runOninitInAwake)
            OnInit();
    }

    protected virtual void Update()
    {
        NUtiliti.AlignCamera(healthBarTransform, mainCameraTranform);
    }
    #endregion

    #region OnTakeDamage

    public void CallActionOnTakeDamage(DamageInfo _damageInfo) => ACOnTakeDamage?.Invoke(_damageInfo);
    public virtual void OnTakeDamage(DamageInfo damageInfo)
    {

    }
    public virtual void CacularHealth(DamageInfo damageInfo)
    {
        int finalDamage = damageInfo.damage - _armor;
        if (finalDamage < 1) finalDamage = 1; // Đảm bảo
        _currentHealth -= finalDamage;
        SetHealthBar(_currentHealth);
        ACOnHealChange?.Invoke(_currentHealth);
        if (_currentHealth <= 0)
        {
            BotDead();
        }
    }
    private void SetHealthBar(float currentHealth)
    {
        float healthBarValue = (currentHealth / botConfigSO.health);
        if (healthBarUI != null)
            healthBarUI.fillAmount = healthBarValue;
    }

    protected IEnumerator IEHideHealthBarAfterDelay()
    {
        if (isDead)
            healthBarTransform.gameObject.SetActive(false);
        yield return HelperCoroutine.GetWait(2f);
        // Ẩn thanh máu nếu bot chưa chết
        if (!isDead)
            healthBarTransform.gameObject.SetActive(false);

        hideHealthBarCoroutine = null;
    }

    public virtual void BotDead()
    {
        EventManager.Instance?.Publish(new BotDeathEvent());
        //wayPoint.isUse = false;
        if (!IsDeadExplosion)
            ACBotDead?.Invoke(true);
        isDead = true;
        _currentHealth = 0;
        if (healthBarTransform != null)
            healthBarTransform.gameObject.SetActive(false);
        //SpawnBotManager.Instance.RemoveBotDead(this);
    }
    #endregion

    public Transform GetTransformThis() => TF;
    public Transform GetTransformCenter() => _centerBot;

    public virtual void ExplosionAndTakeDamageInRadius()
    {

    }

    #region Movement
    public Tween MoveToPositionDOTween(Vector3 _targetPos, float _timer, Ease _ease)
    {
        return TF.DOMove(_targetPos, _timer).SetEase(_ease);
    }

    public void RotateToPlayer()
    {
        Vector3 targetPos = GameController.Instance.GetPosLocalPlayer();
        targetPos.y = TF.position.y;
        TF.LookAt(targetPos);
    }
    public float DistanceToPlayermain() => Vector3.Distance(GameController.Instance.GetPosLocalPlayer(), TF.position);
    #endregion

    public virtual void Other(int _type)// Dùng để làm các chức năng khác ngoài các chức năng cơ bản tự định nghĩa qua type để biết gọi
    {

    }

    public virtual bool GetBool(int _NameOrType)
    {
        return false;
    }
}