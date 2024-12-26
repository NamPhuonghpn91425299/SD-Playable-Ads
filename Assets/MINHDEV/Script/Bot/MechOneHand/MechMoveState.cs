using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BotPlayItaStateMachine;
using static MechStateMachine;

public class MechMoveState : BaseState<MechState>
{
    public MechMoveBase mechMoveBase;
    public BotNetwork botNet;
    protected WayPoint path;
    public bool isMoveDone;
    protected int moveIndex;

    public override void EnterState()
    {
        Invoke(nameof(Init), 0.1f);
    }

    protected void Init()
    {
        path = botNet.Path;
        isMoveDone = false;
        moveIndex = 1; // chạy tiếp tới điểm 

    }

    public override void ExitState()
    {
       
    }

    public override MechState GetNextState()
    {
        if (botNet.IsDead)
        {
            return MechState.Dead;
        }
        else
        {
            if (isMoveDone)
            {
                return MechState.Attack;
            }
            else
            {
                return StateKey;
            }

        }
    }

    public override void UpdateState()
    {
        if (path != null)
        {
            if (moveIndex < path.WayPoints.Count)
            {
                mechMoveBase.SetBotMove(path.WayPoints[moveIndex]);
                float distance = Vector3.Distance(mechMoveBase.myTrans.position, path.WayPoints[moveIndex].position);
                if (distance < 0.1)
                {
                    moveIndex++;
                }
            }
            if (moveIndex == path.WayPoints.Count)
            {
                isMoveDone = true;
            }

        }
    }

}
