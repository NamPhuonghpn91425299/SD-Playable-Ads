using UnityEngine;

public class ThuyenCano_StateController : StateControllerBase
{
    [Header("State")]
    public ThuyenHiggins_Move moveState;
    public ThuyenHiggins_Dead deadState;
    public ThuyenCano_DropTroops dropTroops;
    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (moveState == null)
            moveState = GetComponent<ThuyenHiggins_Move>();
        if (deadState == null)
            deadState = GetComponent<ThuyenHiggins_Dead>();
        if (dropTroops == null)
            dropTroops = GetComponent<ThuyenCano_DropTroops>();
    }
#endif
    
    private void Awake()
    {
        moveState.Initialize(GameConstants.EnemyState.Move, botContext);
        deadState.Initialize(GameConstants.EnemyState.Dead, botContext);
        dropTroops.Initialize(GameConstants.EnemyState.DropTroops, botContext);
        
        stateController.Add(GameConstants.EnemyState.Move, moveState);
        stateController.Add(GameConstants.EnemyState.Dead, deadState);
        stateController.Add(GameConstants.EnemyState.DropTroops, dropTroops);
    }

    public override void OnInit(GameConstants.EnemyState _EnterState)
    {
        moveState.OnInitState();
        deadState.OnInitState();
        base.OnInit(_EnterState);
        dropTroops.InitSpawnBot();
    }

    protected override void OnDead(bool isDead)
    {
        base.OnDead(isDead);
        if(_currentState.StateKey!=GameConstants.EnemyState.DropTroops)
            dropTroops.CallbotEqualsNull_IfCanoDead();//set parent to null for all troops spawned
    }
}