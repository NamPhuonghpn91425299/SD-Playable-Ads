using System.Collections;
using UnityEngine;
using static GameConstants;

public class Default_Start : StateBase
{
    public int _animType { get; set; }
    Coroutine _startCoroutine;
    public override void EnterState()
    {
        botContext.ChangeAnimAndType(HashStart, _animType);
    }


    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        if (_startCoroutine != null)
        {
            StopCoroutine(_startCoroutine);
            _startCoroutine = null;
        }
    }

    public void EndStart()
    {
        _startCoroutine = StartCoroutine(IEStart());
    }
    
    private IEnumerator IEStart()
    {
        yield return new WaitForSeconds(Random.Range(0.1f,.7f));
        botContext.ChangeAnimAndType(HashEndStart, _animType);
    }

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
        botContext.stateController.ChangeState(EnemyState.Move);
    }
}