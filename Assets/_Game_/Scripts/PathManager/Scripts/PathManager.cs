using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Cần thiết cho .ToList() và .Where()
using static GameConstants;
#if UNITY_EDITOR
using UnityEditor; // Cần cho các hàm chỉ chạy trong Editor
#endif

/// <summary>
/// Quản lý tất cả các tuyến đường (PointGroup) trong màn chơi.
/// Đây là một hệ thống tối ưu hóa cao, sử dụng caching để cung cấp đường đi cho bot
/// với hiệu năng O(1) trong hầu hết các trường hợp.
/// </summary>
public class PathManager : MonoBehaviour
{
    #region Singleton
    
    /// <summary>
    /// Singleton Instance để dễ dàng truy cập từ bất kỳ đâu thông qua `PathManager.Instance`.
    /// </summary>
    public static PathManager Instance { get; private set; }

    #endregion

    #region Fields & Properties
    
    [Header("Game Settings")]
    [Tooltip("Chế độ chơi hiện tại, sẽ ảnh hưởng đến cách chọn đường đi.")]
    public GameConstants.PlayMode currentPlayMode;
    
    [Header("Spawn Settings")]
    [Tooltip("Chế độ spawn đường đi: Random, Sequential, Reverse, PingPong, hoặc Cycle")]
    public SpawnMode spawnMode = SpawnMode.Sequential;

    [Header("Path Data Source")]
    [Tooltip("Đây là nguồn dữ liệu thô, chứa tất cả các tuyến đường được tìm thấy trong Scene. " +
             "Dữ liệu này được lưu lại khi bạn save scene, giúp khởi động game cực nhanh.")]
    [SerializeField] private List<PointGroup> allAvailableRoutes;

    /// <summary>
    /// Cấu trúc dữ liệu chính để truy cập nhanh các tuyến đường.
    /// Key: Loại Bot (ví dụ: Tank). Value: Một danh sách chứa tất cả các PointGroup cho loại bot đó.
    /// </summary>
    [HideInInspector]
    public Dictionary<BotMoveType, List<PointGroup>> classifiedRoutes = new Dictionary<BotMoveType, List<PointGroup>>();

    // --- CÁC HỆ THỐNG CACHING ĐỂ TỐI ƯƠ HÓA ---
    
    private Dictionary<BotMoveType, List<PointGroup>> unusedPathsCache = new Dictionary<BotMoveType, List<PointGroup>>();
    private Dictionary<BotMoveType, Queue<PointGroup>> sequentialPathQueues = new Dictionary<BotMoveType, Queue<PointGroup>>();
    
    // --- HỆ THỐNG QUẢN LÝ SPAWN MODES ---
    
    /// <summary>
    /// Quản lý index hiện tại cho mỗi loại bot trong chế độ Sequential/Reverse/PingPong
    /// </summary>
    private Dictionary<BotMoveType, int> currentPathIndices = new Dictionary<BotMoveType, int>();
    
    /// <summary>
    /// Quản lý hướng spawn cho PingPong mode (true = tiến, false = lùi)
    /// </summary>
    private Dictionary<BotMoveType, bool> pingPongDirections = new Dictionary<BotMoveType, bool>();

    #endregion

    #region Unity Lifecycle Methods

