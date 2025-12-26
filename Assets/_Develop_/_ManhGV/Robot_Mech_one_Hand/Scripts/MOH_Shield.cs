using System.Collections;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using static GameConstants;
using UnityEngine;

public class MOH_Shield : StateBase
{
    public Transform pointSpawnBullet;
    [SerializeField] private MOH_Move moveState;
    [SerializeField] private ShieldControl shieldControl;
    [SerializeField] private ParticleSystem vfxElectric;
    [SerializeField] private ParticleSystem vfxFire;
    public float timeFixRocket;

    private void OnEnable()
    {
        shieldControl.transform.parent = null;
    }

    public override void EnterState()
    {
        botContext.ChangeAnimAndType(HashShield, 10);
        StartCoroutine(PlayShield());
    }

    public override void UpdateState()
    {
        moveState.RotateToPlayer();
    }

    public override void ExitState()
    {
        StopAllCoroutines();
        botContext.audioPlayable.StopAllAudioDontEnbleFalse();
        vfxElectric.Stop();
        vfxFire.Stop();
        if (shieldControl.gameObject.activeInHierarchy)
            shieldControl.gameObject.SetActive(false);
    }

    public void ShieldExplosion()
    {
        botContext.audioPlayable.StopAllAudioDontEnbleFalse();
        vfxFire.Stop();
        StopAllCoroutines();
        StartCoroutine(IEShieldExplosion());
    }

    private IEnumerator IEShieldExplosion()
    {
        botContext.ChangeAnimAndType(HashShield, 13);
        vfxElectric.Play();
        yield return new WaitForSeconds(6f);
        vfxElectric.Stop();
        botContext.ChangeAnimAndType(HashShield, 10);
        yield return new WaitForSeconds(1f);
        StartCoroutine(PlayShield());
    }

    public IEnumerator PlayShield()
    {
        yield return new WaitForSeconds(.5f);
        shieldControl.OnInit(timeFixRocket, TF.position);
        while (true)
        {
            vfxFire.Play();
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
                
                yield return new WaitForSeconds(Random.Range(3, 5));
            }
            vfxFire.Stop();
            yield return new WaitForSeconds(Random.Range(3, 5));
        }
    }

    public void DoneShield()
    {
        botContext.audioPlayable.StopAllAudioDontEnbleFalse();
        vfxFire.Stop();
        botContext.botNetwork.Other(1);
        botContext.ChangeAnimAndType(HashShield, 12);
    }

    public override void AnimationFinishTrigger() // dùng để chạy anim done shield sau đó chạy phương thức này (trigger animation event)
    {
        base.AnimationFinishTrigger();
        botContext.stateController.ChangeState(EnemyState.Move);
    }
}