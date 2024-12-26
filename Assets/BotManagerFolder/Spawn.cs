using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static GameConstants;

public class Spawn : MonoBehaviour
{
    private List<BotConfig> _botConfigs = new List<BotConfig>();
    private List<RewardConfig> _rewardConfig = new List<RewardConfig>();
    //private RewardConfig _rewardConfig;
    [SerializeField] public BotType botType;
    [SerializeField] public RewardType rewardType;
    [SerializeField] private bool isSpawnBotKill ;

    private void OnEnable()
    {
        isSpawnBotKill = false;
        EventManager.AddListener<int>(EventName.OnBotKillCount, BotKillOnSpawnReward);
    }


    private void OnDisable()
    {
        EventManager.RemoveListener<int>(EventName.OnBotKillCount, BotKillOnSpawnReward);
    }
    #region SPAWN BOT

    public void InitDataBot(BotConfig[] botConfigs)
    {
        _botConfigs.Clear();
        foreach (var config in botConfigs)
        {
            if (config.botType == botType && !config.isNotUse)
            {
                _botConfigs.Add(config);
            }
        }
    }

    public void SpawnBot()
    {
        foreach (var config in _botConfigs)
        {
            if (!config.IsSpawmOnBotKill) 
            {
                StartCoroutine(OnSpawnBot(config));            
            }
            else if (config.IsSpawmOnBotKill && isSpawnBotKill)
            {
                StartCoroutine(OnSpawnBotKill(config));
            }
        }
    }

    private IEnumerator OnSpawnBot(BotConfig config)
    {
        yield return new WaitForSeconds(config.WaitToSpawn);
        for (var i = 0; i < config.botQuantity; i++)
        {
            WayPoint path = PathManager.Instance.GetWayPoint(botType);
            var spawnPosition = path.WayPoints[0].position;
            BotManager.Instance.SpawnBot(config.botPrefab, spawnPosition, path);
            yield return new WaitForSeconds(config.botDelaySpawn);
        }
    }

    private IEnumerator OnSpawnBotKill(BotConfig config)
    {
        for (var i = 0; i < config.botQuantity; i++)
        {
            WayPoint path = PathManager.Instance.GetWayPoint(botType);
            var spawnPosition = path.WayPoints[0].position;
            BotManager.Instance.SpawnBot(config.botPrefab, spawnPosition, path);
            yield return new WaitForSeconds(config.botDelaySpawn);
        }
    }

    public bool IsBotType(BotType type)
    {
        return botType == type;
    }

    #endregion

    #region SPAWN REWARD
    public void InitDataReward(RewardConfig[] rewardConfigs)
    {
        _rewardConfig.Clear();
        foreach (var config in rewardConfigs)
        {
            if (config.rewardType == rewardType && !config.isNotUse)
            {
                _rewardConfig.Add(config);
                break;
            }
        }

    }

    public void BotKillOnSpawnReward(int arg0)
    {
        foreach (var rewardConfig in _rewardConfig)
        {
            if (rewardConfig.IsSpawmOnBotKill)
            {
                if (arg0 == rewardConfig.BotkillSpawn)
                {
                    isSpawnBotKill = true;
                    SpawnReward();
                }
            }
        }

        foreach (var config in _botConfigs)
        {
            if (config.IsSpawmOnBotKill)
            {
                if (arg0 == config.BotkillSpawn)
                {
                    isSpawnBotKill = true;
                    SpawnBot();
                }
            }
        }
    }
    public void SpawnReward()
    {
        foreach (var config in _rewardConfig)
        {
            if (!config.IsSpawmOnBotKill)
            {
                StartCoroutine(OnSpawnReward(config));
            }
            else if (config.IsSpawmOnBotKill && isSpawnBotKill)
            {
                StartCoroutine(OnBotKillSpawnReward(config));
            }
        }

    }


    private IEnumerator OnSpawnReward(RewardConfig rewardConfig)
    {
        for (var i = 0; i < rewardConfig.rewardQuantity; i++)
        {
            yield return new WaitForSeconds(rewardConfig.WaitToSpawn);
            SpawnRewardPoint point = SpawnRewardManager.Instance.GetSpawnPoint(rewardType);
            var spawnPosition = point.SpawnPoint[0].position;
            RewardManager.Instance.SpawnReward(rewardConfig.rewardPrefab, spawnPosition, point);
            yield return new WaitForSeconds(rewardConfig.RewardDelaySpawn);
        }
    }

    private IEnumerator OnBotKillSpawnReward(RewardConfig rewardConfig)
    {

            for (var i = 0; i < rewardConfig.rewardQuantity; i++)
            {
                SpawnRewardPoint point = SpawnRewardManager.Instance.GetSpawnPoint(rewardType);
                var spawnPosition = point.SpawnPoint[0].position;
                RewardManager.Instance.SpawnReward(rewardConfig.rewardPrefab, spawnPosition, point);
                yield return new WaitForSeconds(rewardConfig.RewardDelaySpawn);
            }
    }
    public bool IsRewardType(RewardType type)
    {
        return rewardType == type;
    }
    #endregion



}
