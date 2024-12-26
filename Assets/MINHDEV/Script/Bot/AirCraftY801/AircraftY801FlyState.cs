using System.Collections;
using UnityEngine;
using static AircraftY801StateMachine;

public class AircraftY801FlyState : BaseState<AirForceState>
{
    [SerializeField] private BotConfigSO AircraftConfig;
    [SerializeField] private BotNetwork botNetwork;
    [Tooltip("Khoảng cách random điểm thả lính, tính từ điểm HidePos")]
    [SerializeField] private float Min_Pos;
    [SerializeField] private float Max_Pos;
    private float distanceSpwan;
    private WayPoint _path;
    private float _speed;
    private bool isFlyDone;

    public override void EnterState()
    {
        isFlyDone = false;
        Invoke(nameof(Init), 0.1f);
    }

    private void Init()
    {
        _path = botNetwork.Path;
        _speed = AircraftConfig.moveSpeed;
        distanceSpwan = Random.Range(Min_Pos, Max_Pos);
    }

    public override void UpdateState()
    {
        Fly();
    }

    private void Fly()
    {
        if (_path != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, _path.WayPoints[1].position, _speed * Time.deltaTime);
            float distance = Vector3.Distance(transform.position, _path.WayPoints[1].position);

            if (distance < distanceSpwan)
            {
                isFlyDone = true;
            }
        }
    }

    public override void ExitState()
    {
        isFlyDone = false; // Reset lại isFlyDone khi rời khỏi trạng thái
    }

    public override AirForceState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return AirForceState.Dead;
        }
        else
        {
            if (isFlyDone)
            {
                return AirForceState.SpawnBot;
            }
            else
            {
                return StateKey;
            }
        }
    }
}
