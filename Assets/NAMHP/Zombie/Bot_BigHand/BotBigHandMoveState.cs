using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotBigHandMoveState : BaseState<BigHandState>
{
    [SerializeField] private HumanMoveBase humanMoveBase;
    [SerializeField] private BotNetwork botNet;
    [SerializeField] private Animator anim;
    [SerializeField] private bool isMoveStyle;
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
        if(anim!=null)
            anim.SetBool("DoneStart",true);
        
        path = botNet.Path;
        isMoveDone = false;
        if (humanMoveBase.isHaveParent)
        {
            moveIndex = moveIndexList[0]; // tức là chỉ điểm đến cuối 
        }else
        {
            moveIndex = moveIndexList[1]; // chạy tiếp tới điểm 
        }
        SetMoveStyle(isMoveStyle);
    }

    private void SetMoveStyle(bool isMoveStyle)
    {
        if(!isMoveStyle)
            return;
        int randomStyle = Random.Range(0, 100);
        
        if (randomStyle % 2 == 0)
        {
            anim.SetFloat("MoveStyle", 1);
            anim.SetFloat("MoveSpeedScale", 1f);
            Debug.Log("SetMoveStyle: 1 : Random = " + randomStyle  + " chia 2 dư: " + randomStyle % 2);
        }
        else
        {
            anim.SetFloat("MoveStyle", 0);
            anim.SetFloat("MoveSpeedScale", 1.5f);
            Debug.Log("SetMoveStyle: 0 : Random = " + randomStyle + " chia 2 dư: " + randomStyle % 2);
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
    public override BigHandState GetNextState()
    {
        if (botNet.DeadExplosion)
            return BigHandState.DeadExplosion;
        else
        {
            if(botNet.IsDead)
            {
                return BigHandState.Dead;
            }
            else
            {
                if (isMoveDone)
                {
                    return BigHandState.Attack;
                }
                else {
                    return StateKey;
                }

            }
        }
      
    }
}
