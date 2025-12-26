using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

public class ExplosionPanzerwerfer : GameUnit<ProjectileEnemy>
{
    private void OnEnable()
    {
        StartCoroutine(IEAutoDespawn());
    }

    private IEnumerator IEAutoDespawn()
    {
        yield return new WaitForSeconds(.55f);
        SimplePool<ProjectileEnemy>.Despawn(this);
    }
}
