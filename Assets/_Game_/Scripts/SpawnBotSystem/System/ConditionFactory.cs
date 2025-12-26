using System.Collections.Generic;
using UnityEngine;
public static class ConditionFactory
{

    private static Stack<TimerCondition> timerPool = new Stack<TimerCondition>(8);
    private static Stack<KillCountCondition> killCountPool = new Stack<KillCountCondition>(8);

    public static TimerCondition CreateTimerCondition(float time)
    {
        TimerCondition condition;
        if (timerPool.Count > 0)
        {
            condition = timerPool.Pop();
            // Reset with new time - we'd need to modify TimerCondition to support this
        }
        else
        {
            condition = new TimerCondition(time);
        }
        return condition;
    }

    public static KillCountCondition CreateKillCountCondition(int kills, SpawnableType type)
    {
        KillCountCondition condition;
        if (killCountPool.Count > 0)
        {
            condition = killCountPool.Pop();
            // Reset with new parameters - we'd need to modify KillCountCondition to support this
        }
        else
        {
            condition = new KillCountCondition(kills, type);
        }
        return condition;
    }

    public static void ReturnCondition(ISpawnCondition condition)
    {
        switch (condition)
        {
            case TimerCondition timer:
                if (timerPool.Count < 16) timerPool.Push(timer);
                break;
            case KillCountCondition killCount:
                if (killCountPool.Count < 16) killCountPool.Push(killCount);
                break;
        }
    }
}