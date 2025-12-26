
---

### --- START OF FILE `GameManager.cs` ---
```csharp
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// LÀ BỘ NÃO CỦA TRÒ CHƠI.
/// Chịu trách nhiệm quản lý trạng thái của game (đang chờ, trong round, hoàn thành).
/// Điều phối việc bắt đầu và kết thúc các vòng chơi (round).
/// Đếm số lượng bot được sinh ra, bị tiêu diệt và tính toán tổng số bot cho mỗi round.
/// Giao tiếp với UI để hiển thị thông tin round và số lượng bot.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Enum để định nghĩa các trạng thái có thể có của game.
    public enum GameState { WaitingToStart, InRound, LevelComplete }
    
    // Sử dụng Singleton pattern để các script khác có thể dễ dàng truy cập vào GameManager.
    public static GameManager Instance { get; private set; }

    [Header("Level Configuration")]
    [Tooltip("Danh sách các vòng chơi (Round) cho màn này, được định nghĩa bằng ScriptableObject.")]
    [SerializeField] private List<RoundSO> levelRounds;

    [Header("Game State")]
    // Các biến nội bộ để theo dõi trạng thái hiện tại của game.
    private GameState currentState = GameState.WaitingToStart;
    public GameState CurrentState => currentState; // Biến public chỉ cho phép đọc, không cho phép sửa từ bên ngoài.

    private int currentRoundIndex = -1; // Index của round hiện tại.
    [SerializeField] private int totalBotsForRound;    // TỔNG SỐ BOT THỰC TẾ cho round này (cha + con + pre-spawned). Dùng cho UI.
    [SerializeField] private int spawnedScriptedBots;  // Số bot CHA đã được spawn từ kịch bản round.
    [SerializeField] private int totalScriptedBots;    // Tổng số bot CHA cần spawn từ kịch bản round. Dùng cho logic kết thúc round.
    [SerializeField] private int killedBotsForRound;   // Số bot đã bị tiêu diệt trong round.

    [Header("Events for UI")]
    // Các sự kiện (UnityEvent) để giao tiếp với hệ thống UI mà không cần tham chiếu trực tiếp.
    public UnityEvent<string, int, int> OnRoundStart; // Gửi tên round, round hiện tại, tổng số round.
    public UnityEvent<int, int> OnBotCountChanged;    // Gửi số bot đã giết, tổng số bot.
    public UnityEvent OnLevelComplete;                // Thông báo màn chơi hoàn tất.

    // Tối ưu #1: Dùng HashSet để lưu các bot pre-spawn đã xử lý, giúp kiểm tra nhanh hơn.
    private HashSet<PreSpawnedBot> processedPreSpawned = new HashSet<PreSpawnedBot>();
    
    // Tối ưu #2: Cache dữ liệu tính toán của các round.
    // Giúp game không phải tính toán lại số lượng bot mỗi khi bắt đầu một round.
    private Dictionary<int, RoundCalculationCache> roundDataCache = new Dictionary<int, RoundCalculationCache>();
    
    // Tối ưu #3: Gộp các lần cập nhật UI.
    // Thay vì cập nhật UI mỗi khi có sự kiện, ta sẽ gộp lại và chỉ cập nhật một lần trong LateUpdate.
    private bool uiUpdatePending = false;
    private int pendingKilledCount = 0;
    private int pendingTotalCount = 0;

    /// <summary>
    /// Một cấu trúc dữ liệu nhỏ để lưu các thông tin đã được tính toán trước cho mỗi round.
    /// </summary>
    private struct RoundCalculationCache
    {
        public int ScriptedBotCount; // Tổng số bot CHA được spawn từ kịch bản.
        public int TotalBotCount;    // TỔNG SỐ bot thực tế của round.
        public int PreSpawnedBotCount; // Tổng số bot có sẵn từ đầu.
        
        public RoundCalculationCache(int scripted, int total, int preSpawned)
        {
            ScriptedBotCount = scripted;
            TotalBotCount = total;
            PreSpawnedBotCount = preSpawned;
        }
    }

    private void Awake()
    {
        // Khởi tạo Singleton.
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Kiểm tra sự tồn tại của BotSpawnManager, một thành phần phụ thuộc quan trọng.
        if (BotSpawnManager.Instance == null) 
        { 
            Debug.LogError("BotSpawnManager not found!"); 
            this.enabled = false; 
            return; 
        }

        // Đăng ký lắng nghe các sự kiện từ BotSpawnManager.
        BotSpawnManager.Instance.OnBotSpawned += HandleBotSpawned;
        BotSpawnManager.Instance.OnBotKilled += HandleBotKilled;

        if (levelRounds == null || levelRounds.Count == 0) 
        { 
            this.enabled = false; 
            return; 
        }

        // TÍNH TOÁN TRƯỚC DỮ LIỆU. Chuyển xuống Start() để đảm bảo các Instance khác đã Awake() xong.
        PreCalculateAllRounds();

        // Bắt đầu vòng chơi đầu tiên.
        StartNextRound();
    }

    // Hủy đăng ký sự kiện khi GameManager bị phá hủy để tránh lỗi.
    private void OnDestroy()
    {
        if (BotSpawnManager.Instance != null)
        {
            BotSpawnManager.Instance.OnBotSpawned -= HandleBotSpawned;
            BotSpawnManager.Instance.OnBotKilled -= HandleBotKilled;
        }
    }

    // Cập nhật UI theo từng đợt (batching).
    private void LateUpdate()
    {
        // Nếu có một yêu cầu cập nhật UI đang chờ, thực hiện nó.
        if (uiUpdatePending)
        {
            OnBotCountChanged?.Invoke(pendingKilledCount, pendingTotalCount);
            uiUpdatePending = false; // Reset cờ hiệu.
        }

        // Kiểm tra điều kiện kết thúc round.
        if (currentState == GameState.InRound && 
            spawnedScriptedBots >= totalScriptedBots && 
            BotSpawnManager.Instance.ActiveSpawnCount == 0)
        {
            EndCurrentRound();
        }
    }

    /// <summary>
    /// Tính toán trước toàn bộ dữ liệu của các round và lưu vào cache.
    /// </summary>
    private void PreCalculateAllRounds()
    {
        if (levelRounds == null) return;

        // Tính tổng số bot có sẵn (pre-spawned) bao gồm cả bot con của chúng.
        int preSpawnedBotAndMinionCount = CalculatePreSpawnedBotCount();

        for (int i = 0; i < levelRounds.Count; i++)
        {
            var round = levelRounds[i];
        
            // Tính số bot CHA sẽ được spawn từ kịch bản. Con số này dùng cho LOGIC KẾT THÚC ROUND.
            int scriptedParentCount = CalculateScriptedBotCount(round); 
        
            // Tính TỔNG SỐ bot thực tế cho round này. Con số này dùng cho HIỂN THỊ UI.
            int totalCountForThisRound = CalculateTotalBotCountForRound(round, (i == 0 ? preSpawnedBotAndMinionCount : 0));

            // Lưu các giá trị đã tính toán vào cache.
            roundDataCache[i] = new RoundCalculationCache(
                scriptedParentCount,
                totalCountForThisRound,
                i == 0 ? preSpawnedBotAndMinionCount : 0
            );
        }
    }

    /// <summary>
    /// Tính toán tổng số bot được đặt sẵn trong màn chơi (pre-spawned), bao gồm cả bot con của chúng.
    /// </summary>
    private int CalculatePreSpawnedBotCount()
    {
        var preSpawnedBots = SpawnableRegistry.AllPreSpawnedBots;
        int count = preSpawnedBots.Count;

        foreach (var bot in preSpawnedBots)
        {
            var spawner = bot.GetComponent<SpawningUnitController>();
            if (spawner != null)
            {
                count += spawner.TotalMinionCount;
            }
        }
        return count;
    }

    /// <summary>
    /// Tính toán số lượng bot CHA được spawn trực tiếp từ kịch bản của một round.
    /// </summary>
    private int CalculateScriptedBotCount(RoundSO round)
    {
        int count = 0;
        for (int i = 0; i < round.SpawnSteps.Count; i++)
        {
            var step = round.SpawnSteps[i];
            count += step.Quantity;
        }
        return count;
    }

    /// <summary>
    /// Tính toán TỔNG SỐ bot thực tế cho một round, bao gồm bot cha, bot con, và bot có sẵn.
    /// </summary>
    private int CalculateTotalBotCountForRound(RoundSO round, int preSpawnedCount)
    {
        int scriptedParentCount = CalculateScriptedBotCount(round);
        int scriptedMinionCount = 0;

        foreach (var step in round.SpawnSteps)
        {
            var definition = BotSpawnManager.Instance?.GetDefinitionForType(step.BotToSpawn);
            if (definition == null || definition.Prefab == null) continue;

            var spawner = definition.Prefab.GetComponent<SpawningUnitController>();
            if (spawner != null)
            {
                scriptedMinionCount += spawner.TotalMinionCount * step.Quantity;
            }
        }
        return preSpawnedCount + scriptedParentCount + scriptedMinionCount;
    }
    
    /// <summary>
    /// Bắt đầu một vòng chơi mới.
    /// </summary>
    private void StartNextRound()
    {
        currentRoundIndex++;
        if (currentRoundIndex >= levelRounds.Count) { LevelCompleted(); return; }

        currentState = GameState.InRound;
        var currentRound = levelRounds[currentRoundIndex];

        Debug.Log($"---------- Starting Round {currentRoundIndex + 1} ----------");

        if (roundDataCache.TryGetValue(currentRoundIndex, out var cachedData))
        {
            totalScriptedBots = cachedData.ScriptedBotCount;
            totalBotsForRound = cachedData.TotalBotCount;
            Debug.Log($"[Cache HIT] Successfully loaded data for round {currentRoundIndex + 1}.");
            Debug.Log($" -> totalScriptedBots (Parents for logic) set to: {totalScriptedBots}");
            Debug.Log($" -> totalBotsForRound (For UI) set to: {totalBotsForRound}");
        }
        else
        {
            CalculateInitialBotCountForRound(currentRound);
        }

        spawnedScriptedBots = 0;
        killedBotsForRound = 0;
        Debug.Log("Internal counters have been reset.");

        OnRoundStart?.Invoke(currentRound.RoundName, currentRoundIndex + 1, levelRounds.Count);
        ScheduleUIUpdate(killedBotsForRound, totalBotsForRound);

        ActivatePreSpawnedBotsOptimized();
        ProcessRoundOptimized(currentRound);
    
        if (ConditionManager.Instance != null) { ConditionManager.Instance.ResetKillCounts(); }
    }


    /// <summary>
    /// Kích hoạt các bot đã được đặt sẵn trong màn chơi.
    /// </summary>
    private void ActivatePreSpawnedBotsOptimized()
    {
        if (currentRoundIndex != 0) return;
        
        var preSpawnedBots = SpawnableRegistry.AllPreSpawnedBots;
        foreach (var bot in preSpawnedBots)
        {
            if (bot != null && !processedPreSpawned.Contains(bot))
            {
                var spawner = bot.GetComponent<SpawningUnitController>();
                if (spawner != null)
                {
                    spawner.ActivateSpawning();
                }
                bot.Register();
                processedPreSpawned.Add(bot);
            }
        }
    }

    /// <summary>
    /// Xử lý các bước spawn của một round.
    /// </summary>
    private void ProcessRoundOptimized(RoundSO round)
    {
        for (int i = 0; i < round.SpawnSteps.Count; i++)
        {
            var step = round.SpawnSteps[i];
            var request = SpawnRequest.Get();
            
            request.BotTypeToSpawn = step.BotToSpawn;
            request.Quantity = step.Quantity;
            request.DelayBetweenSpawns = step.DelayBetweenSpawns;
            request.BotMoveType = step.botMoveType;
            request.IsFromRoundScript = true;
            
            if (request.Conditions == null) request.Conditions = new List<ISpawnCondition>(step.Conditions.Count);
            else request.Conditions.Clear();

            for (int j = 0; j < step.Conditions.Count; j++)
            {
                var condition = step.Conditions[j].CreateRuntimeCondition();
                if (condition != null) request.Conditions.Add(condition);
            }

            StartCoroutine(ProcessRequestWithCleanup(request));
        }
    }

    /// <summary>
    /// Đảm bảo SpawnRequest được trả lại pool sau khi sử dụng xong.
    /// </summary>
    private IEnumerator ProcessRequestWithCleanup(SpawnRequest request)
    {
        yield return StartCoroutine(BotSpawnManager.Instance.ProcessRequest(request));
        SpawnRequest.Return(request);
    }

    /// <summary>
    /// Hàm được gọi mỗi khi có một con bot được spawn thành công.
    /// </summary>
    private void HandleBotSpawned(ISpawnable spawnedBot)
    {
        if (currentState != GameState.InRound || spawnedBot.Type != SpawnableType.Bot) return;

        if (spawnedBot.IsFromRoundScript)
        {
            spawnedScriptedBots++;
        }
    }

    /// <summary>
    /// Hàm được gọi mỗi khi có một con bot bị tiêu diệt.
    /// </summary>
    private void HandleBotKilled(ISpawnable killedBot)
    {
        if (currentState == GameState.InRound && killedBot.Type == SpawnableType.Bot)
        {
            Debug.Log($"<color=orange>[BOT KILLED]</color> Bot '{killedBot.GameObject.name}' đã bị tiêu diệt.");
            Debug.Log($"    -> TRƯỚC KHI TĂNG: killedBotsForRound = {killedBotsForRound}");
            killedBotsForRound++;
            Debug.Log($"    -> SAU KHI TĂNG: killedBotsForRound bây giờ là {killedBotsForRound}");
            Debug.Log($"    -> TIẾN TRÌNH HIỆN TẠI: {killedBotsForRound} / {totalBotsForRound}");
            Debug.Log($"    -> THÔNG TIN BOT: IsFromRoundScript = {killedBot.IsFromRoundScript}");

            ScheduleUIUpdate(killedBotsForRound, totalBotsForRound);
        }
        else if (killedBot.Type == SpawnableType.Bot)
        {
            Debug.LogWarning($"[CÁI CHẾT BỊ BỎ QUA] Bot '{killedBot.GameObject.name}' đã chết, nhưng không được đếm vì trạng thái game đang là '{currentState}'.");
        }
    }

    /// <summary>
    /// Lên lịch cập nhật UI thay vì gọi trực tiếp.
    /// </summary>
    private void ScheduleUIUpdate(int killed, int total)
    {
        pendingKilledCount = killed;
        pendingTotalCount = total;
        uiUpdatePending = true;
    }

    /// <summary>
    /// Hàm fallback trong trường hợp cache bị lỗi.
    /// </summary>
    private void CalculateInitialBotCountForRound(RoundSO round)
    {
        Debug.LogWarning($"[GameManager] CACHE MISS! Falling back to manual calculation for round {currentRoundIndex}. This should not happen frequently.");

        totalScriptedBots = CalculateScriptedBotCount(round);
        int preSpawnedCount = (currentRoundIndex == 0) ? CalculatePreSpawnedBotCount() : 0;
        totalBotsForRound = CalculateTotalBotCountForRound(round, preSpawnedCount);
    }

    /// <summary>
    /// Xử lý logic khi một round kết thúc.
    /// </summary>
    public void EndCurrentRound()
    {
        currentState = GameState.WaitingToStart;
        if (currentRoundIndex < 0 || currentRoundIndex >= levelRounds.Count) return;
        var completedRound = levelRounds[currentRoundIndex];
        StartCoroutine(RoundCompletionDelay(completedRound.DelayAfterComplete));
    }

    private IEnumerator RoundCompletionDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextRound();
    }

    private void LevelCompleted()
    {
        currentState = GameState.LevelComplete;
        OnLevelComplete?.Invoke();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Hàm này chỉ chạy trong Editor, dùng để vẽ các thông tin gỡ lỗi (debug) lên màn hình game.
    /// </summary>
    private void OnGUI()
    {
            GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 400));
            GUILayout.Label("=== GAMEMANAGER DEBUG ===");
            GUILayout.Label($"Current Round: {currentRoundIndex + 1}/{levelRounds?.Count ?? 0}");
            GUILayout.Label($"State: {currentState}");
            GUILayout.Label($"Scripted Bots Spawned: {spawnedScriptedBots}/{totalScriptedBots}");
            GUILayout.Label($"Bots Killed: {killedBotsForRound}/{totalBotsForRound}");
            GUILayout.Label($"Total Bots (incl. Minions): {totalBotsForRound}");
            GUILayout.Label($"Cached Rounds: {roundDataCache.Count}");
            GUILayout.Label($"UI Update Pending: {uiUpdatePending}");

            if (roundDataCache.TryGetValue(currentRoundIndex, out var cached))
            {
                GUILayout.Label($"Cached Total Bots: {cached.TotalBotCount}");
                GUILayout.Label($"Cached Scripted Bots: {cached.ScriptedBotCount}");
                GUILayout.Label($"Cached Pre-spawned Bots: {cached.PreSpawnedBotCount}");
            }
            GUILayout.EndArea();
    }
#endif
}
```

