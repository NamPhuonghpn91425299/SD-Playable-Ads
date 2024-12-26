 using System.Collections;
using System.Collections.Generic;
using Luna.Unity;
using UnityEngine;
using static HelperCoroutine;
public class UIEndGame : MonoBehaviour
{
    public static UIEndGame Instance;
    public GameObject EndGameObj;
    public GameObject EndGameWon;
    public GameObject EndGameLose;
    public CanvasGroup EndGameCanvasGroup;
    public UIAnimSimulator uIAnimSimulator;
    public bool IsShowEndGame;
    public bool IsCheckEndGame;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        HideUIEndGame();
    }

    private void Update()
    {
        if (IsCheckEndGame)
        {
            StartCoroutine(uIAnimSimulator.ShowUIEndGame());
            IsCheckEndGame = false; 
        } 

    }

    public void HideUIEndGame()
    {
        EndGameObj.SetActive(false);
        EndGameLose.SetActive(false);
        EndGameWon.SetActive(false);
        EndGameCanvasGroup.alpha = 0;
    }    
    public void ShowUIEndGame()
    {
        OnShowUIEndGameWon();
    }    
    private void OnShowUIEndGameWon()
    {
        IsShowEndGame = true;
        EndGameObj.SetActive(true);
        EndGameLose.SetActive(false);
        EndGameWon.SetActive(true);
        this.RunOnSeconds(1f, () => EndGameCanvasGroup.alpha += Time.deltaTime);
        //this.DelaySeconds(3f, () => EndGameWon.SetActive(true));
        Luna.Unity.LifeCycle.GameEnded();
        Luna.Unity.Analytics.LogEvent(Luna.Unity.Analytics.EventType.EndCardShown);
        Debug.Log("EndgameWonPanel");
        //StartCoroutine(uIAnimSimulator.ShowUIEndGame());
    }

    public void ShowEndGameLose()
    {
        OnshowUiEndGameLose();
    }
    private void OnshowUiEndGameLose()
    {
        IsShowEndGame = true;
        EndGameObj.SetActive(true);
        EndGameWon.SetActive(false);
        EndGameLose.SetActive(true);
        this.RunOnSeconds(1f, ()=> EndGameCanvasGroup.alpha += Time.deltaTime);
        Luna.Unity.LifeCycle.GameEnded();
        Luna.Unity.Analytics.LogEvent(Luna.Unity.Analytics.EventType.EndCardShown);
        Debug.Log("EndgameLosePanel.");
    }
}
