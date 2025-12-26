using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Unity.EditorCoroutines.Editor;

/// <summary>
/// Cung cấp một cửa sổ Editor "Spawn Bot Tool" để gỡ lỗi và kiểm tra hệ thống spawn bot.
/// Công cụ này có thể được cấu hình ngay cả khi game chưa chạy và sẽ ghi nhớ các thiết lập.
/// </summary>
public class SpawnBotTool : EditorWindow
{
    //public EditorCoroutineUtility.EditorCoroutine activeSpawnCoroutine;
    #region EditorPrefs Keys

    // --- Các khóa (Keys) để lưu trữ dữ liệu trong EditorPrefs ---
    // Sử dụng const string để đảm bảo không gõ sai key.
    private const string BotTypeKey = "SpawnBotTool_BotType";
    private const string QuantityKey = "SpawnBotTool_Quantity";
    private const string DelayKey = "SpawnBotTool_Delay";

    #endregion

    #region Tool State

    // --- Trạng thái của Tool ---
    private BotType botTypeToSpawn;
    private int quantityToSpawn;
    private float delayBetweenSpawns;
    private EnemyBase selectedBot;
    private Vector2 scrollPosition;

    #endregion

    #region Window Management

    /// <summary>
    /// Tạo menu item để mở cửa sổ.
    /// </summary>
    [MenuItem("Tools/Spawn Bot Tool")]
    public static void ShowWindow()
    {
        GetWindow<SpawnBotTool>("Spawn Bot Tool");
    }

    /// <summary>
    /// Được gọi khi cửa sổ được mở hoặc khi code được biên dịch lại.
    /// Đây là nơi lý tưởng để tải các thiết lập đã lưu.
    /// </summary>
    private void OnEnable()
    {
        LoadSettings();
        // Đăng ký sự kiện để tự động cập nhật khi lựa chọn thay đổi.
        Selection.selectionChanged += OnSelectionChange;
        OnSelectionChange(); // Gọi một lần để khởi tạo
    }

