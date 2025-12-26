using System;
using UnityEngine;

[Serializable]
public class ConditionDefinition
{
    public enum EConditionType { Timer, KillCount }
    public EConditionType Type;
    public float WaitTime;
    public int TargetKills;
    public SpawnableType TypeToCount = SpawnableType.Bot; // Mặc định là đếm Bot

    public ISpawnCondition CreateRuntimeCondition()
    {
        switch (Type)
        {
            case EConditionType.Timer: return ConditionFactory.CreateTimerCondition(WaitTime);
            case EConditionType.KillCount: return ConditionFactory.CreateKillCountCondition(TargetKills, TypeToCount);
        }
        return null;
    }
}