using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using Assets._Develop_.ThanhNT.Scripts.Observer;

/// <summary>
/// LÀ BỘ NÃO CỦA TRÒ CHƠI.
/// Chịu trách nhiệm quản lý trạng thái của game, điều phối các vòng chơi,
/// và đếm số lượng bot để xác định khi nào round kết thúc.
/// </summary>
public class GameManager : MonoBehaviour,
Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<GameStateChangedEvent>
{
    #region Enums & Singleton

    public enum GameState { WaitingToStart, InRound, LevelComplete }
    public static GameManager Instance { get; private set; }

    #endregion

    #region Fields and Properties

    [Header("Level Configuration")]
    [Tooltip("Danh sách các kịch bản Round (RoundSO) sẽ được thực thi tuần tự cho màn chơi này.")]
    [SerializeField] public List<RoundSO> levelRounds;

    [Header("Game State")]
    public GameState currentState = GameState.WaitingToStart;
    public GameState CurrentState => currentState;

    public int currentRoundIndex = -1;
    [Tooltip("(DEBUG) Tổng số bot mục tiêu của round hiện tại (bao gồm cả bot con).")]
    [SerializeField] public int totalBotsForRound;
    [Tooltip("(DEBUG) Tổng số bot đã bị tiêu diệt trong round hiện tại.")]
    [SerializeField] public int killedBotsForRound;
    [Tooltip("(DEBUG) Tổng số bot CHA theo kịch bản của round hiện tại.")]
    [SerializeField] public int totalScriptedBots;

    [Header("Events for UI")]
    public UnityEvent<string, int, int> OnRoundStart;
    public UnityEvent<int, int> OnBotCountChanged;
    public UnityEvent OnLevelComplete;

    // Khởi tạo Dictionary một lần duy nhất, reuse khi cần
    private readonly Dictionary<int, RoundCalculationCache> roundDataCache = new Dictionary<int, RoundCalculationCache>();
    private bool uiUpdatePending = false;
    private int pendingKilledCount = 0;
    private int pendingTotalCount = 0;
    private struct RoundCalculationCache
    {
        public int ScriptedBotCount;
        public int TotalBotCount;
        public int PreSpawnedBotCount;

        public RoundCalculationCache(int scripted, int total, int preSpawned)
        {
            ScriptedBotCount = scripted;
            TotalBotCount = total;
            PreSpawnedBotCount = preSpawned;
        }
    }

    #endregion

    #region Unity Lifecycle Methods

    private void Awake()
    {
        
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartGame()
    {
        if (BotSpawnManager.Instance == null) { this.enabled = false; return; }
        BotSpawnManager.Instance.OnBotSpawned += HandleBotSpawned;
        BotSpawnManager.Instance.OnBotKilled += HandleBotKilled;
        if (levelRounds == null || levelRounds.Count == 0) { this.enabled = false; return; }
        PreCalculateAllRounds();
        StartNextRound();

    }

    private void OnEnable()
    {
    }

    private void Start()
    {
        EventManager.Instance?.Subscribe<GameStateChangedEvent>(this);
    }

    private void OnDestroy()
    {
        if (BotSpawnManager.Instance != null)
        {
            BotSpawnManager.Instance.OnBotSpawned -= HandleBotSpawned;
            BotSpawnManager.Instance.OnBotKilled -= HandleBotKilled;
            EventManager.Instance?.Unsubscribe<GameStateChangedEvent>(this);
        }
    }

    private void LateUpdate()
    {
        if (uiUpdatePending)
        {
            OnBotCountChanged?.Invoke(pendingKilledCount, pendingTotalCount);
            uiUpdatePending = false;
        }

        if (currentState == GameState.InRound && killedBotsForRound >= totalBotsForRound)
        {
                // // EndCurrentRound();
                // print("+: "+ BotSpawnManager.Instance.ActiveSpawnCount);
            if (BotSpawnManager.Instance.ActiveSpawnCount == 0)
            {
                EndCurrentRound();
            }
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Xử lý khi một con bot bị tiêu diệt. Đây là hàm cốt lõi cho logic đếm.
    /// </summary>
    /// <param name="killedBot">Interface của con bot vừa bị tiêu diệt, được gửi từ sự kiện OnBotKilled.</param>
    private void HandleBotKilled(ISpawnable killedBot)
    {
        if (currentState != GameState.InRound) return;
        if (killedBot.Type == SpawnableType.Bot)
        {
            killedBotsForRound++;

            MinionSpawner spawner = killedBot.GameObject.GetComponent<MinionSpawner>();
            if (spawner != null)
            {
                int remainingMinions = spawner.RemainingMinionCount;
                if (remainingMinions > 0)
                {
                    killedBotsForRound += remainingMinions;
                }
            }
        }
        ScheduleUIUpdate(killedBotsForRound, totalBotsForRound);
    }

    /// <summary>
    /// Xử lý khi một con bot được spawn.
    /// </summary>
    /// <param name="spawnedBot">Interface của con bot vừa được spawn.</param>
    private void HandleBotSpawned(ISpawnable spawnedBot)
    {
        // Hiện tại không cần logic ở đây
    }

    #endregion

    #region Round and Spawning Logic

    /// <summary>
    /// Bắt đầu xử lý các bước spawn cho round hiện tại.
    /// </summary>
    /// <param name="round">Đối tượng ScriptableObject chứa kịch bản của round.</param>
    private void ProcessRoundOptimized(RoundSO round)
    {
        if (round.SpawnSteps == null) return;
        foreach (var step in round.SpawnSteps)
        {
            StartCoroutine(ProcessSingleStep(step));
        }
    }

    /// <summary>
    /// Coroutine quản lý toàn bộ vòng đời của một SpawnStep: chờ điều kiện, sau đó vào vòng lặp để spawn từng con bot.
    /// </summary>
    /// <param name="step">Đối tượng dữ liệu SpawnStep chứa toàn bộ kịch bản cho bước này.</param>
    private IEnumerator ProcessSingleStep(BotWave step)
    {
        yield return HelperCoroutine.GetWait(0.5f);
        if (step.Conditions != null && step.Conditions.Count > 0)
        {
            List<ISpawnCondition> runtimeConditions = step.Conditions.Select(condDef => condDef.CreateRuntimeCondition()).ToList();
            foreach (var cond in runtimeConditions) cond.Reset();
            while (runtimeConditions.Any(cond => !cond.IsMet()))
            {
                yield return null;
            }
            foreach (var cond in runtimeConditions) cond.Terminate();
        }
        BotDefinition definition = BotSpawnManager.Instance.GetDefinitionForType(step.BotToSpawn);
        if (definition == null)
        {
            Debug.LogError($"Không tìm thấy Definition cho BotType '{step.BotToSpawn}', không thể spawn.");
            yield break;
        }
        var order = BotSpawnOrder.Get();
        order.BotTypeToSpawn = step.BotToSpawn;
        order.BotMoveType = definition.BotMoveType;
        order.IsFromRoundScript = true;
        WaitForSeconds delayWait = step.DelayBetweenSpawns > 0 ? HelperCoroutine.GetWait(step.DelayBetweenSpawns) : null;

        for (int i = 0; i < step.Quantity; i++)
        {
            BotSpawnManager.Instance.ExecuteSpawnOrder(order);
            if (i < step.Quantity - 1 && delayWait != null)
            {
                yield return delayWait;
            }
        }

        BotSpawnOrder.Return(order);
    }

    #endregion

    #region Calculation and Initialization

    /// <summary>
    /// Tính toán trước toàn bộ dữ liệu của các round và lưu vào cache để tăng tốc độ bắt đầu round.
    /// </summary>
    private void PreCalculateAllRounds()
    {
        if (levelRounds == null) return;
        
        // Clear cache cũ thay vì tạo mới Dictionary
        roundDataCache.Clear();

        // Tính toán số pre-spawned bot (chỉ loại Bot) một lần duy nhất.
        int preSpawnedBotCount = CalculatePreSpawnedBotCount();

//        Debug.Log($"PreSpawnedBotCount: {preSpawnedBotCount}");
        for (int i = 0; i < levelRounds.Count; i++)
        {
            var round = levelRounds[i];

            // Tính số bot CHA từ kịch bản (chỉ loại Bot).
            int scriptedParentCount = CalculateScriptedBotCount(round);

            int totalCountForThisRound = CalculateTotalBotCountForRound(round, i == 0 ? preSpawnedBotCount : 0);

            roundDataCache[i] = new RoundCalculationCache(scriptedParentCount, totalCountForThisRound, i == 0 ? preSpawnedBotCount : 0);
        }
    }

    /// <summary>
    /// Tính tổng số bot được đặt sẵn trong màn chơi, bao gồm cả bot con tiềm năng của chúng.
    /// </summary>
    private int CalculatePreSpawnedBotCount()
    {
        int count = 0;
        var preSpawnedBots = SpawnableRegistry.AllPreSpawnedBots;
        foreach (var bot in preSpawnedBots)
        {

            if (bot == null || bot.Type != SpawnableType.Bot) continue;
            count++;
            // Kiểm tra xem nó có khả năng đẻ lính không.
            var spawner = bot.GetComponent<MinionSpawner>();
            if (spawner != null)
            {
                // Nếu có, cộng thêm số minion vào tổng.
                count += spawner.TotalMinionCount;
            }
        }

        return count;
    }

    /// <summary>
    /// Tính tổng số bot CHA được định nghĩa trong kịch bản của một round.
    /// </summary>
    /// <param name="round">Kịch bản round cần tính toán.</param>
    private int CalculateScriptedBotCount(RoundSO round)
    {
        int count = 0;
        if (round.SpawnSteps != null)
        {
            foreach (var step in round.SpawnSteps)
            {
                var definition = BotSpawnManager.Instance.GetDefinitionForType(step.BotToSpawn);
                if (definition != null && definition.Type == SpawnableType.Bot)
                {
                    count += step.Quantity;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// Tính tổng số bot thực tế cho một round (cha + con + pre-spawned).
    /// </summary>
    /// <param name="round">Kịch bản round cần tính toán.</param>
    /// <param name="preSpawnedCount">Số bot có sẵn đã được tính từ trước.</param>
    private int CalculateTotalBotCountForRound(RoundSO round, int preSpawnedCount)
    {
        int scriptedParentCount = 0;
        int scriptedMinionCount = 0;
        if (round.SpawnSteps != null)
        {
            foreach (var step in round.SpawnSteps)
            {
                var def = BotSpawnManager.Instance.GetDefinitionForType(step.BotToSpawn);
                var haveBotChild = def.Prefab.GetComponent<HaveBotChild>();
                if (haveBotChild != null)
                    scriptedMinionCount += haveBotChild.countBotChild * step.Quantity;
                if (def != null && def.Type == SpawnableType.Bot)
                {
                    scriptedParentCount += step.Quantity;
                    var spawner = def.Prefab.GetComponent<MinionSpawner>();
                    if (spawner != null) scriptedMinionCount += spawner.TotalMinionCount * step.Quantity;
                }
            }
        }
        // Tổng số bot của round = (bot cha + bot con từ kịch bản) + bot có sẵn
        return preSpawnedCount + scriptedParentCount + scriptedMinionCount;
    }

    /// <summary>
    /// Bắt đầu một vòng chơi mới, lấy dữ liệu từ cache và reset các bộ đếm.
    /// </summary>
    private void StartNextRound()
    {
        currentRoundIndex++;
        if (currentRoundIndex >= levelRounds.Count) { LevelCompleted(); return; }

        currentState = GameState.InRound;
        var currentRound = levelRounds[currentRoundIndex];

        if (roundDataCache.TryGetValue(currentRoundIndex, out var cachedData))
        {
            totalScriptedBots = cachedData.ScriptedBotCount;
            totalBotsForRound = cachedData.TotalBotCount;
        }
        else
        {
            CalculateInitialBotCountForRound(currentRound);
        }

        killedBotsForRound = 0;
        ActivatePreSpawnedBotsOptimized();

        OnRoundStart?.Invoke(currentRound.RoundName, currentRoundIndex + 1, levelRounds.Count);
        EventManager.Instance?.Publish(new GameDataChangedEvent(currentRound: $"{currentRoundIndex + 1}/{levelRounds?.Count ?? 0}"));
        
        ProcessRoundOptimized(currentRound);
        ScheduleUIUpdate(killedBotsForRound, totalBotsForRound);

        if (ConditionManager.Instance != null) { ConditionManager.Instance.ResetKillCounts(); }
    }

    /// <summary>
    /// Thực hiện quy trình "nhập tịch" cho các bot đã có sẵn trong scene.
    /// </summary>
    private void ActivatePreSpawnedBotsOptimized()
    {
        if (currentRoundIndex != 0) return;
        var preSpawnedBots = SpawnableRegistry.AllPreSpawnedBots;
        foreach (var bot in preSpawnedBots)
        {
            if (bot == null) continue;
            BotIdentity botIdentity = bot.GetComponent<BotIdentity>();
            if (botIdentity == null) continue;
            PointGroup path = PathManager.Instance.GetPath(bot.BotMoveType);
            if (path == null) { bot.gameObject.SetActive(false); continue; }
            bot.transform.position = path.points[0].position;
            botIdentity.Bot_Initialize(bot.BotType, bot.BotMoveType, bot.Type, false, path);
            BotSpawnManager.Instance.TrackSpawnedBot(botIdentity);
        }
    }

    #endregion

    #region UI and State Management

    /// <summary>
    /// Lên lịch cập nhật UI để tránh gọi sự kiện quá nhiều lần trong một frame.
    /// </summary>
    /// <param name="killed">Số bot đã bị tiêu diệt hiện tại.</param>
    /// <param name="total">Tổng số bot của round.</param>
    private void ScheduleUIUpdate(int killed, int total)
    {
        pendingKilledCount = killed;
        pendingTotalCount = total;
        if (pendingKilledCount > pendingTotalCount)
        {
            pendingKilledCount = pendingTotalCount;
        }
        uiUpdatePending = true;
    }

    /// <summary>
    /// Hàm fallback tính toán dữ liệu round nếu không tìm thấy trong cache.
    /// </summary>
    /// <param name="round">Kịch bản round cần tính toán.</param>
    private void CalculateInitialBotCountForRound(RoundSO round)
    {
        totalScriptedBots = CalculateScriptedBotCount(round);
        int preSpawnedCount = (currentRoundIndex == 0) ? CalculatePreSpawnedBotCount() : 0;
        totalBotsForRound = CalculateTotalBotCountForRound(round, preSpawnedCount);
    }

    /// <summary>
    /// Bắt đầu quy trình kết thúc round hiện tại.
    /// </summary>
    public void EndCurrentRound()
    {
        if (currentState != GameState.InRound) return;
        currentState = GameState.WaitingToStart;
        if (currentRoundIndex < 0 || currentRoundIndex >= levelRounds.Count) return;
        var completedRound = levelRounds[currentRoundIndex];
        StartCoroutine(RoundCompletionDelay(completedRound.DelayAfterComplete));
    }

    /// <summary>
    /// Coroutine chờ một khoảng thời gian trước khi bắt đầu round tiếp theo.
    /// </summary>
    /// <param name="delay">Thời gian chờ tính bằng giây.</param>
    private IEnumerator RoundCompletionDelay(float delay)
    {
        yield return HelperCoroutine.GetWait(delay);
        StartNextRound();
    }

    /// <summary>
    /// Xử lý khi tất cả các round đã hoàn thành.
    /// </summary>
    private void LevelCompleted()
    {
        currentState = GameState.LevelComplete;
        OnLevelComplete?.Invoke();
        
        // Chỉ end game khi đã hoàn thành tất cả các round
        Debug.Log("All rounds completed! Ending game...");
        EndGame();
    }

    private void EndGame()
    {
         EventManager.Instance?.Publish(new GameStateChangedEvent(GameConstants.GameState.GameWin));
    }

    #endregion

    #region Debug Editor
    /// <summary>
    /// Dừng tất cả các coroutine spawn đang chạy của GameManager.
    /// </summary>
    public void StopAllSpawningCoroutines()
    {
        StopAllCoroutines(); // Dừng tất cả coroutine trên GameObject này
    }
    /// <summary>
    /// Một hàm "cheat" chỉ dùng cho debug.
    /// Nó sẽ đặt số kill bằng tổng số bot để ngay lập tức thỏa mãn điều kiện thắng.
    /// </summary>
    public void ForceCompleteRoundObjective()
    {
        if (currentState == GameState.InRound)
        {
            // Gán thẳng số kill bằng tổng số.
            killedBotsForRound = totalBotsForRound;
            // Cập nhật UI lần cuối.
            ScheduleUIUpdate(killedBotsForRound, totalBotsForRound);
        }
    }
    #endregion
#if UNITY_EDITOR
    /// <summary>
    /// Hàm này chỉ chạy trong Editor, dùng để vẽ các thông tin gỡ lỗi (debug) lên màn hình game.
    /// </summary>
    // private void OnGUI()
    // {
    //     GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 400));
    //     GUILayout.Label("=== GAMEMANAGER DEBUG ===");
    //     GUILayout.Label($"Current Round: {currentRoundIndex + 1}/{levelRounds?.Count ?? 0}");
    //     GUILayout.Label($"State: {currentState}");
    //     GUILayout.Label($"Scripted Bots Spawned: {totalScriptedBots}/{totalScriptedBots}");
    //     GUILayout.Label($"Bots Killed: {killedBotsForRound}/{totalBotsForRound}");
    //     GUILayout.Label($"Total Bots (incl. Minions): {totalBotsForRound}");
    //     GUILayout.Label($"Cached Rounds: {roundDataCache.Count}");
    //     GUILayout.Label($"UI Update Pending: {uiUpdatePending}");
    //
    //     if (roundDataCache.TryGetValue(currentRoundIndex, out var cached))
    //     {
    //         GUILayout.Label($"Cached Total Bots: {cached.TotalBotCount}");
    //         GUILayout.Label($"Cached Scripted Bots: {cached.ScriptedBotCount}");
    //         GUILayout.Label($"Cached Pre-spawned Bots: {cached.PreSpawnedBotCount}");
    //     }
    //
    //     GUILayout.EndArea();
    // }
#endif



    public void OnNotify(GameStateChangedEvent data)
    {
        if(data.NewState == GameConstants.GameState.InGame)
        {
            StartGame();
        }
    }
}