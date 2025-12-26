using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Một trung tâm đăng ký tĩnh, hiệu năng cao để theo dõi các đối tượng PreSpawnedBot.
/// Thay thế hoàn toàn sự cần thiết của FindObjectsOfType.
/// </summary>
public static class SpawnableRegistry
{
    private static readonly List<PreSpawnedBot> preSpawnedBots = new List<PreSpawnedBot>();
    private static bool isInitialized = false;
    
    // Cung cấp một phiên bản chỉ đọc ra bên ngoài để đảm bảo an toàn.
    public static IReadOnlyList<PreSpawnedBot> AllPreSpawnedBots => preSpawnedBots;
    
    public static void Register(PreSpawnedBot bot)
    {
        if (!isInitialized)
        {
            Initialize();
        }
        
        if (bot != null && !preSpawnedBots.Contains(bot))
        {
            preSpawnedBots.Add(bot);
        }
    }
    
    public static void Unregister(PreSpawnedBot bot)
    {
        if (bot != null)
        {
            preSpawnedBots.Remove(bot);
        }
    }
    
    // Method public để clear registry khi cần (ví dụ: khi unload scene)
    public static void Clear()
    {
        preSpawnedBots.Clear();
    }
    
    // Initialize và đăng ký scene callbacks
    private static void Initialize()
    {
        if (isInitialized) return;
        
        isInitialized = true;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        
        // Đăng ký cleanup khi application quit
        Application.quitting += OnApplicationQuit;
    }
    
    // Cleanup khi scene unload
    private static void OnSceneUnloaded(Scene scene)
    {
        // Clear list để tránh giữ references tới objects đã bị destroy
        Clear();
    }
    
    // Cleanup khi application quit
    private static void OnApplicationQuit()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        Application.quitting -= OnApplicationQuit;
        Clear();
        isInitialized = false;
    }
    
    // Đảm bảo danh sách tĩnh luôn sạch sẽ giữa các lần chạy trong Editor.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearStaticData()
    {
        // Unsubscribe từ events cũ nếu có
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        Application.quitting -= OnApplicationQuit;
        
        // Clear data và reset state
        preSpawnedBots.Clear();
        isInitialized = false;
    }
}
