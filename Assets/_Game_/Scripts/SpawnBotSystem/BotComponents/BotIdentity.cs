// FileName: SpawnableWrapper.cs
using System;
using UnityEngine;
using System.Collections.Generic;
using static GameConstants;
/// <summary>
/// LÀ TẤM THẺ "CĂN CƯỚC" CHO MỌI ĐỐI TƯỢNG CÓ THỂ SPAWN.
/// Hiện thực hóa interface ISpawnable, cung cấp một cách chung để các hệ thống khác
/// có thể lấy thông tin cơ bản về một đối tượng, bao gồm cả đường đi của nó.
/// </summary>
public class BotIdentity : MonoBehaviour, ISpawnable
{
    // --- Các thuộc tính cũ ---
    public BotType BotType { get; private set; }
    
    public BotMoveType BotMoveType { get; private set; }
    public SpawnableType Type { get; private set; }
    public bool IsFromRoundScript { get; private set; }
    public GameObject GameObject => this.gameObject; 
    public PointGroup AssignedPath { get; set; }
    public List<Transform> Waypoints = new List<Transform>(); 
    public event Action<ISpawnable> OnBotDeathReported;

    /// <summary>
    /// GIẢI THÍCH: Đã được cập nhật để nhận thêm tham số `PointGroup`.
    /// Hàm này được gọi bởi BotSpawnManager ngay sau khi đối tượng được Instantiate.
    /// Nó "điền" tất cả thông tin vào tấm thẻ căn cước này.
    /// </summary>
    public void Bot_Initialize(BotType botType, BotMoveType botMoveType, SpawnableType spawnableType, bool isFromRoundScript, PointGroup assignedPath)
    {
        this.BotType = botType;
        this.BotMoveType = botMoveType;
        this.Type = spawnableType;
        this.IsFromRoundScript = isFromRoundScript;
        this.AssignedPath = assignedPath;
        Waypoints = AssignedPath.points;
    }
    
    private void OnEnable()
    {
        OnBotDeathReported = null;

    }

    /// <summary>
    /// Bắn ra sự kiện OnBotDeathReported để BotSpawnManager có thể dọn dẹp.
    /// </summary>
    public void Bot_ReportKill()
    {
        OnBotDeathReported?.Invoke(this);
    }
}