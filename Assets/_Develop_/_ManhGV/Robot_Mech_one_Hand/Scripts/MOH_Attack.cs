using System;
using System.Collections;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using static GameConstants;
using UnityEngine;

public class MOH_Attack : StateBase
{
    [Header("FireBall Attack")] 
    [SerializeField] private Transform pointSpawnBullet;
    [SerializeField] private Transform point;
    [SerializeField] private BulletBezier bulletBezier;
    [SerializeField] private ParticleSystem vfxHoldGun;

    [Header("Machine Gun Attack")] 
    [SerializeField] private ParticleSystem vfxMachineGun;

    public int AnimTypeAttack = 10;

    private void OnEnable()
    {
        bulletBezier.transform.parent = null;
    }

    public override void EnterState()
    {
        botContext.ChangeAnimAndType(HashAttack, AnimTypeAttack);
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        StopAllCoroutines();
        botContext.audioPlayable.StopAllAudioDontEnbleFalse();
        if (AnimTypeAttack == 10)
            vfxHoldGun.Stop();
        else if (AnimTypeAttack == 20)
            vfxMachineGun.Stop();
    }

    public void StopFireBallVFX()
    {
        vfxHoldGun.Stop();
        AnimTypeAttack = 20;
    }

    public override void TriggerCenterAnimation()
    {
        base.TriggerCenterAnimation();
        StartCoroutine(IEAttack());
    }

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
        botContext.stateController.ChangeState(EnemyState.Move);
    }

    private IEnumerator IEAttack()
    {
        if (AnimTypeAttack == 10)
        {
            vfxHoldGun.Play();
            botContext.ChangeAnimAndType(HashAttack, AnimTypeAttack + 1);
            botContext.audioPlayable.PlayAudioIndexLoop(GameConstants.AudioType.BotAttack,1,false);
            bulletBezier.transform.position = point.position;
            yield return new WaitForSeconds(2.6f);
            vfxHoldGun.Stop();
            botContext.ChangeAnimAndType(HashAttack, AnimTypeAttack + 2);
            yield return new WaitForSeconds(.15f);
            bulletBezier.Init(point.position, PlayerInstant.Instance.explosionPos.position, 1f, 10);
            bulletBezier.gameObject.SetActive(true);
        }
        else if (AnimTypeAttack == 20)
        {
            vfxMachineGun.Play();
            botContext.ChangeAnimAndType(HashAttack, AnimTypeAttack + 1);
            if (GameController.Instance.CurrentGameState == GameState.InGame)
            {
                            botContext.audioPlayable.PlayAudioIndexLoop(GameConstants.AudioType.BotAttack,0);
                float maxDuration = 3f;
                float timerTakeDamage = .5f;
                Vector3 posPlayer = PlayerInstant.Instance.TF.position;
                while (maxDuration > 0) 
                {
                    maxDuration -= Time.deltaTime;
                    timerTakeDamage += Time.deltaTime;
                    if (timerTakeDamage >= .2f)
                    {
                        timerTakeDamage = 0;
                        EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: botContext.botNetwork.Damage, state:"OnlyDamage"));
                        BulletTrail bullet = SimplePool<GameConstants.ProjecttilePlayer>.Spawn<BulletTrail>(ProjecttilePlayer.Projectile_Bullet_Norman, pointSpawnBullet.position, Quaternion.identity);
                        bullet.Init((posPlayer - pointSpawnBullet.position).normalized, posPlayer);
                    }
                    yield return null;
                }
                botContext.audioPlayable.StopAllAudioDontEnbleFalse();
            }
            else
            {
                yield return new WaitForSeconds(3.5f);
            }
            vfxMachineGun.Stop();
            botContext.ChangeAnimAndType(HashAttack, AnimTypeAttack + 2);
        }
    }
}