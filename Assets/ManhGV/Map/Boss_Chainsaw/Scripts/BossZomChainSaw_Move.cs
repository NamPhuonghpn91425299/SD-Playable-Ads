using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BossZomChainSaw_Move : BaseState<bossChainSawState>
{
    [SerializeField] private HumanMoveBase humanMoveBase;
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] private BossZomChainSaw thisBoss;
    private WayPoint path;
    private int moveIndex;
    private bool DoneMove;
    
    public override void EnterState()
    {
        Init();
    }

    public void Init()
    {
        path = botNetwork.Path;
        botNetwork.BotConfigSO.moveSpeed = 1.5f;
        DoneMove = false;
        RandomMoveIndex();
    }

    public void RandomMoveIndex()
    {
        int randomIndex = Random.Range(0, path.AttackWayPoints.Count);
        while (randomIndex == moveIndex)
        {
            randomIndex = Random.Range(0, path.AttackWayPoints.Count);
        }
        moveIndex = randomIndex;
        botNetwork.ChangeAnim("Walk");
    }
    
    public override void UpdateState()
    {
        if (path != null)
        {
            if (!humanMoveBase.isHaveParent)
            {
                humanMoveBase.SetBotMove(path.AttackWayPoints[moveIndex]);
            }

            float distance = Vector3.Distance(humanMoveBase.myTrans.position, path.AttackWayPoints[moveIndex].position);
            if (distance < 0.1)
            {
                DoneMove = true;
            }
        }else
            print("Pat null");
    }

    public override void ExitState()
    {
        
    }

    public override bossChainSawState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return bossChainSawState.Dead;
        }
        else
        {
            if (thisBoss.CanHit())
                return bossChainSawState.Hit;
            else if (DoneMove)
            {
                return bossChainSawState.Attack;
            }
            else
            {
                return StateKey;
            }
        }
    }
}