using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// LÀ NHÀ MÁY SPAWN BOT, CHỈ LÀM MỘT VIỆC: TẠO RA MỘT CON BOT KHI ĐƯỢC LỆNH.
/// Vai trò của class này rất chuyên biệt: nhận một mệnh lệnh spawn đã sẵn sàng và hiện thực hóa nó.
/// Nó không quản lý vòng lặp, delay hay các điều kiện phức tạp; những việc đó thuộc về GameManager và MinionSpawner.
/// </summary>
public class BotSpawnManager : MonoBehaviour
{
    #region Singleton & Fields

    public static BotSpawnManager Instance { get; private set; }
    
    [Tooltip("Danh sách các 'bản thiết kế' bot. Game Designer sẽ kéo các file BotDefinition.asset vào đây.")]
    [FormerlySerializedAs("spawnableDefinitions")]
    [SerializeField] private List<BotDefinition> botDefinitions;
    public List<Transform> botInScene = new List<Transform>(); // Danh sách các bot đã spawn trong scene, dùng để kiểm tra khi spawn bot mới
    private HashSet<ISpawnable> activeSpawns = new HashSet<ISpawnable>();
    private Dictionary<BotType, BotDefinition> definitionMap = new Dictionary<BotType, BotDefinition>(16);
    private Dictionary<GameObject, ComponentCache> componentCache = new Dictionary<GameObject, ComponentCache>(100);
    
    #endregion

    #region Events & Properties

    public event Action<ISpawnable> OnBotSpawned;
    public event Action<ISpawnable> OnBotKilled;
    public int ActiveSpawnCount => activeSpawns.Count;

    private struct ComponentCache
    {
        public BotIdentity botIdentity;
        public MinionSpawner Spawner;
        
        public ComponentCache(GameObject go)
        {
            botIdentity = go.GetComponent<BotIdentity>();
            Spawner = go.GetComponent<MinionSpawner>();
        }
    }

    #endregion

    #region Unity Lifecycle Methods

