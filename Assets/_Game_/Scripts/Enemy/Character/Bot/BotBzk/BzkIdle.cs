using System.Collections;
using UnityEngine;
using static GameConstants;

public class BzkIdle : StateBase
{
    [SerializeField] private float _timerDelayChangeAttackStateMax = 4f;
    [SerializeField] private float _timerDelayChangeAttackStateMin = 2f;
    public override void EnterState()
    {
        var _timerDelayChangeAttackState = Random.Range(_timerDelayChangeAttackStateMin, _timerDelayChangeAttackStateMax);
        botContext.ChangeAnimAndType(HashIdle);
        StartCoroutine(IEDelayChangeState(_timerDelayChangeAttackState));
    }
    public override void ExitState()
    {
        // Add any cleanup logic here if needed
        StopAllCoroutines();
    }

    public override void UpdateState()
    {
        // Add update logic here if needed
    }

    public void EndStart()
    {
        StopAllCoroutines();
    }

    IEnumerator IEDelayChangeState(float delay)
    {
        yield return HelperCoroutine.GetWait(delay);
        botContext.stateController.ChangeState(EnemyState.Attack);
    }


}