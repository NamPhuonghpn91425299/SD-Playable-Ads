using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleConfigSO", menuName = "ScriptableObjects/BattleConfig")]
public class BattleConfigSO : ScriptableObject
{
    public FightRounds[] fightRound;

}
[Serializable]
public class FightRounds
{
    public BotStartConfig[] BotStartConfig;
}
[Serializable]
public class BotStartConfig
{
    [Tooltip("Số lượng bot sinh ra")]
    public int botQuantity;
    public BotConfigSO BotConfigSO;
    public float botDelaySpawn;
}