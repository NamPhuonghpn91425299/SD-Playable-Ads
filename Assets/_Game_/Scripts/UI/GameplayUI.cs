using System;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : UIBase,
Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<GameDataChangedEvent>,
Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<PlayerHealthChangedEvent>,
Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<BotDeathEvent>,
Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<BossSpawnEvent>
{
    [SerializeField] private Text _bulletText;
    [SerializeField] private Text _rocketText;
    [SerializeField] private Text _enemyRemainText;
    [SerializeField] private Text _roundText;
    [SerializeField] private Text _timerText;
    [SerializeField] private Button _aimButton;
    [SerializeField] private Button _reloadButton;
    [SerializeField] private GameObject _infinityImage;
    [SerializeField] private GameObject[] _infinityImageDisableGameobject;

    [SerializeField] private float _timerCountdown = 60f; // Example countdown duration
    [SerializeField] private int _botDeathToActiveAchievement = 2; // Example threshold for bot deaths to trigger achievement
    [SerializeField] private float _achievementTimeWindow = 40f; // Time window to achieve the kill count

    private float _currentTime;
    private int _killedBotsForRound = 0; // Counter for killed bots in the current round
    private float _achievementTimer = 0f; // Timer for achievement window
    private bool _achievementWindowActive = false; // Flag to track if achievement window is active
    private int _currentBotDeathCount = 0;

    private List<IVfx> _vfxList;

    [SerializeField] private int[] _achievementCounters;

    private int                       _prevRoundIndex  = -1;
    private int                       _prevRoundsCount = -1;
    private int                       _prevEnemyRemain = int.MinValue;
    private int                       _prevTimerInt    = int.MinValue;
    private System.Text.StringBuilder _sb; // tránh alloc khi ghép "a/b"
    void Awake()
    {
        this._aimButton.onClick.AddListener(AimAction);
        this._reloadButton.onClick.AddListener(ReloadAction);
    }

    private void ReloadAction()
    {
        // Gọi OnReload() của weapon hiện tại
        WeaponBase currentWeapon = GameController.Instance?.CurrentWeapon;
        if (currentWeapon != null && currentWeapon is ReloadableWeapons reloadableWeapon)
        {
            reloadableWeapon.OnReload_Corountine();
            Debug.Log("Reload button pressed - calling OnReload()");
        }
        else
        {
            Debug.LogWarning("Current weapon is null or not reloadable");
        }
    }

    void Start()
    {
        _vfxList = new List<IVfx>(GetComponentsInChildren<IVfx>(true));
        //        Debug.Log($"Found {_vfxList.Count} VFX components in GameplayUI.");

        EventManager.Instance?.Subscribe<GameDataChangedEvent>(this);
        EventManager.Instance?.Subscribe<PlayerHealthChangedEvent>(this);
        EventManager.Instance?.Subscribe<BotDeathEvent>(this);
        EventManager.Instance?.Subscribe<BossSpawnEvent>(this);

        // Initialize timers
        _currentTime = _timerCountdown;
        _achievementTimer = _achievementTimeWindow;
        
        _currentTime      = _timerCountdown;
        _achievementTimer = _achievementTimeWindow;

        _sb = new System.Text.StringBuilder(16);
    }

    void Update()
    {
        // _roundText.text = $"{GameManager.Instance?.currentRoundIndex + 1}/{GameManager.Instance?.levelRounds?.Count ?? 0}";
        // _enemyRemainText.text = $"{GameManager.Instance?.totalBotsForRound - GameManager.Instance?.killedBotsForRound}";
        var gm          = GameManager.Instance;
        int roundIndex  = (gm?.currentRoundIndex ?? -1) + 1;
        int roundsCount = gm?.levelRounds?.Count ?? 0;
        int enemyRemain = (gm?.totalBotsForRound ?? 0) - (gm?.killedBotsForRound ?? 0);

        // Update round text only when changed
        if ((_roundText != null) && (roundIndex != _prevRoundIndex || roundsCount != _prevRoundsCount))
        {
            _sb.Clear();
            _sb.Append(roundIndex);
            _sb.Append('/');
            _sb.Append(roundsCount);
            _roundText.text  = _sb.ToString();
            _prevRoundIndex  = roundIndex;
            _prevRoundsCount = roundsCount;
        }

        // Update enemy remain only when changed
        if ((_enemyRemainText != null) && enemyRemain != _prevEnemyRemain)
        {
            _enemyRemainText.text = enemyRemain.ToString();
            _prevEnemyRemain      = enemyRemain;
        }
        // Main countdown timer
        if (_currentTime > 0)
        {
            _currentTime -= Time.deltaTime;
            if (_timerText != null)
                _timerText.text = Mathf.Ceil(_currentTime).ToString();
        }

        // Achievement window timer
        if (_achievementWindowActive)
        {
            _achievementTimer -= Time.deltaTime;

            if (_achievementTimer <= 0)
            {
                // Time window expired, reset counter
                _killedBotsForRound = 0;
                _achievementWindowActive = false;
                _achievementTimer = _achievementTimeWindow;
            }
        }
    }



    void OnDisable()
    {
        EventManager.Instance?.Unsubscribe<GameDataChangedEvent>(this);
        EventManager.Instance?.Unsubscribe<PlayerHealthChangedEvent>(this);
        EventManager.Instance?.Unsubscribe<BotDeathEvent>(this);
        EventManager.Instance?.Unsubscribe<BossSpawnEvent>(this);
        _aimButton.onClick.RemoveListener(AimAction);
        _reloadButton.onClick.RemoveListener(ReloadAction);
    }

    void OnDestroy()
    {
        // Đảm bảo unsubscribe khi object bị destroy
        EventManager.Instance?.Unsubscribe<GameDataChangedEvent>(this);
        EventManager.Instance?.Unsubscribe<PlayerHealthChangedEvent>(this);
        EventManager.Instance?.Unsubscribe<BotDeathEvent>(this);
        EventManager.Instance?.Unsubscribe<BossSpawnEvent>(this);
    }



    public void OnNotify(GameDataChangedEvent data)
    {
        if (data == null)
        {
            Debug.LogWarning("GameDataChangedEvent data is null");
            return;
        }

        try
        {
            if (data.BulletRemaning.HasValue && _bulletText != null)
                _bulletText.text = data.BulletRemaning.Value.ToString();
            if (data.RocketRemaning.HasValue && _rocketText != null)
                _rocketText.text = data.RocketRemaning.Value.ToString();
            if (data.EmemyRemaning.HasValue && _enemyRemainText != null)
                _enemyRemainText.text = data.EmemyRemaning.Value.ToString();
            if (data.isInfinityBullet)
            {
                _infinityImage.SetActive(true);
                foreach (GameObject VARIABLE in _infinityImageDisableGameobject)
                    VARIABLE.SetActive(false);
                _bulletText.gameObject.SetActive(false);
                Debug.Log("GameplayUI: Infinity bullets enabled");
            }


            if (data.ReloadTime.HasValue)
                PlayReloadVFX(data.ReloadTime.Value);
            if (!string.IsNullOrEmpty(data.hitEnemy))
                PlayShootingVFX(data.hitEnemy);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in GameplayUI.OnNotify: {ex.Message}");
        }
    }

    private void AimAction()
    {
        ExecuteVFX<AimUIVfx>(vfx => vfx.Play(true)); // Toggle aiming state
    }

    // // Action-based approach - no boxing, type-safe
    // public void PlayAchievementVFX(GameConstants.AchievementAnimationParameter parameter)
    // {
    //     ExecuteVFX<MedalsUI>(vfx => vfx.Play(parameter));
    // }

    public void PlayDamageVFX(float GetSpeed)
    {
        ExecuteVFX<TakeDamageUIVfx>(vfx => vfx.Play(GetSpeed));
    }

    public void PlayShootingVFX(string name)
    {
        ExecuteVFX<ShootUIVfx>(vfx => vfx.Play(name));
    }

    public void PlayReloadVFX(float speed = 2f)
    {
        ExecuteVFX<ReloadUIVfx>(vfx => vfx.Play(speed));
    }

    public void PlayHealVFX(string name)
    {
        ExecuteVFX<HealUIVfx>(vfx => vfx.Play(name));
    }

    public void PlaySpawnBossVFX(float duration)
    {
        ExecuteVFX<SpawnBossUIVfx>(vfx => vfx.Play(duration));
    }

    public void PlayHealthbarVFX(int currentHealth, int maxHealth)
    {

        ExecuteVFX<HealthbarUI>(vfx => vfx.Play((currentHealth, maxHealth)));
    }

    public void PlayNextRoundVFX()
    {
        ExecuteVFX<NextRound>(vfx => vfx.Play(1.5f));
    }



    // Generic executor method
    private void ExecuteVFX<T>(Action<T> action) where T : class, IVfx
    {
        foreach (var vfx in _vfxList)
        {
            if (vfx is T typedVfx)
            {
                var vfxComponent = vfx as Component;
                if (vfxComponent != null && vfxComponent.gameObject.activeInHierarchy == false)
                {
                    vfxComponent.gameObject.SetActive(true);
                    // Wait one frame for Start() to complete before calling action
                    StartCoroutine(DelayedAction(() => action(typedVfx)));
                }
                else
                {
                    action(typedVfx);
                }
            }
        }
    }

    private System.Collections.IEnumerator DelayedAction(System.Action action)
    {
        yield return null; // Wait one frame
        action?.Invoke();
    }

    public void OnNotify(PlayerHealthChangedEvent data)
    {
        if (data.State == "Damaged")
        {
            int maxHealth = data.MaxHealth ?? 0;
            int currentHealth = data.CurrentHealth ?? 0;
            ValueTuple<int, int> healthData = (maxHealth, currentHealth);
            PlayHealthbarVFX(healthData.Item1, healthData.Item2);
            ExecuteVFX<TakeDamageUIVfx>(vfx => vfx.Play(1f));
        }
        else if (data.State == "Healed")
        {
            int maxHealth = data.MaxHealth ?? 0;
            int currentHealth = data.CurrentHealth ?? 0;
            ValueTuple<int, int> healthData = (maxHealth, currentHealth);
            PlayHealthbarVFX(healthData.Item1, healthData.Item2);
        }
    }



    public void OnNotify(BotDeathEvent data)
    {
        _killedBotsForRound++;
        _currentBotDeathCount++;

        for (int i = 0; i < _achievementCounters.Length; i++)
        {
            if (_currentBotDeathCount == _achievementCounters[i])
            {
                GameConstants.AchievementAnimationParameter parameter;
                switch (i)
                {
                    case 0:
                        parameter = GameConstants.AchievementAnimationParameter.Killmark_center_2;
                        _killedBotsForRound = 0;
                        break;
                    case 1:
                        parameter = GameConstants.AchievementAnimationParameter.Killmark_center_3;
                        _killedBotsForRound = 0;
                        break;
                    case 2:
                        parameter = GameConstants.AchievementAnimationParameter.Killmark_center_4;
                        _killedBotsForRound = 0;
                        break;
                    case 3:
                        parameter = GameConstants.AchievementAnimationParameter.Killmark_center_5;
                        _killedBotsForRound = 0;
                        break;
                    default:
                        parameter = GameConstants.AchievementAnimationParameter.None;
                        break;
                }
                //PlayAchievementVFX(parameter);
                EventManager.Instance?.Publish(
                    new AchievementUnlockedEvent(
                        (GameConstants.AchievementType)(i + 1),
                        $"Killmark {i + 1} activated!"
                    )
                );
                break; // Exit loop after finding the first match
            }
            else
            {
                // Start achievement window on first kill
                if (!_achievementWindowActive)
                {
                    _achievementWindowActive = true;
                    _achievementTimer = _achievementTimeWindow;
                }

                // Check if the number of killed bots meets the threshold within time window
                if (_killedBotsForRound >= _botDeathToActiveAchievement)
                {
                    //(GameConstants.AchievementAnimationParameter.Killmark_center_1);
                    EventManager.Instance?.Publish(
                        new AchievementUnlockedEvent(
                            GameConstants.AchievementType.Killmark1,
                            "Killmark activated!"
                        )
                    );
                    _killedBotsForRound = 0;
                    _achievementWindowActive = false;
                    _achievementTimer = _achievementTimeWindow;
                }

            }
        }


    }

    public void OnNotify(ChangeProjectileGunEvent data)
    {


    }

    public void OnNotify(BossSpawnEvent data)
    {
        if (data.Trigger)
        {
            PlaySpawnBossVFX(.7f);
        }

    }
}

