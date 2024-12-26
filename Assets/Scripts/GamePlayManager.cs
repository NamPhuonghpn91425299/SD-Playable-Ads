using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField] public ConfigGame configBotInGame;
    [SerializeField] public GameResultData gameResultData;
    [SerializeField] public List<Spawn> spawns;
    public static GamePlayManager Instance;
    public int Turn;
    public bool IsShowLunaEndGame;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BotManager.Instance.TotalBotOnTurn = OnCheckTotalBotOnMap();
        UIManager.Instance.UpdateInitBot(BotManager.Instance.TotalBotOnTurn);
        EventManager.Invoke(EventName.OnCheckTurnPlay, gameResultData.TurnCount);
        SetData();
        GameStart();
    }

    private void Update()
    {
        if (CheckTurnDone() && !UIEndGame.Instance.IsShowEndGame)
        {
            Turn++;
            gameResultData.TurnCount++;
            EventManager.Invoke(EventName.OnCheckTurnPlay, gameResultData.TurnCount);
            gameResultData.BotKillCount = 0;
            BotManager.Instance.TotalBotOnTurn = OnCheckTotalBotOnMap();
            UIManager.Instance.UpdateInitBot(BotManager.Instance.TotalBotOnTurn);
            PathManager.Instance.ResetPath();
            StartCoroutine(ClearPreviousBots());  // Thêm dòng này để loại bỏ các bot của lượt trước
            StartCoroutine(TurnDelay());
        }
    }

    private void OnEnable()
    {
        EventManager.AddListener<bool>(EventName.OnShowLunaEndGame, OnShowLunaEndGame);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<bool>(EventName.OnShowLunaEndGame, OnShowLunaEndGame);
        OnResetResultData();
    }

    private void OnShowLunaEndGame(bool IsShow)
    {
        IsShowLunaEndGame = IsShow;
    }

    void OnResetResultData()
    {
        gameResultData.TurnCount = 0;
    }

    private IEnumerator TurnDelay()
    {
        yield return new WaitForSeconds(1);
        SetData();
        GameStart();
    }

    public bool CheckTurnDone()
    {
        return BotManager.Instance.TotalBotOnTurn == gameResultData.BotKillCount;
    }

    public void SetData()
    {
        if (!IsShowLunaEndGame)
        {
            foreach (var spawn in spawns)
            {
                spawn.InitDataBot(configBotInGame.fightRound[Turn].botConfigs);
                spawn.InitDataReward(configBotInGame.fightRound[Turn].rewardConfig); // Thêm dòng này để khởi tạo dữ liệu phần thưởng
            }

        }
    }

    public void GameStart()
    {
        foreach (var spawn in spawns)
        {
            if (ShouldSpawnBot(spawn))
            {
                spawn.SpawnBot();
            }
            if (ShouldSpawnReward(spawn))
            {
                spawn.SpawnReward();
            }
        }
        
        
    }

    private bool ShouldSpawnBot(Spawn spawn)
    {
        if (!IsShowLunaEndGame)
        {
            foreach (var config in configBotInGame.fightRound[Turn].botConfigs)
            {
                if (spawn.IsBotType(config.botType) && !config.isNotUse)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return false;
        }

    }

    private bool ShouldSpawnReward(Spawn spawn)
    {
        if (!IsShowLunaEndGame)
        {
            foreach (var config in configBotInGame.fightRound[Turn].rewardConfig)
            {
                if (spawn.IsRewardType(config.rewardType) && !config.isNotUse)
                {
                    return true;
                }
            }
            return false;
        }
        else { return false; }

    }

    private int OnCheckTotalBotOnMap()
    {
        if (!IsShowLunaEndGame)
        {
            int botCount = 0;
            for (int i = 0; i < configBotInGame.fightRound[Turn].botConfigs.Length; i++)
            {
                if (!configBotInGame.fightRound[Turn].botConfigs[i].isNotCount)
                {
                    botCount += configBotInGame.fightRound[Turn].botConfigs[i].botQuantity;
                }  

            }
            //Debug.Log($"Total bots on Turn {Turn}: {botCount}");
            return botCount;
        }
        else
        {
            return 0;
        }
    }

    private IEnumerator ClearPreviousBots()
    {
        yield return new WaitForSeconds(.5f);
        BotManager.Instance.ClearAllBots();
        RewardManager.Instance.ClearAllRewards();
    }

    public void LunaClick()
    {
        Debug.LogError("Luna Clicked");
        Luna.Unity.Playable.InstallFullGame();
    }
}
