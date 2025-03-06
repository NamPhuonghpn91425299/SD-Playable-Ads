using System;
using UnityEngine;
using UnityEditor;
using static GameConstants;

[CreateAssetMenu(fileName = "ConfigGame", menuName = "ScriptableObjects/ConfigGame", order = 1)]
public class ConfigGame : ScriptableObject
{
    public FightRound[] fightRound;
}
[Serializable]
public class FightRound
{
    public BotConfig[] botConfigs;
    public RewardConfig[] rewardConfig;
}

[Serializable]
public class BotConfig
{
    [Header("Thông tin Bot")]
    [Tooltip("Kéo Prefab của Bot vào đây")]
    public GameObject botPrefab;

    [Tooltip("Chọn loại Bot")]
    public BotType botType;

    [Tooltip("Có sử dụng bot này trong kịch bản không?")]
    public bool isNotUse;

    [Tooltip("Bot có sử dụng nhưng ko tính vào BotCount")]
    public bool isNotCount;

    [Tooltip("Số lượng bot sinh ra")]
    public int botQuantity;

    [Tooltip("Thời gian Delay giữa các lần thả bot")]
    public float botDelaySpawn;
    [Tooltip("Thời gian Spawn của Lính")]
    public float WaitToSpawn;
    public bool IsSpawmOnBotKill;

    public float BotkillSpawn;
}

[Serializable]
public class RewardConfig
{
    public RewardNetwork rewardPrefab;

    public RewardType rewardType;

    public bool isNotUse;

    public int rewardQuantity;

    public float RewardDelaySpawn;

    public float WaitToSpawn;

    public bool IsSpawmOnBotKill;

    public float BotkillSpawn;
}


