using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BotPlayItaStateMachine;

public class BotPlayItaMoveState : BaseState<PlayItaState>
{
    [SerializeField] private HumanMoveBase humanMoveBase;
    [SerializeField] private BotNetwork botNet;
    protected WayPoint path;
    protected int moveIndex;
    public int[] moveIndexList;
    private bool isMoveDone;

    public override void EnterState()
    {
        Invoke(nameof(Init), 0.1f);

    }
 
    protected void Init()
    {
        path = botNet.Path;
        isMoveDone = false;
        if (humanMoveBase.isHaveParent)
        {
            moveIndex = moveIndexList[0]; // tức là chỉ điểm đến cuối 
        }else
        {
            moveIndex = moveIndexList[1]; // chạy tiếp tới điểm 
        }

    }
    public override void UpdateState()
    {
        if (path != null)
        {
            if (!humanMoveBase.isHaveParent && moveIndex < path.WayPoints.Count)
            {
                humanMoveBase.SetBotMove(path.WayPoints[moveIndex]);
                float distance = Vector3.Distance(humanMoveBase.myTrans.position, path.WayPoints[moveIndex].position);
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
    public override void ExitState()
    {

    }
    public override PlayItaState GetNextState()
    {
        if(botNet.IsDead)
        {
            return PlayItaState.Dead;
        }
        else
        {
            if (isMoveDone)
            {
                return PlayItaState.Attack;
            }
            else {
                return StateKey;
            }

        }
      
    }
}