### --- START OF FILE `BotSpawnManager.cs` ---
```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// LÀ NGƯỜI QUẢN LÝ VIỆC SPAWN BOT CỦA GAME.
/// Chịu trách nhiệm nhận các yêu cầu spawn (SpawnRequest) và hiện thực hóa chúng (Instantiate GameObject).
/// Quản lý một danh sách các "bản thiết kế" (SpawnableDefinition) để biết prefab nào tương ứng với loại bot nào.
/// Được tối ưu hóa rất nhiều về hiệu năng bằng cách sử dụng cache cho mọi thứ có thể.
/// </summary>
public class BotSpawnManager : MonoBehaviour
{
    public static BotSpawnManager Instance { get; private set; }
    
    [SerializeField] private List<SpawnableDefinition> spawnableDefinitions;
    
    private HashSet<ISpawnable> activeSpawns = new HashSet<ISpawnable>();
    private Dictionary<BotType, SpawnableDefinition> definitionMap = new Dictionary<BotType, SpawnableDefinition>(16);
    private Dictionary<GameObject, ComponentCache> componentCache = new Dictionary<GameObject, ComponentCache>(100);
    private Dictionary<float, WaitForSeconds> waitCache = new Dictionary<float, WaitForSeconds>();
    
    public event Action<ISpawnable> OnBotSpawned;
    public event Action<ISpawnable> OnBotKilled;
    
    public int ActiveSpawnCount => activeSpawns.Count;

    private struct ComponentCache
    {
        public SpawnableWrapper Wrapper;
        public SpawningUnitController Spawner;
        
        public ComponentCache(GameObject go)
        {
            Wrapper = go.GetComponent<SpawnableWrapper>();
            Spawner = go.GetComponent<SpawningUnitController>();
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        if (spawnableDefinitions?.Count > 0)
        {
            foreach (var def in spawnableDefinitions)
            {
                if (def.BotType != BotType.None && !definitionMap.ContainsKey(def.BotType))
                {
                    definitionMap.Add(def.BotType, def);
                }
            }
        }
    }

    public SpawnableDefinition GetDefinitionForType(BotType botType)
    {
        definitionMap.TryGetValue(botType, out var definition);
        return definition;
    }

    public IEnumerator ProcessRequest(SpawnRequest request)
    {
        if (request.Conditions?.Count > 0)
        {
            yield return StartCoroutine(WaitForConditionsOptimized(request.Conditions));
        }
        
        yield return StartCoroutine(SpawnWithDelayOptimized(request));
    }

    private IEnumerator WaitForConditionsOptimized(List<ISpawnCondition> conditions)
    {
        for (int i = 0; i < conditions.Count; i++) conditions[i].Reset();
        
        bool allConditionsMet = false;
        while (!allConditionsMet)
        {
            allConditionsMet = true;
            for (int i = 0; i < conditions.Count; i++)
            {
                if (!conditions[i].IsMet())
                {
                    allConditionsMet = false;
                    break;
                }
            }
            if (!allConditionsMet) yield return null;
        }
        
        for (int i = 0; i < conditions.Count; i++) conditions[i].Terminate();
    }

    private IEnumerator SpawnWithDelayOptimized(SpawnRequest request)
    {
        if (!definitionMap.TryGetValue(request.BotTypeToSpawn, out var definition))
        {
            Debug.LogError($"Spawn failed: Definition for BotType '{request.BotTypeToSpawn}' not found!");
            yield break;
        }

        WaitForSeconds delayWait = null;
        if (request.DelayBetweenSpawns > 0 && request.Quantity > 1)
        {
            if (!waitCache.TryGetValue(request.DelayBetweenSpawns, out delayWait))
            {
                delayWait = new WaitForSeconds(request.DelayBetweenSpawns);
                waitCache[request.DelayBetweenSpawns] = delayWait;
            }
        }

        for (int i = 0; i < request.Quantity; i++)
        {
            GameObject go = Instantiate(definition.Prefab, transform.position, transform.rotation);
            
            if (!componentCache.TryGetValue(go, out var cachedComponents))
            {
                cachedComponents = new ComponentCache(go);
                componentCache[go] = cachedComponents;
            }

            if (cachedComponents.Wrapper == null)
            {
                Debug.LogError($"Spawn failed: Prefab '{definition.Prefab.name}' is missing a SpawnableWrapper component!");
                componentCache.Remove(go);
                Destroy(go);
                continue;
            }

            cachedComponents.Wrapper.System_Initialize(definition.BotType, definition.Type, request.IsFromRoundScript);
            TrackSpawnedBot(cachedComponents.Wrapper);

            if (cachedComponents.Spawner != null)
            {
                cachedComponents.Spawner.ActivateSpawning();
            }

            if (i < request.Quantity - 1 && delayWait != null)
            {
                yield return delayWait;
            }
        }
    }

    private void TrackSpawnedBot(ISpawnable spawnable)
    {
        activeSpawns.Add(spawnable);
        spawnable.OnSystemDestroy += HandleBotKilledInternal;
        OnBotSpawned?.Invoke(spawnable);
    }

    private void HandleBotKilledInternal(ISpawnable spawnable)
    {
        spawnable.OnSystemDestroy -= HandleBotKilledInternal;
        activeSpawns.Remove(spawnable);
        
        if (spawnable.GameObject != null)
        {
            componentCache.Remove(spawnable.GameObject);
        }
        
        OnBotKilled?.Invoke(spawnable);
        
        if (spawnable.GameObject != null) 
            Destroy(spawnable.GameObject);
    }

    private void OnDestroy()
    {
        componentCache.Clear();
        waitCache.Clear();
        activeSpawns.Clear();
    }
}
```

### --- START OF FILE `ConditionManager.cs` ---
```csharp
﻿using UnityEngine;
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

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        killCounts[SpawnableType.Bot] = 0;
        killCounts[SpawnableType.Generic] = 0;
        killCounts[SpawnableType.Reward] = 0;
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
        foreach (var key in new List<SpawnableType>(killCounts.Keys))
            killCounts[key] = 0;
        killCountsDirty = true;
    }
}
```