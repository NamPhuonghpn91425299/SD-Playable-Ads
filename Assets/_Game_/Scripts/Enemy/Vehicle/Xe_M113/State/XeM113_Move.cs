using static GameConstants;
using UnityEngine;

public class XeM113_Move : StateBase
{
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của bot.")]
    [SerializeField] private float moveSpeed = 5.0f;

    [Header("Pathing Info (Read-Only)")]
    [Tooltip("Tuyến đường mà bot này đang đi theo.")]
    [SerializeField] private int pointDropVehicle; // Để debug trong Inspector
    [SerializeField] private PointGroup assignedPath; // Để debug trong Inspector
    [SerializeField] private BotIdentity botIdentity; // Tham chiếu đến BotIdentity để lấy thông tin về đường đi
    private int currentPointIndex = 0; // Điểm tiếp theo cần đến
    private Vector3 pointMove;
    private void Start()
    {
        // if(assignedPath == null)
        //     assignedPath = botIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
    }
    
    private void OnEnable()
    {
        assignedPath = botIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
        currentPointIndex = 0;
    }

    public override void EnterState()
    {
        //assignedPath = botIdentity.AssignedPath;
        Invoke(nameof(Init),.1f);
    }

    void Init()
    {
        if(assignedPath == null)
            assignedPath = botIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
        pointMove = assignedPath.points[currentPointIndex].position;
        TF.LookAt(pointMove);
    }
    
    public override void UpdateState()
    {
        if (assignedPath == null || currentPointIndex >= assignedPath.points.Count)
        {
            Debug.LogError($"Bot '{gameObject.name}' không có tuyến đường để di chuyển.");
            return;
        }

        if (Vector3.Distance(TF.position, pointMove) < 0.1f)
        {
            currentPointIndex++;
            if (currentPointIndex >= assignedPath.points.Count)
            {
                botContext.botNetwork.OnDespawn(0f);
                return;
            }
            else if (currentPointIndex == pointDropVehicle + 1)
            {
                botContext.stateController.ChangeState(EnemyState.DropTroops);
                return;
            }
            pointMove = assignedPath.points[currentPointIndex].position;
            TF.LookAt(pointMove);
        }
        TF.position = Vector3.MoveTowards(TF.position,pointMove, moveSpeed * Time.deltaTime);//botContext.botNetwork.Getspeed);
    }

    public override void ExitState()
    {
        
    }
}