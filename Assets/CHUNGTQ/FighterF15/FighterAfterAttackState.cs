using Luna.Unity.FacebookInstantGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FighterStateMachine;

public class FighterAfterAttackState : BaseState<FighterState>
{
    [SerializeField] BotNetwork botNetwork;
    [SerializeField] Transform rotaBody;
    private float countFrame;
    private float tempMoveSpeed;
    private float tempRotaSpeed;
    private bool isMoveAfterAttack;
    [SerializeField] BotConfigSO fighterConfig;

    public override void EnterState()
    {
        isMoveAfterAttack = false;
        countFrame = 0f;
        tempMoveSpeed = fighterConfig.moveSpeed;
        tempRotaSpeed = TUtiliti.GetRotaSpeed(transform, botNetwork.Path.WayPoints[1].position, tempMoveSpeed, 90);

    }
    public override void UpdateState()
    {
        SetTempRotaSpeed();
        TUtiliti.TiltBodyOnMoveZ(transform, rotaBody, botNetwork.Path.WayPoints[1].position, 1.5f, 40);
        TUtiliti.TiltBodyOnMoveX(rotaBody, tempMoveSpeed, tempMoveSpeed, 1, 1.2f);
        transform.Translate(Vector3.forward * tempMoveSpeed * Time.deltaTime);
        TUtiliti.RotaToTarget(transform, botNetwork.Path.WayPoints[1].position, tempMoveSpeed);
        float distance = Vector3.Distance(transform.position, botNetwork.Path.WayPoints[1].position);
        if (distance < 2)
        {
            isMoveAfterAttack = true;
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
        }
        else
        {

            //if (isMoveAfterAttack)
            //{
            //    return FighterState;
            //}
            return StateKey;
        }

    }
    public virtual void SetTempRotaSpeed(float campAngle = 90, float addSpeed = 10)
    {
        countFrame++;
        if (countFrame % 10 == 0)
        {
            countFrame = 9;
            float rotaspeedClone = TUtiliti.GetRotaSpeed(transform, botNetwork.Path.WayPoints[1].position, tempMoveSpeed, campAngle) + addSpeed;
            if (rotaspeedClone < 80)
            {
                tempRotaSpeed = rotaspeedClone;
            }
        }
    }
}
