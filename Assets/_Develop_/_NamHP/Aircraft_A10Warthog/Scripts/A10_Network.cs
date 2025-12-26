using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A10_Network : VehicleNetwork
{
    [SerializeField] private GameObject vfxDead1;
    public override void CacularHealth(DamageInfo damageInfo)
    {
        base.CacularHealth(damageInfo);
        if (!vfxDead1.activeSelf && currentHealth > 0 && currentHealth <= botConfigSO.health * 0.3f)
        {
            vfxDead1.SetActive(true);
        }
    }

    public override void BotDead()
    {
        AchievementEvaluator.Instance.ResetKillData();
        AchievementEvaluator.Instance.GrantMedal(4);
        base.BotDead();
        vfxDead1.SetActive(false);
        
    }
}
