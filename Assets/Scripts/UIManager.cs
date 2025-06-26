using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using static HelperCoroutine;
using static ParameterManagers;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject InGame;
    public GameObject canvasRocket;
    public GameObject HUD;
    public GameObject playerHealth;
    public GameObject reloadButton;
    public GameObject dowloadButton;
    public Text TotalBotText;
    [FormerlySerializedAs("initBot")] public int TotalBotinConfig;
    public UIAnimSimulator uIAnimSimulator;
    public Image process;
    public GameObject gameProcess;
    public Button tapToPlay;
    public Text bulletCountText; // Text UI để hiển thị số lượng đạn
    public Text RoundTxt; // Text UI để hiển thị số lượng đạn
    public Text RoundTxtShadow; // Text UI để hiển thị số lượng đạn
    public CanvasGroup hudCanvasGroup;
    public Text timeToEndGameTxt;
    public float timeEndConfig = 30f;
    public Action<float> OnTimeout;
    private bool timeOut;
    [Header("Component Hide On Attack")]
    [SerializeField] private GameObject _rocketBtn;
    [SerializeField] private GameObject _crossHair;
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private GameObject _switchRocketBtn;
    
    [Header("Effect Add rapidFire")]
    [SerializeField] GameObject _rapidFireEffect;
    [SerializeField] GameObject _ChangeGunEffecct;

    [Header("Check Portrait")] 
    public GameObject btnRocket;
    public RectTransform rectBtnRocket;
    public RectTransform btnReplay;
    public GameObject rocket;

    public void PlayEffectChangeGun() => _ChangeGunEffecct.SetActive(true);
    public void PlayEffectRapidFire() => _rapidFireEffect.SetActive(true);
    [SerializeField] private float _timeDelayShowHUD = 3f;
    public float _timeEnd;
    public bool isLoseGame;
    public bool isWinGame;
    public bool isCanTouch;
    public bool isStopGame;
    private void Awake()
    {
        if (Screen.width < Screen.height)
        {
            Debug.Log("Màn hình đang ở chế độ dọc (Portrait)");
            rectBtnRocket.anchoredPosition = new Vector2(-160f, rectBtnRocket.anchoredPosition.y);
            rocket.transform.localPosition = new Vector3(0.405000001f, 0.02f, 1.218f);
            btnReplay.anchoredPosition = new Vector2(0f, 795f);
        }
        
        Instance = this;
        InGame.SetActive(true);
        gameProcess.SetActive(true);
        playerHealth.SetActive(true);
        if (reloadButton) reloadButton.SetActive(true);
        if (uIAnimSimulator) uIAnimSimulator.StartAnimateTextAppear();

    }

    private void Start()
    {
        tapToPlay.gameObject.SetActive(true);
        btnRocket.SetActive(false);
        Time.timeScale = 0f;
        tapToPlay.onClick.AddListener(OnButtonClick);
    }
    public void SetTimeToEndGame()
    {
        if (timeToEndGameTxt != null)
        {
            timeEndConfig = Mathf.Max(0, timeEndConfig - Time.deltaTime);
            timeToEndGameTxt.text = $"TIME TO DEFENSE: {timeEndConfig:F0}s";
            if (!timeOut && timeEndConfig <= 0)
            {
                timeOut = true;
                EventManager.Invoke<float>(EventName.OnTimeOut, timeEndConfig);
            }
            //yield return null;
        }
    }
    private void OnEnable()
    {

        EventManager.AddListener<int>(EventName.UpdateBulletCount, UpdateBulletCount);
        EventManager.AddListener<int>(EventName.OnCheckTurnPlay, OnCheckTurnPlay);
        EventManager.AddListener<bool>(EventName.OnShowLunaEndGame, OnShowLunaEndGame);
        EventManager.AddListener<bool>(EventName.OnPlayerDead, OnPlayerDead);
        EventManager.AddListener<float>(EventName.OnTimeOut, OnTimeOut);
        EventManager.AddListener<bool>(EventName.OnCameraFollowRocket, OnCameraFollowRocket);
    }


    private void OnDisable()
    {
        EventManager.RemoveListener<int>(EventName.UpdateBulletCount, UpdateBulletCount);
        EventManager.RemoveListener<int>(EventName.OnCheckTurnPlay, OnCheckTurnPlay);
        EventManager.RemoveListener<bool>(EventName.OnShowLunaEndGame, OnShowLunaEndGame);
        EventManager.RemoveListener<bool>(EventName.OnPlayerDead, OnPlayerDead);
        EventManager.RemoveListener<float>(EventName.OnTimeOut, OnTimeOut);
        EventManager.RemoveListener<bool>(EventName.OnCameraFollowRocket, OnCameraFollowRocket);
        tapToPlay.onClick.RemoveAllListeners();
    }

    private void OnCheckTurnPlay(int Turn)
    {
        if (Instance == null)
        {
            Debug.LogError("UIManager instance is null");
            return;
        }

        if (UIEndGame.Instance == null)
        {
            Debug.LogError("UIEndGame instance is null");
            return;
        }
        if (!UIEndGame.Instance.IsShowEndGame)
        {
            int Round = Turn + 1;
            RoundTxt.text = "ROUND " + Round;
            RoundTxtShadow.text = "ROUND " + Round;
            uIAnimSimulator.StartAnimateTextAppear();
            if (Round >= 2)
            {
                dowloadButton.gameObject.SetActive(true);
            }
            else
            {
                dowloadButton.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerExit()
    {
        IsIngameGUI = false; // Thoát khỏi UI
    }
    public void OnButtonClick()
    {
        OnTapToPlay();
    }
    public void OnTapToPlay()
    {
        Time.timeScale = 1f;
        btnRocket.SetActive(true);
        tapToPlay.gameObject.SetActive(false);
        StartCoroutine(FadeIn());
        OnPointerExit();
        Luna.Unity.LifeCycle.GameStarted();
        Luna.Unity.Analytics.LogEvent(Luna.Unity.Analytics.EventType.TutorialComplete);
        Debug.Log($"{nameof(OnTapToPlay)}");
    }

    IEnumerator ShowEndCard()
    {

        yield return WaitSeconds(1f);
        Time.timeScale = 0;
        InGame.SetActive(false);
        gameProcess.SetActive(false);
    }

    public void EndGameUI()
    {
        StartCoroutine(ShowEndCard());
    }

    private void OnShowLunaEndGame(bool IsShow)
    {
        if (IsShow)
        {
            //StartCoroutine(DelayHideHUD());
        }
    }
    private void OnPlayerDead(bool isPlayerDead)
    {
        if (isPlayerDead)
        {
            isLoseGame = true;
            playerHealth.SetActive(false);
            reloadButton.SetActive(false);
            StartCoroutine(PlayerDeadUI());
            EventManager.Invoke(EventName.OnGameLost, isPlayerDead);
            //this.RunOnSeconds(1f, () => hudCanvasGroup.alpha -= Time.deltaTime);
        }

    }
    public void EndGame()
    {
        StartCoroutine(DelayHideHUD());
    }
    private IEnumerator DelayHideHUD()
    {
        isWinGame = true;
        UIEndGame.Instance.IsShowEndGame = true;
        if (RoundTxtShadow) RoundTxtShadow.text = "VICTORY!";
        RoundTxt.text = "VICTORY!";
        uIAnimSimulator.StartAnimateTextAppear();
        yield return WaitSeconds(_timeDelayShowHUD);
        HUD.SetActive(false);
        canvasRocket.SetActive(false);
        uIAnimSimulator.StartAnimateTextAppear();
        yield return null;
        UIEndGame.Instance.ShowUIEndGame();
        uIAnimSimulator.ShowUIEndGameWin();
        isCanTouch = true;

    }
    private IEnumerator PlayerDeadUI()
    {
        isLoseGame = true;
        UIEndGame.Instance.IsShowEndGame = true;
        yield return WaitSeconds(1f);
        if (RoundTxtShadow) RoundTxtShadow.text = "GAME OVER!";
        RoundTxt.text = "GAME OVER!";
        uIAnimSimulator.StartAnimateTextAppear();
        yield return WaitSeconds(_timeDelayShowHUD);
        HUD.SetActive(false);
        canvasRocket.SetActive(false);
        uIAnimSimulator.StartAnimateTextAppear();
        yield return null;
        UIEndGame.Instance.ShowEndGameLose();
        uIAnimSimulator.StartShowUIEndGame();
        isCanTouch = true;
    }
    private void OnTimeOut(float timer)
    {
        if (timer <= 0 && !UIEndGame.Instance.IsShowEndGame)
        {
            StartCoroutine(PlayerDeadUI());
        }
    }

    private void StopGame()
    {
        if (isCanTouch && !isStopGame)
        {
            _timeEnd = Mathf.Max(0, _timeEnd - Time.deltaTime);
            if (_timeEnd <= 0)
            {
                Time.timeScale = 0;
                isStopGame = true;
            }
        }
    }
    void Update()
    {
        //SetTimeToEndGame();
        StopGame();
        TotalBotText.text = $"{GameResultInstance.Instance.GetGameResultData().BotKillCount} / {TotalBotinConfig}";
        process.fillAmount = ((float)(GameResultInstance.Instance.GetGameResultData().BotKillCount) / TotalBotinConfig);
    }



    public void UpdateInitBot(int value)
    {
        TotalBotinConfig = value;
    }

    public void UpdateBulletCount(int bulletCount)
    {
        bulletCountText.text = "Bullet Count: " + bulletCount;
    }

    private IEnumerator FadeIn()
    {
        CanvasGroup canvasGroup = HUD.GetComponent<CanvasGroup>();
        float timeElapsed = 0;
        float duration = 1f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, timeElapsed / duration);
            yield return null;
        }
    }
    
    private void OnCameraFollowRocket(bool isOn)
    {
        _rocketBtn.SetActive(!isOn);
        _crossHair.SetActive(!isOn);
        _healthBar.SetActive(!isOn);
        _switchRocketBtn.SetActive(!isOn);
    }
}
