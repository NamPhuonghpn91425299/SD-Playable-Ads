using UnityEngine;
using static GameConstants;
/// <summary>
/// Gắn vào các GameObject bot đã được đặt sẵn trong Scene Editor.
/// Giờ đây nó cũng chứa thông tin cấu hình ban đầu cho con bot này.
/// </summary>
[RequireComponent(typeof(BotIdentity))]
public class PreSpawnedBot : MonoBehaviour
{
    // GIẢI THÍCH: Thêm các trường này để Game Designer có thể cấu hình trực tiếp trong Inspector.
    [Header("Pre-Spawn Configuration")]
    [Tooltip("Loại của con bot này.")]
    [SerializeField] private BotType botType;
    [Tooltip("Loại đường đi mà con bot này sẽ sử dụng.")]
    [SerializeField] private BotMoveType botMoveType = BotMoveType.Infantry;
    [Tooltip("Loại đối tượng mà Definition này đại diện, chỉ tính botkill trên type Bot.")]
    [SerializeField] private SpawnableType killType = SpawnableType.Bot;
    // Cung cấp các thuộc tính public để các hệ thống khác (như GameManager) có thể đọc.
    public BotType BotType => botType;
    public BotMoveType BotMoveType => botMoveType;

    public SpawnableType Type => killType;
    private void OnEnable() => SpawnableRegistry.Register(this);
    private void OnDisable() => SpawnableRegistry.Unregister(this);
}