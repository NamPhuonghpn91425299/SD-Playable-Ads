using static BotTankStateMachine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotTankMoveState : BaseState<TankState>
{
    [SerializeField] private TankBaseMovement tankMovement;
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] protected WayPoint wayPoint;
    public bool isChangeAttackpoint = false;
    public int pointIndex = 0;

    public override void EnterState()
    {
        Init();
    }

    private void Init()
    {
        wayPoint = botNetwork.Path;
        isChangeAttackpoint = false;

    }
    private void MoveToNextPoint()
    {
            // Kiểm tra null và số lượng phần tử trước khi tiếp tục
            if (wayPoint == null || wayPoint.WayPoints == null || wayPoint.WayPoints.Count == 0) return;
            // Kiểm tra để tránh vượt quá index
            if (pointIndex > wayPoint.WayPoints.Count -1)
            {
                    isChangeAttackpoint = true;
                    return;
            }

            tankMovement.SetBotTankMove(wayPoint.WayPoints[pointIndex]);
            float distance = Vector3.Distance(tankMovement.myTrans.position, botNetwork.Path.WayPoints[pointIndex].position);
            // Cập nhật hướng đi cho lần tiếp theo
            if (distance < 0.1f)
            {
                //IsCloseToNextPoint(pointIndex);
                //IsCloseToPreviousPoint(pointIndex);
                pointIndex++;
            }

    }


    public override void UpdateState()
    {
        // Chỉ gọi MoveToNextPoint khi không trong trạng thái tấn công
        if (!isChangeAttackpoint)
        {
            MoveToNextPoint();

        }
    }
    

    public override TankState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return TankState.Dead;
        }
        else if (isChangeAttackpoint)
        {
            return TankState.MoveToAttack;
        }
        return StateKey;
    }

    //// Hàm kiểm tra khoảng cách đến điểm tiếp theo
    //public bool IsCloseToNextPoint(int currentIndex)
    //{
    //    if (currentIndex >= wayPoint.WayPoints.Count - 1)
    //        return false; // Không có điểm tiếp theo

    //    int nextIndex = currentIndex + 1;
    //    float distance = Vector3.Distance(tankMovement.myTrans.position, wayPoint.WayPoints[nextIndex].position);
    //    Debug.LogError("điểm tiếp theo " + IsCloseToNextPoint(nextIndex) + " " + distance);
    //    return distance <= checkDistanceThreshold;
    //}
    //// Hàm kiểm tra khoảng cách đến điểm trước đó
    //public bool IsCloseToPreviousPoint(int currentIndex)
    //{
    //    if (currentIndex <= 0)
    //        return false; // Không có điểm trước đó

    //    int previousIndex = currentIndex - 1;
    //    float distance = Vector3.Distance(tankMovement.myTrans.position, wayPoint.WayPoints[previousIndex].position);
    //    Debug.LogError("điểm trước đó " + IsCloseToPreviousPoint(previousIndex) + " " + distance);
    //    return distance <= checkDistanceThreshold;
    //}

    public override void ExitState()
    {
        //pointAttackIndex = Random.Range(0,pointAttackIndex);
        audioSource.Stop();
        isChangeAttackpoint = false;
    }
}
