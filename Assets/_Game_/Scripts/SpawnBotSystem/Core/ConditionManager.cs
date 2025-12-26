using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// LÀ HỆ THỐNG QUẢN LÝ ĐIỀU KIỆN TẬP TRUNG.
/// Theo dõi các sự kiện toàn cục và thông báo cho các điều kiện spawn đang hoạt động.
/// </summary>
public class ConditionManager : MonoBehaviour
{
    public static ConditionManager Instance { get; private set; }
    
    private Dictionary<SpawnableType, int> killCounts = new Dictionary<SpawnableType, int>();
    private List<KillCountCondition> activeKillConditions = new List<KillCountCondition>(32);
    private bool killCountsDirty = false;
    
    // Cache array để tránh allocation mỗi frame
    private SpawnableType[] cachedKillCountKeys;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        killCounts[SpawnableType.Bot] = 0;
        killCounts[SpawnableType.None] = 0;
        killCounts[SpawnableType.Reward] = 0;
        
        // Cache keys array một lần để tránh allocation
        UpdateCachedKeys();
    }

    private void Start()
    {
        if (BotSpawnManager.Instance != null)
        {
            BotSpawnManager.Instance.OnBotKilled += OnGlobalBotKilled;
        }
    }

    private void OnDestroy()
    {
        if (BotSpawnManager.Instance != null)
        {
            BotSpawnManager.Instance.OnBotKilled -= OnGlobalBotKilled;
        }
    }

    private void OnGlobalBotKilled(ISpawnable killedBot)
    {
        if (killCounts.ContainsKey(killedBot.Type))
        {
            killCounts[killedBot.Type]++;
            killCountsDirty = true;
        }
    }

    private void LateUpdate()
    {
        if (killCountsDirty && activeKillConditions.Count > 0)
        {
            for (int i = activeKillConditions.Count - 1; i >= 0; i--)
            {
                if (activeKillConditions[i] != null)
                    activeKillConditions[i].OnKillCountsUpdated(killCounts);
                else
                    activeKillConditions.RemoveAt(i);
            }
            killCountsDirty = false;
        }
    }

    public void RegisterKillCondition(KillCountCondition condition)
    {
        if (!activeKillConditions.Contains(condition))
            activeKillConditions.Add(condition);
    }

    public void UnregisterKillCondition(KillCountCondition condition)
    {
        activeKillConditions.Remove(condition);
    }

    public int GetKillCount(SpawnableType type) => killCounts.TryGetValue(type, out var count) ? count : 0;

    public void ResetKillCounts()
    {
        // Sử dụng cached keys array thay vì tạo List mới
        if (cachedKillCountKeys != null)
        {
            foreach (var key in cachedKillCountKeys)
            {
                if (killCounts.ContainsKey(key))
                    killCounts[key] = 0;
            }
        }
        killCountsDirty = true;
    }
    
    private void UpdateCachedKeys()
    {
        cachedKillCountKeys = new SpawnableType[killCounts.Count];
        killCounts.Keys.CopyTo(cachedKillCountKeys, 0);
    }
}