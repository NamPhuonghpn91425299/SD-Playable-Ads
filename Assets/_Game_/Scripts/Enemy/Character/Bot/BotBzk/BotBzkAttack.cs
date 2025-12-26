using System.Collections;
using static GameConstants;
using UnityEngine;
using Random = UnityEngine.Random;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using System;

public class BotBzkAttack : StateBase
{
    [SerializeField] private GameObject FireVfx;
    [SerializeField] private Transform positionSpawnRocket;
    [SerializeField] protected GameConstants.ProjectileEnemy _bulletType;  // Loại đạn sẽ được bắn

    public override void EnterState()
    {
        botContext.botNetwork.RotateToPlayer();
        botContext.ChangeAnimAndType(HashAttack);
        StartCoroutine(IEChangeReloadState());

    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        StopAllCoroutines();
        FireVfx.SetActive(false);
    }

    private IEnumerator IEChangeReloadState()
    {
        yield return HelperCoroutine.GetWait(botContext.animator.GetCurrentAnimatorStateInfo(0).length);
        SpawnRocket();
        botContext.stateController.ChangeState(EnemyState.Reload);
    }

    public void SpawnRocket()
    {
        Rocket bullet = SimplePool<GameConstants.ProjectileEnemy>.Spawn<Rocket>(_bulletType, positionSpawnRocket.position, positionSpawnRocket.rotation);
        bullet.Init(botContext.botNetwork.Damage);
        FireVfx.SetActive(true);
    }
}