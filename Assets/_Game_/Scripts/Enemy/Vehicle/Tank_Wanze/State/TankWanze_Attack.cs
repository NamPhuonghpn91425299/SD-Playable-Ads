using System.Collections;
using static GameConstants;
using UnityEngine;

public class TankWanze_Attack : StateBase
{
    [SerializeField] private Transform RocketLanch;
    [SerializeField] private Transform[] posSpawn;
    [SerializeField] private ParticleSystem[] vfxToPlay;
    private Coroutine IERotationToPlayer;
    public override void EnterState()
    {
        IERotationToPlayer = StartCoroutine(IErotationToPlayer());
    }

    private IEnumerator IErotationToPlayer()
    {
        Vector3 playerPos = PlayerInstant.Instance.TF.position;
        while (true)
        {
            Vector3 flatDirection = playerPos - TF.position;
            flatDirection.y = 0;
            
            if (flatDirection.sqrMagnitude < 0.01f)
                break;

            Quaternion targetRot = Quaternion.LookRotation(flatDirection);
            TF.rotation = Quaternion.RotateTowards(TF.rotation, targetRot, 30 * Time.deltaTime);

            if (Quaternion.Angle(TF.rotation, targetRot) < 1f)
                break;

            yield return null;
        }
        
        Quaternion startRotation = RocketLanch.localRotation;
        Vector3 startEuler = startRotation.eulerAngles;
        Vector3 targetEuler = new Vector3(-17.5f, startEuler.y, startEuler.z);
        Quaternion targetRotation = Quaternion.Euler(targetEuler);
        float elapsed = 0f;

        while (elapsed < 1)
        {
            float t = elapsed / 1;
            RocketLanch.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
        RocketLanch.localRotation = targetRotation;

        for (int i = 0; i < posSpawn.Length; i++)
        {
            vfxToPlay[i].Play();
            botContext.ChangeAnimAndType(HashAttack);
            SimplePool<ProjectileEnemy>.Spawn<RocketTankWanze>(ProjectileEnemy.RocketTankWanze,posSpawn[i].position,posSpawn[i].rotation).OnInit(GameController.Instance.GetPosLocalPlayer());
            yield return new WaitForSeconds(1f);
        }
        
        startRotation = RocketLanch.localRotation;
        startEuler = startRotation.eulerAngles;
        targetEuler = new Vector3(17.5f, startEuler.y, startEuler.z);
        targetRotation = Quaternion.Euler(targetEuler);
        elapsed = 0f;

        while (elapsed < 1)
        {
            float t = elapsed / 1;
            RocketLanch.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
        RocketLanch.localRotation = targetRotation;
        
        botContext.stateController.ChangeState(EnemyState.Move);
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        if (IERotationToPlayer != null) StopCoroutine(IERotationToPlayer);
    }
}