    private void Awake()
    {
        // Thiết lập Singleton
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        InitializeAndFillDictionary();
        InitializePathCaches();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Lấy một tuyến đường phù hợp cho một loại bot.
    /// Hàm này được tối ưu hóa cao để có tốc độ phản hồi gần như ngay lập tức.
    /// </summary>
    /// <param name="moveType">Loại bot cần đường đi.</param>
    /// <returns>Một PointGroup phù hợp, hoặc null nếu không có.</returns>
    public PointGroup GetPath(BotMoveType moveType)
    {
        if (!classifiedRoutes.ContainsKey(moveType) || classifiedRoutes[moveType].Count == 0)
        {
            Debug.LogError($"PathManager: Không có tuyến đường nào được định nghĩa cho loại '{moveType}'!");
            return null;
        }

        // Sử dụng spawn mode mới thay vì chế độ cũ
        return GetPathBySpawnMode(moveType);
    }

    /// <summary>
    /// "Thả" hoặc "trả lại" một tuyến đường đã dùng xong, cho phép các bot khác sử dụng nó.
    /// </summary>
    /// <param name="path">PointGroup cần được trả lại.</param>
    public void ReleasePath(PointGroup path)
    {
        if (path == null || !path.isBeingUsed) return;

        path.isBeingUsed = false;
        
        BotMoveType moveType = path.botMoveType;
        if (unusedPathsCache.ContainsKey(moveType) && !unusedPathsCache[moveType].Contains(path))
        {
            unusedPathsCache[moveType].Add(path);
        }
        
        if (sequentialPathQueues.ContainsKey(moveType) && !sequentialPathQueues[moveType].Contains(path))
        {
            sequentialPathQueues[moveType].Enqueue(path);
        }
        
        Debug.Log($"Đã giải phóng đường đi '{path.name}' cho loại bot '{moveType}'.");
    }

    /// <summary>
    /// Reset lại trạng thái 'isBeingUsed' của tất cả các tuyến đường và xây dựng lại cache.
    /// </summary>
    [ContextMenu("Execute - Reset All Path States")]
    public void ResetAllPathStates()
    {
        Debug.Log("PathManager: Đang reset trạng thái tất cả các đường đi...");
        
        if (allAvailableRoutes != null)
        {
            foreach (var route in allAvailableRoutes)
            {
                if(route != null) route.isBeingUsed = false;
            }
        }
        
        InitializePathCaches();
        Debug.Log("Reset hoàn tất và cache đã được xây dựng lại.");
    }

    /// <summary>
    /// Đặt lại tất cả các index spawn về trạng thái ban đầu
    /// </summary>
    [ContextMenu("Execute - Reset Spawn Indices")]
    public void ResetSpawnIndices()
    {
        currentPathIndices.Clear();
        pingPongDirections.Clear();
        
        // Khởi tạo lại các giá trị mặc định
        foreach (BotMoveType type in System.Enum.GetValues(typeof(BotMoveType)))
        {
            currentPathIndices[type] = 0;
            pingPongDirections[type] = true; // Bắt đầu với hướng tiến
        }
        
        Debug.Log("PathManager: Đã reset tất cả spawn indices về trạng thái ban đầu.");
    }

    #endregion
    
    #region Private Logic
    
    /// <summary>
    /// Hàm chính xử lý spawn path theo các chế độ khác nhau
    /// </summary>
    private PointGroup GetPathBySpawnMode(BotMoveType moveType)
    {
        List<PointGroup> availablePaths = classifiedRoutes[moveType];
        
        switch (spawnMode)
        {
            case SpawnMode.Random:
                return GetPathRandom(moveType, availablePaths);
                
            case SpawnMode.Sequential:
                return GetPathSequential(moveType, availablePaths);
                
            case SpawnMode.Reverse:
                return GetPathReverse(moveType, availablePaths);
                
            case SpawnMode.PingPong:
                return GetPathPingPong(moveType, availablePaths);
                
            case SpawnMode.Cycle:
                return GetPathCycle(moveType, availablePaths);
                
            default:
                Debug.LogWarning($"PathManager: Spawn mode '{spawnMode}' không được hỗ trợ. Sử dụng Random.");
                return GetPathRandom(moveType, availablePaths);
        }
    }
    
    /// <summary>
    /// Spawn ngẫu nhiên (chế độ cũ)
    /// </summary>
    private PointGroup GetPathRandom(BotMoveType moveType, List<PointGroup> availablePaths)
    {
        List<PointGroup> unusedPaths = unusedPathsCache[moveType];
        
        if (unusedPaths.Count > 0)
        {
            int randomIndex = Random.Range(0, unusedPaths.Count);
            PointGroup chosenPath = unusedPaths[randomIndex];
            
            chosenPath.isBeingUsed = true;
            unusedPaths.RemoveAt(randomIndex);
            
            Debug.Log($"[Random] Gán đường đi '{chosenPath.name}' cho '{moveType}'");
            return chosenPath;
        }
        
        // Nếu hết đường chưa dùng, chọn ngẫu nhiên
        int sharedIndex = Random.Range(0, availablePaths.Count);
        Debug.LogWarning($"[Random] Hết đường trống, dùng chung '{availablePaths[sharedIndex].name}' cho '{moveType}'");
        return availablePaths[sharedIndex];
    }
    
    /// <summary>
    /// Spawn tuần tự từ index 0 đến cuối
    /// </summary>
    private PointGroup GetPathSequential(BotMoveType moveType, List<PointGroup> availablePaths)
    {
        if (!currentPathIndices.ContainsKey(moveType))
        {
            currentPathIndices[moveType] = 0;
        }
        
        int currentIndex = currentPathIndices[moveType];
        PointGroup selectedPath = availablePaths[currentIndex];
        
        // Tăng index cho lần tiếp theo
        currentPathIndices[moveType] = (currentIndex + 1) % availablePaths.Count;
        
        //Debug.Log($"[Sequential] Gán đường đi index {currentIndex} '{selectedPath.name}' cho '{moveType}'");
        return selectedPath;
    }
    
    /// <summary>
    /// Spawn ngược từ index cuối về 0
    /// </summary>
    private PointGroup GetPathReverse(BotMoveType moveType, List<PointGroup> availablePaths)
    {
        if (!currentPathIndices.ContainsKey(moveType))
        {
            currentPathIndices[moveType] = availablePaths.Count - 1; // Bắt đầu từ cuối
        }
        
        int currentIndex = currentPathIndices[moveType];
        PointGroup selectedPath = availablePaths[currentIndex];
        
        // Giảm index cho lần tiếp theo, wrap around khi về 0
        currentPathIndices[moveType] = currentIndex == 0 ? availablePaths.Count - 1 : currentIndex - 1;
        
        Debug.Log($"[Reverse] Gán đường đi index {currentIndex} '{selectedPath.name}' cho '{moveType}'");
        return selectedPath;
    }
    
    /// <summary>
    /// Spawn ping-pong: 0->1->2->3->2->1->0->1->2...
    /// </summary>
    private PointGroup GetPathPingPong(BotMoveType moveType, List<PointGroup> availablePaths)
    {
        if (!currentPathIndices.ContainsKey(moveType))
        {
            currentPathIndices[moveType] = 0;
            pingPongDirections[moveType] = true; // Bắt đầu với hướng tiến
        }
        
        int currentIndex = currentPathIndices[moveType];
        bool isForward = pingPongDirections[moveType];
        PointGroup selectedPath = availablePaths[currentIndex];
        
        // Tính toán index tiếp theo
        if (availablePaths.Count == 1)
        {
            // Nếu chỉ có 1 đường, luôn trả về đường đó
        }
        else if (isForward)
        {
            if (currentIndex == availablePaths.Count - 1)
            {
                // Đã đến cuối, chuyển sang hướng ngược
                currentPathIndices[moveType] = currentIndex - 1;
                pingPongDirections[moveType] = false;
            }
            else
            {
                currentPathIndices[moveType] = currentIndex + 1;
            }
        }
        else // isBackward
        {
            if (currentIndex == 0)
            {
                // Đã về đầu, chuyển sang hướng tiến
                currentPathIndices[moveType] = currentIndex + 1;
                pingPongDirections[moveType] = true;
            }
            else
            {
                currentPathIndices[moveType] = currentIndex - 1;
            }
        }
        
        string direction = isForward ? "tiến" : "lùi";
        Debug.Log($"[PingPong] Gán đường đi index {currentIndex} '{selectedPath.name}' cho '{moveType}' (hướng {direction})");
        return selectedPath;
    }
    
    /// <summary>
    /// Spawn cycle: 0->1->2->3->0->1->2->3...
    /// </summary>
    private PointGroup GetPathCycle(BotMoveType moveType, List<PointGroup> availablePaths)
    {
        if (!currentPathIndices.ContainsKey(moveType))
        {
            currentPathIndices[moveType] = 0;
        }
        
        int currentIndex = currentPathIndices[moveType];
        PointGroup selectedPath = availablePaths[currentIndex];
        
        // Tăng index và wrap around
        currentPathIndices[moveType] = (currentIndex + 1) % availablePaths.Count;
        
        Debug.Log($"[Cycle] Gán đường đi index {currentIndex} '{selectedPath.name}' cho '{moveType}'");
        return selectedPath;
    }
    
    #endregion

    #region Initialization and Data Handling

    /// <summary>
    /// Xây dựng lại các hệ thống cache từ đầu.
    /// </summary>
    private void InitializePathCaches()
    {
        // Clear tất cả Lists và Queues thay vì Clear() toàn bộ Dictionary
        foreach (var kvp in unusedPathsCache)
        {
            kvp.Value.Clear();
        }
        foreach (var kvp in sequentialPathQueues)
        {
            kvp.Value.Clear();
        }
        
        // Khởi tạo các hệ thống spawn mode mới
        currentPathIndices.Clear();
        pingPongDirections.Clear();
        
        if (classifiedRoutes == null) return;
        
        foreach (BotMoveType type in System.Enum.GetValues(typeof(BotMoveType)))
        {
            // Chỉ tạo mới nếu chưa tồn tại
            if (!unusedPathsCache.ContainsKey(type))
                unusedPathsCache[type] = new List<PointGroup>();
            
            if (!sequentialPathQueues.ContainsKey(type))
                sequentialPathQueues[type] = new Queue<PointGroup>();
                
            // Khởi tạo spawn mode indices
            currentPathIndices[type] = 0;
            pingPongDirections[type] = true; // Bắt đầu với hướng tiến
            
            if (classifiedRoutes.ContainsKey(type))
            {
                List<PointGroup> sortedPaths = classifiedRoutes[type];
                sortedPaths.Sort((a, b) => a.name.CompareTo(b.name));

                foreach (var path in sortedPaths)
                {
                    if (path != null && !path.isBeingUsed)
                    {
                        unusedPathsCache[type].Add(path);
                        sequentialPathQueues[type].Enqueue(path);
                    }
                }
            }
        }
        
        Debug.Log($"PathManager: Đã khởi tạo cache và spawn mode indices. Chế độ hiện tại: {spawnMode}");
    }

    /// <summary>
    /// Nạp dữ liệu từ danh sách được lưu trong Scene vào Dictionary.
    /// </summary>
    private void InitializeAndFillDictionary()
    {
        // Chỉ tạo mới Dictionary nếu chưa tồn tại
        if (classifiedRoutes == null)
        {
            classifiedRoutes = new Dictionary<BotMoveType, List<PointGroup>>();
        }
        else
        {
            // Clear các List bên trong thay vì tạo mới Dictionary
            foreach (var kvp in classifiedRoutes)
            {
                kvp.Value.Clear();
            }
        }
        
        foreach (BotMoveType type in System.Enum.GetValues(typeof(BotMoveType)))
        {
            // Chỉ tạo List mới nếu chưa tồn tại
            if (!classifiedRoutes.ContainsKey(type))
                classifiedRoutes[type] = new List<PointGroup>();
        }

        if (allAvailableRoutes == null) return;
        
        foreach (var route in allAvailableRoutes)
        {
            if (route != null)
            {
                classifiedRoutes[route.botMoveType].Add(route);
            }
        }
    }

    #endregion

    #region Editor-Only Methods

    #if UNITY_EDITOR
    /// <summary>
    /// Được gọi trong Editor mỗi khi có thay đổi trong Inspector.
    /// </summary>
    // private void OnValidate()
    // {
    //     // Chỉ chạy khi không ở Play Mode
    //     if (!Application.isPlaying)
    //     {
    //         // Dùng delayCall để tránh việc hàm bị gọi nhiều lần liên tiếp khi kéo chuột
    //         EditorApplication.delayCall -= CollectAllRoutesInScene;
    //         EditorApplication.delayCall += CollectAllRoutesInScene;
    //
    //     }
    // }

    /// <summary>
    /// Tìm và lưu tất cả các PointGroup trong scene. Có thể gọi bằng tay qua menu Context.
    /// </summary>
    [ContextMenu("FORCE REFRESH - Find All Routes in Scene")]
    public void CollectAllRoutesInScene()
    {
        // Tránh lỗi nếu đối tượng bị xóa trong lúc delayCall chờ được thực thi
        if (this == null) return;

        Debug.Log("PathManager [Editor]: Đang tìm tất cả PointGroups trong scene...");
        
        // Dùng Linq.Where để lọc ra các phần tử null một cách an toàn
        allAvailableRoutes = FindObjectsOfType<PointGroup>().Where(route => route != null).ToList();
        // Sắp xếp theo tên để đảm bảo thứ tự luôn nhất quán mỗi khi refresh
        allAvailableRoutes.Sort((a, b) => a.name.CompareTo(b.name));

        Debug.Log($"PathManager [Editor]: Tìm thấy {allAvailableRoutes.Count} tuyến đường.");
        
        // Nạp ngay dữ liệu vào Dictionary
        InitializeAndFillDictionary();
        
        // Đánh dấu đối tượng là "bẩn" (dirty) để Unity biết cần phải lưu lại thay đổi này vào file scene.
        EditorUtility.SetDirty(this);
    }
    #endif
    
    #endregion
}

