using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    public static BotManager Instance { get; private set; }
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private PoolType bot;
    [SerializeField] private BotConfigSO botConfig;
    

    private void Awake()
    {
        Instance = this;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnBot( BotType.Infantry);
        }
    }

    public Bot SpawnBot( BotType botType)
    {
        HousePoint housePoint = HouseManager.instance.GetHousePoint(botType);
        GameObject botObj  = ObjectPool.Instance.GetPooledObject(this.bot, botPrefab);
        //zbotObj.transform.position = _housePoint.SpawnPoint;
        Bot bot = botObj .GetComponent<Bot>();
        bot.transform.position = housePoint.SpawnPoint.position;
        bot.Initialize(botConfig, housePoint);
        bot.OnSpawn();
        gameObject.SetActive(true);        
        return bot;
    }

    public void DespawnBot(Bot bot)
    {
        ObjectPool.Instance.ReturnToPool(this.bot, bot.gameObject);
    }
    
    
}

public enum BotType
{
    Infantry
}