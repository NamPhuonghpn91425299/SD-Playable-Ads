using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Asset ScriptableObject chứa toàn bộ kịch bản cho một Round hoàn chỉnh.
/// Đây là file chính mà Game Designer sẽ tương tác.
/// </summary>
[CreateAssetMenu(fileName = "Round_SO_", menuName = "Spawning/Gameplay/1. Round Kịch Bản")]
public class RoundSO : ScriptableObject
{
    [Header("Round Configuration")]
    public string RoundName = "Round_1";
    [Tooltip("Thời gian chờ (giây) sau khi Round này hoàn thành trước khi round tiếp theo bắt đầu.")]
    public float DelayAfterComplete = 1.0f;

    [Header("Kịch Bản Spawn")]
    [Tooltip("Danh sách các bước spawn sẽ diễn ra trong Round này.")]
    public List<BotWave> SpawnSteps;
    
    /// <summary>
    /// Tính toán và trả về tổng số lượng bot sẽ được spawn theo kịch bản gốc của round này.
    /// </summary>
    public int TotalBotCount
    {
        get
        {
            if (SpawnSteps == null) return 0;
            // Dùng LINQ để tính tổng của trường "Quantity" từ mỗi "SpawnStep" trong danh sách.
            return SpawnSteps.Sum(step => step.Quantity);
        }
    }
}