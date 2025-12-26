using System;
using static GameConstants;
using UnityEngine;

public class ThuyenHiggins_Move : StateBase
{
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của bot.")]
    [SerializeField] private float moveSpeed = 5.0f;
    
    [Header("Thuyen_Cano")]
    [SerializeField] bool isCano; // Kiểm tra xem đây có phải là Cano hay không
    
    [Header("Thuyen_Higgins")]
    [SerializeField] private int pointDropVehicle; // Điểm thả xe
    [SerializeField] private GameObject vfxMoveStart;
    [SerializeField] private GameObject vfxMoveEnd;
    
    [SerializeField] private BotIdentity botIdentity; // Tham chiếu đến BotIdentity để lấy thông tin về đường đi
    [SerializeField] private int currentPointIndex = 0; // Điểm tiếp theo cần đến
    private bool IsMoveStart = false;
    private Vector3 pointMove;

    public void OnInitState()
    {
        currentPointIndex = 0;
        IsMoveStart = false;
    }
    
    public override void EnterState()
    {
        IsMoveStart = !IsMoveStart;
        if (!isCano)
        {
            vfxMoveStart.SetActive(IsMoveStart);
            vfxMoveEnd.SetActive(!IsMoveStart);
        }
        pointMove = botIdentity.AssignedPath.points[currentPointIndex].position;
        // if (IsMoveStart)
        //     TF.LookAt(pointMove);
    }

    public override void UpdateState()
    {
        if (botIdentity.AssignedPath == null || currentPointIndex >= botIdentity.AssignedPath.points.Count)
        {
            Debug.LogError($"Bot '{gameObject.name}' không có tuyến đường để di chuyển.");
            return;
        }

        if (Vector3.Distance(TF.position, pointMove) < 0.1f)
        {
            currentPointIndex++;
            if (currentPointIndex < botIdentity.AssignedPath.points.Count)
                pointMove = botIdentity.AssignedPath.points[currentPointIndex].position;
            // if (currentPointIndex < assignedPath.points.Count)
            // {
            //     if(IsMoveStart)
            //         TF.LookAt(pointMove);
            // }
            if (isCano)
            {
                if (currentPointIndex >= botIdentity.AssignedPath.points.Count)
                {
                    botContext.stateController.ChangeState(EnemyState.DropTroops);
                    return;
                }
            }
            else
            {
                if (currentPointIndex >= botIdentity.AssignedPath.points.Count)
                {
                    botContext.botNetwork.OnDespawn(0f);
                    return;
                }
                else if (currentPointIndex == pointDropVehicle + 1)
                {
                    botContext.stateController.ChangeState(EnemyState.DropTroops);
                    return;
                }
            }
        }
        TF.position = Vector3.MoveTowards(TF.position,pointMove, moveSpeed * Time.deltaTime);//botContext.botNetwork.Getspeed);
    }

    public override void ExitState()
    {
        
    }
}