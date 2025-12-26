using static GameConstants;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Đóng gói tất cả thông tin cho một "bước spawn" để gửi đến Orchestrator.
/// </summary>
public class BotSpawnOrder
{
    public BotType BotTypeToSpawn;
    public int Quantity;
    public float DelayBetweenSpawns;
    public BotMoveType BotMoveType;
    public List<ISpawnCondition> Conditions;
    public bool IsFromRoundScript;

    // OPTIMIZATION #14: Object pooling for SpawnRequest
    private static Stack<BotSpawnOrder> requestPool = new Stack<BotSpawnOrder>(16);

    public static BotSpawnOrder Get()
    {
        if (requestPool.Count > 0)
        {
            var request = requestPool.Pop();
            request.Reset();
            return request;
        }
        return new BotSpawnOrder();
    }

    public static void Return(BotSpawnOrder order)
    {
        if (order != null && requestPool.Count < 32) // Limit pool size
        {
            requestPool.Push(order);
        }
    }

    private void Reset()
    {
        BotTypeToSpawn = BotType.None;
        Quantity = 0;
        DelayBetweenSpawns = 0f;
        BotMoveType = BotMoveType.Infantry;
        Conditions?.Clear();
        IsFromRoundScript = false;
    }
}