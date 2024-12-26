using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Y8_AirDefenseStateMachine;
public class Y8_AirDefenseMoveState : BaseState<Y8_AirDefense>
{
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] private BotConfigSO botConfigSO;
    [SerializeField] private F15TrackingMovement f15TrackingMovement;
    private Transform _transform;
    private bool isMoveDone;
    [Tooltip("Chỉnh giảm tốc độ khi tới gần điểm đặt ở biến này nhé !!!")]
    [SerializeField] private float slowDownFactor = 2f; // Giảm tốc dựa trên khoảng cách
    public override void EnterState()
    {
        _transform = transform;
        f15TrackingMovement.enabled = false;
        isMoveDone = false;
    }
    private void Y8MoveState(Transform point)
    {
        if (!botNetwork.IsDead)
        {
            var targetRotation = Quaternion.LookRotation(point.position - _transform.position);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, botConfigSO.moveSpeed * Time.deltaTime);
            float distance = Vector3.Distance(_transform.position, point.position);
            // Giảm tốc độ nếu gần đến đích
            float adjustedSpeed = Mathf.Lerp(0f, botConfigSO.moveSpeed, distance/slowDownFactor);
            _transform.position = Vector3.MoveTowards(_transform.position, point.position, adjustedSpeed * Time.deltaTime);
        }
    }

    public override void UpdateState()
    {
        Y8MoveState(botNetwork.Path.WayPoints[1]);
        float distance = Vector3.Distance(transform.position, botNetwork.Path.WayPoints[1].position);
        if (distance < 1f)
        {
            f15TrackingMovement.enabled = true;
            isMoveDone = true;
        }
    }
    public override void ExitState()
    {
        
    }

    public override Y8_AirDefense GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return Y8_AirDefense.Dead;
        }
        else
        {
            if (isMoveDone)
            {
                return Y8_AirDefense.Idle;
            }
            return StateKey;
        }
    }

}
