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
    public GameObject HUD;
    public GameObject playerHealth;
    public GameObject reloadButton;
    public GameObject dowloadButton;
    public Text TotalBotText;
    [FormerlySerializedAs("initBot")] public int TotalBotinConfig;
    public UIAnimSimulator uIAnimSimulator;
    public Image process;
    public Image processTimer;
    public GameObject gameProcess;
    public Button tapToPlay;
    public Text bulletCountText; // Text UI để hiển thị số lượng đạn
    public Text RoundTxt; // Text UI để hiển thị số lượng đạn
    public Text RoundTxtShadow; // Text UI để hiển thị số lượng đạn
    public CanvasGroup hudCanvasGroup;
    public Text timeToEndGameTxt;
    public float timeEndConfig = 30f;
    private float totalTime;
    public float _timeEnd;
    public Action<float> OnTimeout;
    private bool timeOut;
    [SerializeField] private float _timeDelayShowHUD = 3f;
    private float timer;
    private bool _isRocketFollow = false;
    private bool _isPlayerDead;
    public bool isLoseGame;
    public bool isWinGame;
    public bool isCanTouch;
    public bool isStopGame;

    private void Awake()
    {
        Instance = this;
        InGame.SetActive(true);
        gameProcess.SetActive(true);
        playerHealth.SetActive(true);
        tapToPlay.gameObject.SetActive(true);
        if (reloadButton) reloadButton.SetActive(true);
        if (uIAnimSimulator) uIAnimSimulator.StartAnimateTextAppear();

    }
    private void Start()
    {
        totalTime = timeEndConfig;
        Time.timeScale = 0f;
        tapToPlay.onClick.AddListener(OnButtonClick);
    }
    public void SetTimeToEndGame()
    {
        if (timeToEndGameTxt != null && !UIEndGame.Instance.IsShowEndGame)
        {
            timeEndConfig = Mathf.Max(0, timeEndConfig - Time.deltaTime);
            timeToEndGameTxt.text = $"TIME TO DEFENSE: {timeEndConfig:F0}s";
            processTimer.fillAmount = Mathf.Clamp01(timeEndConfig / totalTime);
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
    }


    private void OnDisable()
    {
        EventManager.RemoveListener<int>(EventName.UpdateBulletCount, UpdateBulletCount);
        EventManager.RemoveListener<int>(EventName.OnCheckTurnPlay, OnCheckTurnPlay);
        EventManager.RemoveListener<bool>(EventName.OnShowLunaEndGame, OnShowLunaEndGame);
        EventManager.RemoveListener<bool>(EventName.OnPlayerDead, OnPlayerDead);
        EventManager.RemoveListener<float>(EventName.OnTimeOut, OnTimeOut);
        tapToPlay.onClick.RemoveAllListeners();
    }

    private void OnCheckTurnPlay(int Turn)
    {
        if (!UIEndGame.Instance.IsShowEndGame)
        {
            int Round = Turn + 1;
            if (RoundTxtShadow) RoundTxtShadow.text = "START!";// + Round;
            RoundTxt.text = "START!";// + Round;
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
        tapToPlay.gameObject.SetActive(false);
        StartCoroutine(FadeIn());
        OnPointerExit();
        Luna.Unity.LifeCycle.GameStarted();
        Luna.Unity.Analytics.LogEvent(Luna.Unity.Analytics.EventType.TutorialComplete);
        Debug.Log($"{nameof(OnTapToPlay)}");
    }

    IEnumerator ShowEndCard()
    {

        yield return WaitSeconds(1);
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
        isPlayerDead = _isPlayerDead;
        if (isPlayerDead)
        {
            playerHealth.SetActive(false);
            if (reloadButton) reloadButton.SetActive(false);
            StartCoroutine(PlayerDeadUI());
            EventManager.Invoke(EventName.OnGameLost, _isPlayerDead);
            //this.RunOnSeconds(1f, () => hudCanvasGroup.alpha -= Time.deltaTime);
        }

    }
    public void EndGame()
    {
        if (!isLoseGame)
        {
            StartCoroutine(DelayHideHUD());
        }
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
        uIAnimSimulator.StartAnimateTextAppear();
        yield return null;
        UIEndGame.Instance.ShowUIEndGame();
        uIAnimSimulator.ShowUIEndGameWin();
        isCanTouch = true;
    }
    private IEnumerator PlayerDeadUI()
    {
        isLoseGame = true;
        //Debug.Log("isLoseGame");
        EventManager.Invoke(EventName.OnGameLost, _isPlayerDead);
        UIEndGame.Instance.IsShowEndGame = true;
        yield return WaitSeconds(1f);
        if (RoundTxtShadow) RoundTxtShadow.text = "GAME OVER!";
        RoundTxt.text = "GAME OVER!";
        uIAnimSimulator.StartAnimateTextAppear();
        yield return WaitSeconds(_timeDelayShowHUD);
        HUD.SetActive(false);
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
        SetTimeToEndGame();
        StopGame();
        //TotalBotText.text = $"Enemy Remaining: {GameResultInstance.Instance.GetGameResultData().BotKillCount} / {TotalBotinConfig}";
        TotalBotText.text = $"Enemy Remaining: {GameResultInstance.Instance.GetGameResultData().BotKillCount} / {GameResultInstance.Instance.GetGameResultData().requiredBotKill}";
        //process.fillAmount = ((float)(GameResultInstance.Instance.GetGameResultData().BotKillCount) / TotalBotinConfig);
        process.fillAmount = ((float)(GameResultInstance.Instance.GetGameResultData().BotKillCount) / GameResultInstance.Instance.GetGameResultData().requiredBotKill);

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

}




