using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FighterStateMachine;

public class FighterMoveTargetState : BaseState<FighterState>
{
    [SerializeField] BotNetwork botNetwork;
    [SerializeField] Transform rotaBody;
    [SerializeField] BotConfigSO fighterConfig;
    private float countFrame;
    private float tempMoveSpeed;
    private float tempRotaSpeed;
    private bool isMoveToTargetPos;
    private Vector3 destination;
    public void SetDestination(Vector3 destination)
    {
        this.destination = destination;
    }
    public override void EnterState()
    {
        isMoveToTargetPos = false;
        countFrame = 0f;
        tempMoveSpeed = fighterConfig.moveSpeed;
        tempRotaSpeed = TUtiliti.GetRotaSpeed(transform, destination, tempMoveSpeed, 90);
    }
    public override void UpdateState()
    {
        TUtiliti.TiltBodyOnMoveZ(transform, rotaBody, destination, 1.5f, 40);
        transform.Translate(Vector3.forward * tempMoveSpeed * Time.deltaTime);
        TUtiliti.RotaToTarget(transform, destination, tempMoveSpeed);
        float distance = Vector3.Distance(transform.position, destination);
        if (distance < 2)
        {
            isMoveToTargetPos = true;
        }
    }
    public override void ExitState()
    {
        
    }
    public override FighterState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return FighterState.Dead;
        }else
        {
            if (isMoveToTargetPos)
            {
                //return FighterState.BombAttackPos;
            }
            return StateKey;
        }
        

    }
    public virtual void SetTempRotaSpeed(float campAngle = 90, float addSpeed = 10)
    {
        countFrame++;
        if (countFrame % 10 == 0)
        {
            countFrame = 9;
            float rotaspeedClone = TUtiliti.GetRotaSpeed(transform, destination, tempMoveSpeed, campAngle) + addSpeed;
            if (rotaspeedClone < 80)
            {
                tempRotaSpeed = rotaspeedClone;
            }
        }
    }
   
}

