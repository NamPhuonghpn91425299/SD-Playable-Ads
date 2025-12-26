using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;
using static GameConstants;
public class Supersoldat_Attack_Rocket_State : StateBase
{

    [SerializeField] private ProjectileEnemy _bulletType;
    [SerializeField] private Transform[]     spawnPoints;
    [SerializeField] private int             rocketCount   = 3;
    [SerializeField] private float           spawnInterval = 0.2f;
    [SerializeField] private float           _speed        = 3f;
    [SerializeField] private float           _curveHeight   = 20f;
    
    public override void EnterState()
    {
        StartCoroutine(SpawnRocket());
    }

    public override void ExitState()
    {
        StopAllCoroutines();
    }

    public override void UpdateState()
    {

    }

    private IEnumerator SpawnRocket()
    {
            botContext.ChangeAnimAndType(HashAttack,5);
            yield return HelperCoroutine.GetWait(2f);

            int count = Mathf.Max(0, rocketCount);
            botContext.ChangeAnimAndType(HashAttack,7);
            for (int i = 0; i < count; i++)
            {
                int idx = Random.Range(0, spawnPoints.Length);
                SpawnAt(spawnPoints[idx]);
                if (spawnInterval > 0f && i < count - 1)
                    yield return HelperCoroutine.GetWait(spawnInterval);
            }
            yield return HelperCoroutine.GetWait(1f);
            botContext.ChangeAnimAndType(HashAttack,6);
            yield return HelperCoroutine.GetWait(2f);
            botContext.stateController.ChangeState(EnemyState.Attack);
    }

    private void SpawnAt(Transform point)
    {
        if (point == null) return;
        var bullet = SimplePool<ProjectileEnemy>.Spawn<BulletBezier>(_bulletType, point.position, point.rotation);
        if (bullet != null)
            bullet.Init(point.position, PlayerInstant.Instance.explosionPos.position, _speed, _curveHeight);
    }
}
