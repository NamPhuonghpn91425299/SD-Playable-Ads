using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

public class ThuyenPR112_Move : StateBase
{
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của bot.")]
    [SerializeField] private float moveSpeed = 5.0f;



    [Header("Thuyen_Higgins")]
    [SerializeField] private GameObject vfxMoveStart;

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
        vfxMoveStart.SetActive(IsMoveStart);
        pointMove = botIdentity.AssignedPath.points[currentPointIndex].position;
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


            if (currentPointIndex >= botIdentity.AssignedPath.points.Count)
            {
                botContext.stateController.ChangeState(EnemyState.Attack);
                return;
            }


        }
        TF.position = Vector3.MoveTowards(TF.position, pointMove, moveSpeed * Time.deltaTime);//botContext.botNetwork.Getspeed);
    }

    public override void ExitState()
    {

    }
}
