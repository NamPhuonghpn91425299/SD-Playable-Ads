using Luna.Unity.FacebookInstantGames;
using System.IO;
using UnityEngine;
using static FighterStateMachine;

public class FighterBehindState : BaseState<FighterState>
{
    [SerializeField] BotNetwork botNetwork;
    [SerializeField] Transform rotaBody;
    [SerializeField] FighterMoveTargetState targetPos;
    [SerializeField] BotConfigSO fighterConfig;
    private float countFrame;
    private float tempMoveSpeed;
    private float tempRotaSpeed;
    private bool isMoveToDirPos;
    private Vector3 destination;
    public override void EnterState()
    {
        isMoveToDirPos = false;
        countFrame = 0f;
        tempMoveSpeed = fighterConfig.moveSpeed;
        int RandomL_R = Random.Range(0,100);

        destination = RandomL_R%2 ==0 ?botNetwork.Path.WayPoints[2].position: botNetwork.Path.WayPoints[3].position;
        Vector3 targetPosDes = RandomL_R % 2 == 0 ? botNetwork.Path.WayPoints[5].position : botNetwork.Path.WayPoints[6].position;
        targetPos.SetDestination(targetPosDes);
        tempRotaSpeed = TUtiliti.GetRotaSpeed(transform, destination, tempMoveSpeed,90);
    }
    public override void UpdateState()
    {
        SetTempRotaSpeed(120, -20);
        TUtiliti.TiltBodyOnMoveZ(transform, rotaBody, destination, 1.5f, 40);
        transform.Translate(Vector3.forward * tempMoveSpeed * Time.deltaTime);
        TUtiliti.RotaToTarget(transform, destination, tempMoveSpeed);
        float distance = Vector3.Distance(transform.position, destination);
        if (distance < 2)
        {
            isMoveToDirPos = true;
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
            if (isMoveToDirPos)
            {
                //return FighterState.TargetPos;
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
