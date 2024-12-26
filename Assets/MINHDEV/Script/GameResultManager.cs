using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HelperCoroutine;
public class GameResultManager : MonoBehaviour
{
    [SerializeField] public GameResultData gameResultData;
    [SerializeField] public bool _isShowCard;

    private void OnEnable()
    {
        EventManager.AddListener<int>(EventName.OnCheckTurnPlay, OnCheckShowEndCard);
        EventManager.AddListener<int>(EventName.OnCountBotLanding, OnCheckBotLanding);
        //EventManager.AddListener<float>(EventName.OnHealthPlayer, OnCheckHP);
    }


    private void OnDisable()
    {
        OnResetValue();
        EventManager.RemoveListener<int>(EventName.OnCheckTurnPlay, OnCheckShowEndCard);
        EventManager.RemoveListener<int>(EventName.OnCountBotLanding, OnCheckBotLanding);
        //EventManager.RemoveListener<float>(EventName.OnHealthPlayer, OnCheckHP);
    }

    private void OnShowEndCard()
    {
        _isShowCard = true;
         EventManager.Invoke(EventName.OnShowLunaEndGame, _isShowCard);
         UIManager.Instance.EndGame();

    }    
    // private void OnCheckHP(float arg0)
    // {
    //     if (arg0 <= 0)
    //     {
    //        OnShowEndCard();
    //     }
    // }

    private void OnCheckShowEndCard(int TurnToShowEndCard)
    {
        if (TurnToShowEndCard == gameResultData.TurnEnd && !_isShowCard && gameResultData.IsCountTurn && BotManager.Instance.TotalBotOnTurn == gameResultData.BotKillCount)
        {
            OnShowEndCard();
        }
    }

    private void OnCheckBotLanding(int BotCount)
    {
        gameResultData.BotLandingCount = BotCount;
        if (gameResultData.BotLandingCount == gameResultData.BotLandingCountConfig && !_isShowCard && gameResultData.IsCountLandingBot)
        {
            OnShowEndCard();
        }
    }
    void OnResetValue()
    {
        gameResultData.BotLandingCount = 0;
    }
}
