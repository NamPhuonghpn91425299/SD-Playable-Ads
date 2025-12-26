using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XeM113_DropTroops : StateBase
{
    [SerializeField] private Transform pointDrop;
    [SerializeField] private BotType[] typeCharacterDrop;
    [SerializeField] int countDrop = 5;
    [SerializeField] float countDownDrop = 1f;
    private Coroutine CTAttack;
    public override void EnterState()
    {
        CTAttack = StartCoroutine(IEAttack());
    }

    private IEnumerator IEAttack()
    {
        yield return HelperCoroutine.GetWait(2f);//time mở cửa sau
        for (int i = 0; i < countDrop; i++)
        {
            EnemyBase enemyBaseNew = SimplePool<BotType>.Spawn<EnemyBase>(typeCharacterDrop[Random.Range(0, typeCharacterDrop.Length)], pointDrop.position, pointDrop.rotation);
            enemyBaseNew.OnInit();
            enemyBaseNew.stateController.SetupStartState(0);
            enemyBaseNew.stateController.OnInit(GameConstants.EnemyState.Start);
            enemyBaseNew.stateController.CallEndStart();
            yield return HelperCoroutine.GetWait(countDownDrop);
        }
        botContext.stateController.ChangeState(GameConstants.EnemyState.Move);
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        if (CTAttack != null)
            StopCoroutine(CTAttack);
    }
}