using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleNetwork : EnemyBase
{
    [Header("Explosion Settings")] 
    [SerializeField] private float radiusExplosion = 5f;
    [SerializeField] private int damageExplosion = 150;
    [SerializeField] private LayerMask layerTargetExplosion;
    
    public override void OnTakeDamage(DamageInfo damageInfo)
    {
        if (IsDeadExplosion || isDead || isImmortal)
            return;

        BotAudio?.PlayAudio(GameConstants.AudioType.GetHit);


        //ACOnTakeDamage?.Invoke(damageInfo.damage);
        CallActionOnTakeDamage(damageInfo);


        if (healthBarTransform != null)// && damageInfo.damageType != DamageType.Explosion)//||healthBarTransform != null && isBoss && damageInfo.damageType == DamageType.Gas)
        {
            CacularHealth(damageInfo);
            healthBarTransform.gameObject.SetActive(true);

            if (hideHealthBarCoroutine != null)
                StopCoroutine(hideHealthBarCoroutine);

            hideHealthBarCoroutine = StartCoroutine(IEHideHealthBarAfterDelay());
        }
        // else if(damageInfo.damageType == DamageType.Explosion)
        // {
        //     CacularHealth(damageInfo);
        //     healthBarTransform.gameObject.SetActive(true);
        //     
        //     if (hideHealthBarCoroutine != null)
        //         StopCoroutine(hideHealthBarCoroutine);
        //
        //     hideHealthBarCoroutine = StartCoroutine(IEHideHealthBarAfterDelay());
        // }
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
        ExplosionAndTakeDamageInRadius();
        BotSpawnManager.Instance.botInScene.Remove(GetTransformCenter());
        if (botIdentity.Type != SpawnableType.None)
            botIdentity.Bot_ReportKill();
    }

    public override void ExplosionAndTakeDamageInRadius()
    {
        base.ExplosionAndTakeDamageInRadius();
#if UNITY_EDITOR
        if (GetTransformCenter() == null)
        {
            Debug.LogError("VehicleNetwork ExplosionAndTakeDamageInRadius: GetTransformCenter() is null");
            return;
        }
#endif
        Collider[] cols = Physics.OverlapSphere(TF.position, radiusExplosion, layerTargetExplosion);
        List<Transform> lstRoot = new List<Transform> ();
        
        foreach (Collider col in cols)
            if (!lstRoot.Contains(col.gameObject.transform.root))
                lstRoot.Add(col.gameObject.transform.root);
        
        foreach(var elem in lstRoot)
        {
            ITakeDamage iTakeDamage = elem.gameObject.GetComponentInParent<ITakeDamage>();
            if(iTakeDamage == null)
                iTakeDamage = elem.gameObject.GetComponent<ITakeDamage>();
            
            if (iTakeDamage != null)
            {
                var damageInfo = new DamageInfo()
                {
                    damageType = DamageType.Explosion,
                    damage = damageExplosion,
                    posExplosion = GetTransformCenter().position,
                };
                iTakeDamage.OnTakeDamage(damageInfo);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (GetTransformCenter() != null)
            Gizmos.DrawWireSphere(GetTransformCenter().position, radiusExplosion);
    }
}