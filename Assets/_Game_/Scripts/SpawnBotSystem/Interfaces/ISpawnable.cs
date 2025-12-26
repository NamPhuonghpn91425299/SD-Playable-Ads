using System;
using UnityEngine;
using static GameConstants;
public interface ISpawnable
{
    BotType BotType { get; }
    BotMoveType BotMoveType { get; }
    SpawnableType Type { get; }
    GameObject GameObject { get; }
    bool IsFromRoundScript { get; } 
    void Bot_Initialize(BotType botType,BotMoveType botMoveType , SpawnableType spawnableType, bool isFromRoundScript, PointGroup assignedPath);
    void Bot_ReportKill();
    event Action<ISpawnable> OnBotDeathReported;
}