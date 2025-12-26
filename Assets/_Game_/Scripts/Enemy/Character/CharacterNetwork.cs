using System;
using UnityEngine;

public class CharacterNetwork : EnemyBase
{
    // public override void OnInit(WayPoint _wayPoint)
    // {
    //     base.OnInit(_wayPoint);
    //     IsDeadExplosion = false;
    // }

    public override void OnTakeDamage(DamageInfo damageInfo)
    {
        if(IsDeadExplosion || isDead || isImmortal)
            return;
        CallActionOnTakeDamage(damageInfo);
        
        if(healthBarTransform != null && damageInfo.damageType != DamageType.Explosion)//||healthBarTransform != null && isBoss && damageInfo.damageType == DamageType.Gas)
        {
            CacularHealth(damageInfo);
            healthBarTransform.gameObject.SetActive(true);
            
            if (hideHealthBarCoroutine != null)
                StopCoroutine(hideHealthBarCoroutine);

            hideHealthBarCoroutine = StartCoroutine(IEHideHealthBarAfterDelay());
        }
        else if(damageInfo.damageType == DamageType.Explosion)
        {
            posExplosion = damageInfo.posExplosion;
            _currentHealth = 0;
            IsDeadExplosion = true;
            stateController?.DeadExplosion();
            CacularHealth(damageInfo);
        }
    }

    public override void BotDead()
    {
        if (PointKillCaculatorMeldal < 10)
            AchievementEvaluator.Instance.OnBotKilled(PointKillCaculatorMeldal,false);
        else
        {
            AchievementEvaluator.Instance.ResetKillData();
            AchievementEvaluator.Instance.GrantMedal(4);
        }
        base.BotDead();
        BotSpawnManager.Instance.botInScene.Remove(GetTransformCenter());
        if (botIdentity.Type != SpawnableType.None)
            botIdentity.Bot_ReportKill();
    }
}