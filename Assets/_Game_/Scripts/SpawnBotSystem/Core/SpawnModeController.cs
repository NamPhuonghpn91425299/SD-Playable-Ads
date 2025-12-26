using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Component controller để cấu hình và quản lý spawn mode trong runtime
/// </summary>
public class SpawnModeController : MonoBehaviour
{
    [Header("Spawn Mode Configuration")]
    [Tooltip("Chế độ spawn mặc định cho tất cả loại bot")]
    public SpawnMode defaultSpawnMode = SpawnMode.PingPong;
    
    [Header("Per-BotType Spawn Mode Override")]
    [Tooltip("Ghi đè chế độ spawn cho từng loại bot cụ thể")]
    public List<BotTypeSpawnModeOverride> botTypeOverrides = new List<BotTypeSpawnModeOverride>();
    
    [Header("Runtime Controls")]
    [Tooltip("Tự động áp dụng cài đặt khi game bắt đầu")]
    public bool autoApplyOnStart = true;
    
    [System.Serializable]
    public class BotTypeSpawnModeOverride
    {
        [Tooltip("Loại bot")]
        public GameConstants.BotMoveType botMoveType;
        [Tooltip("Chế độ spawn riêng cho loại bot này")]
        public SpawnMode spawnMode;
        [Tooltip("Kích hoạt override này")]
        public bool enabled = true;
    }
    
    #region Unity Lifecycle
    
    private void Start()
    {
        if (autoApplyOnStart)
        {
            ApplySpawnModeSettings();
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Áp dụng cài đặt spawn mode cho PathManager
    /// </summary>
    [ContextMenu("Apply Spawn Mode Settings")]
    public void ApplySpawnModeSettings()
    {
        if (PathManager.Instance == null)
        {
            Debug.LogError("SpawnModeController: PathManager.Instance is null!");
            return;
        }
        
        // Áp dụng chế độ mặc định
        PathManager.Instance.spawnMode = defaultSpawnMode;
        
        // TODO: Implement per-bot-type spawn mode trong tương lai nếu cần
        // Hiện tại PathManager chỉ hỗ trợ 1 spawn mode global
        
        Debug.Log($"SpawnModeController: Đã áp dụng spawn mode '{defaultSpawnMode}' cho PathManager");
        
        // Reset lại các index để bắt đầu từ đầu
        PathManager.Instance.ResetSpawnIndices();
    }
    
    /// <summary>
    /// Thay đổi spawn mode trong runtime
    /// </summary>
    /// <param name="newMode">Chế độ spawn mới</param>
    public void ChangeSpawnMode(SpawnMode newMode)
    {
        defaultSpawnMode = newMode;
        ApplySpawnModeSettings();
    }
    
    /// <summary>
    /// Thay đổi spawn mode trong runtime bằng int (để gọi từ UI)
    /// </summary>
    /// <param name="modeIndex">Index của enum SpawnMode</param>
    public void ChangeSpawnModeByIndex(int modeIndex)
    {
        if (modeIndex >= 0 && modeIndex < System.Enum.GetValues(typeof(SpawnMode)).Length)
        {
            SpawnMode newMode = (SpawnMode)modeIndex;
            ChangeSpawnMode(newMode);
        }
        else
        {
            Debug.LogError($"SpawnModeController: Invalid spawn mode index {modeIndex}");
        }
    }
    
    /// <summary>
    /// Reset tất cả spawn indices về trạng thái ban đầu
    /// </summary>
    [ContextMenu("Reset Spawn Indices")]
    public void ResetSpawnIndices()
    {
        if (PathManager.Instance != null)
        {
            PathManager.Instance.ResetSpawnIndices();
        }
    }
    
    #endregion
    
    #region Debug & Editor
    
#if UNITY_EDITOR
    [Header("Debug Info")]
    [SerializeField, TextArea(3, 5)]
    private string debugInfo = "Spawn Mode Controller\n" +
                              "- Random: Spawn ngẫu nhiên\n" +
                              "- Sequential: 0→1→2→3→0...\n" +
                              "- Reverse: 3→2→1→0→3...\n" +
                              "- PingPong: 0→1→2→3→2→1→0...\n" +
                              "- Cycle: 0→1→2→3→0→1→2...";
    
    /// <summary>
    /// Hiển thị thông tin debug trong Inspector
    /// </summary>
    private void OnValidate()
    {
        // Cập nhật debug info khi có thay đổi trong Editor
        if (!Application.isPlaying)
        {
            debugInfo = GetDebugInfo();
        }
    }
    
    private string GetDebugInfo()
    {
        string info = $"Current Spawn Mode: {defaultSpawnMode}\n\n";
        info += "Available Modes:\n";
        info += "- Random: Spawn ngẫu nhiên\n";
        info += "- Sequential: 0→1→2→3→0... (lặp lại)\n";
        info += "- Reverse: 3→2→1→0→3... (lặp ngược)\n";
        info += "- PingPong: 0→1→2→3→2→1→0... (qua lại)\n";
        info += "- Cycle: 0→1→2→3→0→1→2... (chu kỳ)\n\n";
        
        if (botTypeOverrides.Count > 0)
        {
            info += "Bot Type Overrides:\n";
            foreach (var Override in botTypeOverrides)
            {
                if (Override.enabled)
                {
                    info += $"- {Override.botMoveType}: {Override.spawnMode}\n";
                }
            }
        }
        
        return info;
    }
#endif
    
    #endregion
}
