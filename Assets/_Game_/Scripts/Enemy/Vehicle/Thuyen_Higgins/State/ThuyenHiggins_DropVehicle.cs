using System.Collections;
using System.Collections.Generic;
using static GameConstants;
using UnityEngine;

public class ThuyenHiggins_DropVehicle : StateBase
{
    [SerializeField] private BotDefinition botToDrop;
    [SerializeField] private Transform pointDrop;
    [SerializeField] private Animator moveVehicleAnimator;
    private EnemyBase enemyBaseNew;
    Coroutine IEDropVehicleCoroutine;
    private XeToyotahilux_Move toyotahiluxMove;
    private bool rotation = false;

    public void SpawnVehical()
    {
        List<PointGroup> pointChindCanMove = botContext.botIdentity.AssignedPath.PointChindCanMove;
        PointGroup pointGroupFind = pointChindCanMove.Find(x => !x.isBeingUsed) ??
                                pointChindCanMove[Random.Range(0, pointChindCanMove.Count)];
        pointGroupFind.isBeingUsed = true;
        enemyBaseNew = BotSpawnManager.Instance.ExecuteSpawnOrder(botToDrop, pointDrop,pointGroupFind, true);
        enemyBaseNew.OnInit();
        toyotahiluxMove = enemyBaseNew.GetComponent<XeToyotahilux_Move>();
        enemyBaseNew.SetIsImmortal(true);
        if(toyotahiluxMove != null) toyotahiluxMove.ResetMovementState();
    }
    
    public override void EnterState()
    {
        IEDropVehicleCoroutine = StartCoroutine(IEDropVehicle());
    }

    public override void UpdateState()
    {
        if(rotation)
            toyotahiluxMove.RotateBanhXe();
    }

    public override void ExitState()
    {
        if (IEDropVehicleCoroutine != null)
            StopCoroutine(IEDropVehicleCoroutine);
    }

    private IEnumerator IEDropVehicle()
    {
        // EnemyBase enemyBaseNew = SimplePool<BotType>.Spawn<EnemyBase>(botToDrop, pointDrop.position, pointDrop.rotation, pointDrop);
        // enemyBaseNew.stateController.OnInit(EnemyState.Idle);
        yield return HelperCoroutine.GetWait(.2f);
        botContext.ChangeAnimAndType(HashOpenDoor);
        yield return HelperCoroutine.GetWait(2.2f);
        
        moveVehicleAnimator.enabled = true;
        if (toyotahiluxMove != null)
            rotation = true;
        yield return HelperCoroutine.GetWait(4f);
        rotation = false;
        enemyBaseNew.transform.parent = null;
        moveVehicleAnimator.transform.localPosition = new Vector3(5.1f, 1.3f, 0);
        moveVehicleAnimator.enabled = false;
        enemyBaseNew.stateController.OnInit(EnemyState.Move);
        enemyBaseNew.SetIsImmortal(false);
        enemyBaseNew = null;
        
        botContext.ChangeAnimAndType(HashCloseDoor);
        yield return HelperCoroutine.GetWait(2.2f);
        botContext.stateController.ChangeState(EnemyState.Move);
    }

    public void DeadAllEndVehical()
    {
        if (enemyBaseNew != null)
        {
            enemyBaseNew.stateController.OnInit(EnemyState.Dead);
            enemyBaseNew.OnTakeDamage(new DamageInfo{damage = 100, damageType = DamageType.Explosion, posExplosion = Vector3.zero});
        }
    }
}
