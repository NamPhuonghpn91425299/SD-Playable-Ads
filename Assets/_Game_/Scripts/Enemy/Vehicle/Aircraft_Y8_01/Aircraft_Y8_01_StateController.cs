using System;
using static GameConstants;

public class Aircraft_Y8_01_StateController : StateControllerBase
{
    public Aircraft_Y8_01_Move moveState;
    public Vehicle_Dead deadState;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        moveState ??= GetComponent<Aircraft_Y8_01_Move>();
        deadState ??= GetComponent<Vehicle_Dead>();
    }
#endif

    private void Awake()
    {
        moveState.Initialize(EnemyState.Move, botContext);
        deadState.Initialize(EnemyState.Dead, botContext);
        
        stateController.Add(EnemyState.Move, moveState);
        stateController.Add(EnemyState.Dead, deadState);
    }

    private void Start()
    {
        OnInit(EnemyState.Move);
    }
}