    /// <summary>
    /// Khởi tạo Singleton và xây dựng cache definition một lần duy nhất.
    /// </summary>
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        if (botDefinitions?.Count > 0)
        {
            foreach (var def in botDefinitions)
            {
                if (def.BotType != BotType.None && !definitionMap.ContainsKey(def.BotType))
                {
                    definitionMap.Add(def.BotType, def);
                }
            }
        }
    }

    /// <summary>
    /// Dọn dẹp tất cả các cache khi đối tượng bị phá hủy để tránh rò rỉ bộ nhớ.
    /// </summary>
    private void OnDestroy()
    {
        componentCache.Clear();
        activeSpawns.Clear();
        definitionMap.Clear();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Lấy ra "bản thiết kế" (BotDefinition) cho một loại bot cụ thể từ cache.
    /// </summary>
    /// <param name="botType">Loại bot cần tìm.</param>
    /// <returns>BotDefinition tương ứng hoặc null nếu không tìm thấy.</returns>
    public BotDefinition GetDefinitionForType(BotType botType)
    {
        definitionMap.TryGetValue(botType, out var definition);
        return definition;
    }

    /// <summary>
    /// Thực thi một mệnh lệnh spawn, tạo ra MỘT con bot ngay lập tức.
    /// </summary>
    /// <param name="order">Mệnh lệnh spawn chứa thông tin về loại bot và đường đi.</param>
    public void ExecuteSpawnOrder(BotSpawnOrder order)
    {
        if (!definitionMap.TryGetValue(order.BotTypeToSpawn, out var definition))
        {
            Debug.LogError($"Spawn failed: Definition for BotType '{order.BotTypeToSpawn}' not found!");
            return;
        }
        
        // Null check cho PathManager.Instance
        if (PathManager.Instance == null)
        {
            Debug.LogError("PathManager.Instance is null! Cannot spawn bot.");
            return;
        }

        PointGroup pathToFollow = PathManager.Instance.GetPath(order.BotMoveType);
        
        if (pathToFollow == null || pathToFollow.points.Count == 0)
        {
            Debug.LogError($"Không thể spawn bot vì không tìm thấy đường đi hợp lệ cho loại '{order.BotMoveType}'");
            return;
        }
        
        Vector3 spawnPosition = pathToFollow.points[0].position;
        Quaternion spawnRotation = (pathToFollow.points.Count > 1) 
            ? Quaternion.LookRotation(pathToFollow.points[1].position - spawnPosition) 
            : Quaternion.identity;

        EnemyBase enemyBaseNew = SimplePool<BotType>.Spawn<EnemyBase>(definition.BotType,spawnPosition,spawnRotation);
        GameObject go = enemyBaseNew.gameObject;
        // GameObject go = Instantiate(definition.Prefab, spawnPosition, spawnRotation);
        
        if (!componentCache.TryGetValue(go, out var cachedComponents))
        {
            cachedComponents = new ComponentCache(go);
            componentCache[go] = cachedComponents;
        }

        if (cachedComponents.botIdentity == null)
        {
            Debug.LogError($"Spawn failed: Prefab '{definition.Prefab.name}' is missing a BotIdentity component!");
            componentCache.Remove(go);
            Destroy(go);
            return;
        }
        cachedComponents.botIdentity.Bot_Initialize(definition.BotType, definition.BotMoveType, definition.Type, order.IsFromRoundScript, pathToFollow);
        TrackSpawnedBot(cachedComponents.botIdentity);
        enemyBaseNew.OnInit();
        if (enemyBaseNew.stateController != null)
        {
            if(enemyBaseNew.stateController.OnInitEqualStart)
                enemyBaseNew.stateController?.OnInit(GameConstants.EnemyState.Start);
            else
                enemyBaseNew.stateController?.OnInit(GameConstants.EnemyState.Move);
        }
        botInScene.Add(enemyBaseNew.GetTransformCenter());
    }
    //spawn bot con trong state của 1 bot cha
    public EnemyBase ExecuteSpawnOrder(BotDefinition order,Transform TFSpawn,PointGroup pathToFollow, bool isFromRoundScript)
    {
        // Null check cho order
        if (order == null)
        {
            Debug.LogError("BotDefinition order is null!");
            return null;
        }
        
        if (!definitionMap.TryGetValue(order.BotType, out var definition))
        {
            Debug.LogError($"Spawn failed: Definition for BotType '{order.BotType}' not found!");
            return null;
        }
#if UNITY_EDITOR
        if (pathToFollow.points.Count == 0)
            Debug.LogWarning($"{order.Prefab.name} Đang không có điểm di chuyển nào trong đường đi {pathToFollow.name}");
#endif
        if (pathToFollow == null)
        {
            Debug.LogError($"Không thể spawn bot vì không tìm thấy đường đi hợp lệ cho loại '{order.BotMoveType}'");
            return null;
        }

        EnemyBase enemyBaseNew = SimplePool<BotType>.Spawn<EnemyBase>(definition.BotType, TFSpawn.position, TFSpawn.rotation, TFSpawn);
        GameObject go = enemyBaseNew.gameObject;
        
        if (!componentCache.TryGetValue(go, out var cachedComponents))
        {
            cachedComponents = new ComponentCache(go);
            componentCache[go] = cachedComponents;
        }

        if (cachedComponents.botIdentity == null)
        {
            Debug.LogError($"Spawn failed: Prefab '{definition.Prefab.name}' is missing a BotIdentity component!");
            componentCache.Remove(go);
            Destroy(go);
            return null;
        }
        cachedComponents.botIdentity.Bot_Initialize(definition.BotType, definition.BotMoveType, definition.Type, isFromRoundScript, pathToFollow);
        TrackSpawnedBot(cachedComponents.botIdentity);
        botInScene.Add(enemyBaseNew.GetTransformCenter());
        return enemyBaseNew;
    }
    
    /// <summary>
    /// "Đăng ký khai sinh" và bắt đầu theo dõi một con bot.
    /// </summary>
    /// <param name="spawnable">Interface của con bot cần được theo dõi.</param>
    public void TrackSpawnedBot(ISpawnable spawnable)
    {
        if(spawnable.Type==SpawnableType.Bot)
            activeSpawns.Add(spawnable);
        spawnable.OnBotDeathReported += HandleBotDeathReported;
        OnBotSpawned?.Invoke(spawnable);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Hàm xử lý nội bộ khi nhận được tín hiệu một con bot báo cáo về cái chết.
    /// </summary>
    /// <param name="spawnable">Interface của con bot đã báo cáo về cái chết.</param>
    private void HandleBotDeathReported(ISpawnable spawnable)
    {
        // Null check cho spawnable
        if (spawnable == null) return;
        
        spawnable.OnBotDeathReported -= HandleBotDeathReported;
        
        if(spawnable.Type==SpawnableType.Bot)
            activeSpawns.Remove(spawnable);
        
        if (spawnable.GameObject != null)
        {
            componentCache.Remove(spawnable.GameObject);
        }
        
        OnBotKilled?.Invoke(spawnable);
        
        // if (spawnable.GameObject != null) 
        //     Destroy(spawnable.GameObject);
    }

    #endregion
    
}