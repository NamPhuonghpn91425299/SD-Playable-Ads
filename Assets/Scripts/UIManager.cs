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
    public GameObject gameProcess;
    public Button tapToPlay;
    public Text bulletCountText; // Text UI để hiển thị số lượng đạn
    public Text RoundTxt; // Text UI để hiển thị số lượng đạn
    public CanvasGroup hudCanvasGroup;
    public Text timeToEndGameTxt;
    public float timeEndConfig = 30f;
    public Action<float> OnTimeout;
    private bool timeOut;

    private void Awake()
    {
        Instance = this;
        InGame.SetActive(true);
        gameProcess.SetActive(true);
        playerHealth.SetActive(true);
        reloadButton.SetActive(true);
        uIAnimSimulator.StartAnimateTextAppear();

    }

    private void Start()
    {
        Time.timeScale = 0f;
        tapToPlay.onClick.AddListener(OnButtonClick);
    }
    public void SetTimeToEndGame()
    {
        if (timeToEndGameTxt!= null)
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
            RoundTxt.text = "ROUND " + Round;
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
        OnPointerExit();
        Luna.Unity.LifeCycle.GameStarted();
        Luna.Unity.Analytics.LogEvent( Luna.Unity.Analytics.EventType.TutorialComplete);
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
        if (isPlayerDead)
        {
            playerHealth.SetActive(false);
            reloadButton.SetActive(false);
            StartCoroutine(PlayerDeadUI());
            
            //this.RunOnSeconds(1f, () => hudCanvasGroup.alpha -= Time.deltaTime);
        }

    }
    public void EndGame()
    {
        StartCoroutine(DelayHideHUD());
    }
    private IEnumerator DelayHideHUD()
    {
        UIEndGame.Instance.IsShowEndGame = true;
        RoundTxt.text = "YOU WIN!";
        uIAnimSimulator.StartAnimateTextAppear();
        yield return WaitSeconds(3f);
        HUD.SetActive(false);
        uIAnimSimulator.StartAnimateTextAppear();
        yield return null;
        UIEndGame.Instance.ShowUIEndGame();
     
    }
    private IEnumerator PlayerDeadUI()
    {
        UIEndGame.Instance.IsShowEndGame = true;
        RoundTxt.text = "YOU LOSE!";
        uIAnimSimulator.StartAnimateTextAppear();
        
        yield return WaitSeconds(3f);
        HUD.SetActive(false);
        uIAnimSimulator.StartAnimateTextAppear();
        yield return null;
        UIEndGame.Instance.ShowEndGameLose();
    }    
    private void OnTimeOut(float timer)
    {
        if (timer <= 0 && !UIEndGame.Instance.IsShowEndGame)
        {
            StartCoroutine(PlayerDeadUI());
        }
    }    
    void Update()
    {
        SetTimeToEndGame();
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


}