    /// <summary>
    /// Được gọi khi cửa sổ bị đóng hoặc đối tượng bị hủy.
    /// </summary>
    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChange;
    }

    /// <summary>
    /// Được gọi khi lựa chọn trong Editor thay đổi.
    /// </summary>
    private void OnSelectionChange()
    {
        selectedBot = null;
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            selectedObject.TryGetComponent<EnemyBase>(out selectedBot);
        }
        Repaint();
    }

    #endregion

    #region GUI Drawing

    /// <summary>
    /// Vẽ giao diện của cửa sổ.
    /// </summary>
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        GUILayout.Label("Spawn Bot Tool", EditorStyles.boldLabel);
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Một số chức năng chỉ hoạt động khi game đang ở chế độ Play.", MessageType.Info);
        }

        // --- Khu vực SPAWN ---
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Manual Spawn", EditorStyles.centeredGreyMiniLabel);

        // Bắt đầu kiểm tra xem có sự thay đổi nào trong GUI không.
        EditorGUI.BeginChangeCheck();

        botTypeToSpawn = (BotType)EditorGUILayout.EnumPopup("Bot Type", botTypeToSpawn);
        quantityToSpawn = EditorGUILayout.IntSlider("Quantity", quantityToSpawn, 1, 50);
        delayBetweenSpawns = EditorGUILayout.Slider("Delay Between (s)", delayBetweenSpawns, 0f, 5f);
        
        // Nếu có bất kỳ sự thay đổi nào trong các trường trên...
        if (EditorGUI.EndChangeCheck())
        {
            // ...lưu lại các thiết lập.
            SaveSettings();
        }

        // Chỉ bật nút Spawn khi game đang chạy.
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("Spawn Bot(s)", GUILayout.Height(30)))
        {
            EditorCoroutineUtility.StartCoroutine(SpawnBotsWithDelay(), this);
        }
        GUI.enabled = true; // Bật lại GUI cho các phần còn lại.

        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Các chức năng bên dưới chỉ có ý nghĩa khi game đang chạy.
        GUI.enabled = Application.isPlaying;

        // --- Khu vực COMMANDS ---
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Commands", EditorStyles.centeredGreyMiniLabel);
        
        bool canExecuteSelected = Application.isPlaying && selectedBot != null;
        GUI.enabled = canExecuteSelected;
        string killSelectedButtonText = (selectedBot != null) ? $"Kill Selected Bot ({selectedBot.name})" : "Kill Selected Bot (None)";
        if (GUILayout.Button(killSelectedButtonText)) {
                                                            var damageInfo = new DamageInfo()
                                                            {
                                                                damage = selectedBot.currentHealth,
                                                                damageType = DamageType.Normal
                                                            };
                                                            selectedBot.OnTakeDamage(damageInfo); 
        }
        GUI.enabled = Application.isPlaying;

        if (GUILayout.Button("Kill All Bots on Scene")) { KillAllBotsOnScene(); }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // --- Khu vực ROUND CONTROLS ---
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Round Controls", EditorStyles.centeredGreyMiniLabel);
        if (GUILayout.Button("Next Round"))
        {
            ForceWinRound();
        }
        EditorGUILayout.EndVertical();
        
        GUI.enabled = true;
        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region Settings Persistence

    /// <summary>
    /// Tải các thiết lập từ EditorPrefs.
    /// </summary>
    private void LoadSettings()
    {
        // GetInt/GetFloat có một giá trị mặc định nếu không tìm thấy khóa.
        botTypeToSpawn = (BotType)EditorPrefs.GetInt(BotTypeKey, 0); // 0 là giá trị của enum đầu tiên
        quantityToSpawn = EditorPrefs.GetInt(QuantityKey, 1);
        delayBetweenSpawns = EditorPrefs.GetFloat(DelayKey, 0.2f);
    }

    /// <summary>
    /// Lưu các thiết lập hiện tại vào EditorPrefs.
    /// </summary>
    private void SaveSettings()
    {
        EditorPrefs.SetInt(BotTypeKey, (int)botTypeToSpawn);
        EditorPrefs.SetInt(QuantityKey, quantityToSpawn);
        EditorPrefs.SetFloat(DelayKey, delayBetweenSpawns);
    }

    #endregion

    #region Tool Logic

    /// <summary>
    /// Logic spawn giờ là một Coroutine để có thể xử lý delay.
    /// </summary>
    private IEnumerator SpawnBotsWithDelay()
    {
        if (!Application.isPlaying || BotSpawnManager.Instance == null) yield break;
        
        BotDefinition definition = BotSpawnManager.Instance?.GetDefinitionForType(botTypeToSpawn);
        if (definition == null) yield break;

        var order = BotSpawnOrder.Get();
        order.BotTypeToSpawn = botTypeToSpawn;
        order.BotMoveType = definition.BotMoveType; 
        order.IsFromRoundScript = false; 

        // Vòng lặp để spawn từng con một.
        for (int i = 0; i < quantityToSpawn; i++)
        {
            if (!Application.isPlaying || BotSpawnManager.Instance == null) yield break;
            // Ghi lại thời điểm bắt đầu của lần spawn này.
            // EditorApplication.timeSinceStartup là một bộ đếm thời gian đáng tin cậy trong Editor.
            double startTime = EditorApplication.timeSinceStartup;

            // Ra lệnh spawn MỘT con bot.
            BotSpawnManager.Instance?.ExecuteSpawnOrder(order);
            
            // Nếu đây không phải là con bot cuối cùng và có độ trễ...
            if (i < quantityToSpawn - 1 && delayBetweenSpawns > 0)
            {
                // ...tính toán thời điểm mà chúng ta được phép spawn con tiếp theo.
                double nextSpawnTime = startTime + delayBetweenSpawns;

                // Vòng lặp chờ: Coroutine sẽ bị "kẹt" ở đây cho đến khi thời gian hiện tại
                // vượt qua thời điểm được phép spawn tiếp theo.
                while (EditorApplication.timeSinceStartup < nextSpawnTime)
                {
                    // `yield return null` cực kỳ quan trọng: Nó trả quyền kiểm soát cho Editor
                    // để nó không bị đóng băng, và sẽ quay lại kiểm tra ở lần cập nhật tiếp theo.
                    yield return null;
                }
            }
        }
        
        BotSpawnOrder.Return(order);
    }
    
    /// <summary>
    /// Chỉ giết những con bot đang thực sự có mặt trên scene.
    /// </summary>
    private void KillAllBotsOnScene()
    {
        // if (!Application.isPlaying) return;
        // var activeBots = FindObjectsOfType<BotIdentity>().ToList();
        // Debug.Log($"[Spawn Bot Tool] Kill {activeBots.Count} bots on scene...");
        // foreach (var bot in activeBots)
        // {
        //     if (bot != null) bot.Bot_ReportKill();
        // }
        KillAllBots();
    }
    public static void KillAllBots()
    {
        if (BotSpawnManager.Instance == null) return;
    
        // Tạo bản copy của list để tránh lỗi khi sửa đổi trong vòng lặp
        var botsToKill = new List<Transform>(BotSpawnManager.Instance.botInScene);
    
        foreach (var botTransform in botsToKill)
        {
            if (botTransform == null) continue;
        
            var enemyBase = botTransform.GetComponentInParent<EnemyBase>();
            if (enemyBase == null)
                enemyBase = botTransform.GetComponent<EnemyBase>();
            
            if (enemyBase != null && !enemyBase.IsDead)
            {
                // Ép buộc giết bot bằng cách set máu = 0
                var damageInfo = new DamageInfo()
                {
                    damage = enemyBase.currentHealth,
                    damageType = DamageType.Normal
                };
                enemyBase.OnTakeDamage(damageInfo);
            }
        }
    }
    /// <summary>
    /// Thực hiện logic "Tự Hủy Toàn Phần" để thắng round ngay lập tức.
    /// </summary>
    private void ForceWinRound()
    {
        if (!Application.isPlaying || GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.InRound)
        {
            Debug.LogWarning("[Spawn Bot Tool] Cannot force win: Game is not in a round.");
            return;
        }

        Debug.LogWarning("[Spawn Bot Tool] ROUND COMPLETION!");

        GameManager.Instance.StopAllSpawningCoroutines();
        var activeMinionSpawners = FindObjectsOfType<MinionSpawner>().ToList();
        foreach (var spawner in activeMinionSpawners)
        {
            spawner.StopAllSpawningCoroutines();
        }
        
        KillAllBotsOnScene();

        EditorApplication.delayCall += GameManager.Instance.ForceCompleteRoundObjective;
    }
    
    /// <summary>
    /// Giúp cửa sổ tự động cập nhật.
    /// </summary>
    void OnInspectorUpdate() { Repaint(); }

    #endregion
}