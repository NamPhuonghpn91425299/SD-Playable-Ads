using UnityEngine;
using static GameConstants;
[CreateAssetMenu(fileName = "BotConfig", menuName = "Spawning/System/1. BotDefinition")]
public class BotDefinition : ScriptableObject
{
    [SerializeField] private GameObject prefab;
    [Tooltip("Loại Bot mà Definition này đại diện. Đây là khóa chính.")]
    [SerializeField] private BotType botType;
    
    [Tooltip("Loại đường đi mà bot này sẽ luôn sử dụng khi được spawn.")]
    [SerializeField] private BotMoveType botMoveType = BotMoveType.Infantry;
    
    [Tooltip("Loại đối tượng mà Definition này đại diện, chỉ tính botkill trên type Bot.")]
    [SerializeField] private SpawnableType spawnableType = SpawnableType.Bot;
    
    [Tooltip("Đánh dấu nếu Prefab này là một GameObject rỗng chứa nhiều bot con (một 'nhóm'). Nếu không, nó là một bot đơn lẻ.")]
    
    [SerializeField] private bool isGroupPrefab = false;

    public BotType BotType => botType;
    public GameObject Prefab => prefab;
    public bool IsGroupPrefab => isGroupPrefab;
    public SpawnableType Type => spawnableType; 
    public BotMoveType BotMoveType => botMoveType;
}