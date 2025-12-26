using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankStugIV_Network : VehicleNetwork
{
    [SerializeField] private GameObject vfxFire;

    public override void CacularHealth(DamageInfo damageInfo)
    {
        base.CacularHealth(damageInfo);
        if (!vfxFire.activeSelf && currentHealth > 0 && currentHealth <= botConfigSO.health * 0.3f)
            vfxFire.SetActive(true);
    }

    public override void BotDead()
    {
        AchievementEvaluator.Instance.ResetKillData();
        AchievementEvaluator.Instance.GrantMedal(4);
        base.BotDead();
        vfxFire.SetActive(false);
    }
}
