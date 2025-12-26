using System.Collections;
using static GameConstants;
using UnityEngine;

public class TankStugIV_Attack : StateBase
{
    [SerializeField] private TankStugIV_Move tankStugIVMove;
    [SerializeField] private float speedRotateToPlayer = 45f;
    
    [Header("Attack Settings")]
    [SerializeField] private Transform muzzle;
    [SerializeField] protected ProjectileEnemy _bulletType;  // Loại đạn sẽ được bắn
    [SerializeField] private ParticleSystem vfxAttack;
    private Coroutine coroutineAttack;
    private Vector3 targetPos;
    public override void EnterState()
    {
        coroutineAttack = StartCoroutine(IEAttack());
    }

    private IEnumerator IEAttack()
    {
        targetPos = PlayerInstant.Instance.TF.position;
        Vector3 directionToTarget = targetPos - TF.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        while (Quaternion.Angle(TF.rotation, targetRotation) > .5f)
        {
            tankStugIVMove.RotationWheel();
            TF.rotation = Quaternion.RotateTowards(TF.rotation, targetRotation, speedRotateToPlayer * Time.deltaTime);
            yield return null;
        }
        
        yield return HelperCoroutine.GetWait(1f);
        botContext.ChangeAnimAndType(HashAttack);
        vfxAttack.Play();
        
        Rocket bullet = SimplePool<ProjectileEnemy>.Spawn<Rocket>(_bulletType, muzzle.position, muzzle.rotation);
        bullet.Init(botContext.botNetwork.Damage);
        
        yield return HelperCoroutine.GetWait(1f);
        coroutineAttack = null;
        botContext.stateController.ChangeState(EnemyState.Move);
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        if (coroutineAttack != null) 
            StopCoroutine(coroutineAttack);
    }
}