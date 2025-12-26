using UnityEngine;
using System.Collections.Generic;
public class KillCountCondition : ISpawnCondition
{
    private int targetKills;
    private SpawnableType typeToCount;
    private int currentKills = 0;
    private bool isActive = false;

    public KillCountCondition(int kills, SpawnableType type)
    {
        targetKills = kills;
        typeToCount = type;
    }

    public bool IsMet() => currentKills >= targetKills;

    /// <summary>
    /// Hàm mới để tái khởi tạo điều kiện khi được lấy từ pool.
    /// </summary>
    public void Reinitialize(int newKills, SpawnableType newType)
    {
        this.targetKills = newKills;
        this.typeToCount = newType;
        Reset();
    }
    
    public void Reset() 
    { 
        currentKills = 0;
        isActive = true;
        
        // Register with centralized manager thay vì direct event subscription
        if (ConditionManager.Instance != null)
        {
            ConditionManager.Instance.RegisterKillCondition(this);
            // Get current kill count
            currentKills = ConditionManager.Instance.GetKillCount(typeToCount);
        }
    }

    public void Terminate()
    {
        isActive = false;
        
        // Unregister from centralized manager
        if (ConditionManager.Instance != null)
        {
            ConditionManager.Instance.UnregisterKillCondition(this);
        }
    }

    // OPTIMIZATION #10: Batch update callback thay vì individual events
    public void OnKillCountsUpdated(Dictionary<SpawnableType, int> killCounts)
    {
        if (isActive && killCounts.TryGetValue(typeToCount, out var count))
        {
            currentKills = count;
        }
    }
}