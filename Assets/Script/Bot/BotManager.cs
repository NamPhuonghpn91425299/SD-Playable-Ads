using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    public static BotManager Instance { get; private set; }
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private const string BOT_POOL_TAG = "Bot";
    [SerializeField] private BotConfigSO botConfig;
    private void Awake()
    {
        Instance = this;
    }

    public Bot SpawnBot(Vector3 position)
    {
        GameObject botObj  = ObjectPool.Instance.GetPooledObject(BOT_POOL_TAG, botPrefab);
        botObj.transform.position = position;
        Bot bot = botObj .GetComponent<Bot>();
        bot.Initialize(botConfig);
        return bot;
    }

    public void DespawnBot(Bot bot)
    {
        ObjectPool.Instance.ReturnToPool(BOT_POOL_TAG, bot.gameObject);
    }
    
    
}
