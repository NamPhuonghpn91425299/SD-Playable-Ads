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
    [SerializeField]private Bot _bot;

    private void Awake()
    {

        Instance = this;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnBot( new Vector3(1,0,62));
        }
    }

    public Bot SpawnBot(Vector3 position)
    {
        GameObject botObj  = ObjectPool.Instance.GetPooledObject(this.bot, botPrefab);
        botObj.transform.position = position;
        Bot bot = botObj .GetComponent<Bot>();
        bot.Initialize(botConfig);
        return bot;
    }

    public void DespawnBot(Bot bot)
    {
        ObjectPool.Instance.ReturnToPool(this.bot, bot.gameObject);
    }
    
    
}
