using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup_Weapon : Powerup<GameConstants.Weapon>
{
    protected override void OnDeath()
    {
        base.OnDeath();
        GameController gameController = GameController.Instance;
        WeaponBase weaponBase = SimplePool<GameConstants.Weapon>.Spawn<WeaponBase>(typeGift, Vector3.zero, Quaternion.identity);
        gameController.CurrentWeapon.OnDespawn();
        weaponBase.OnInit();
    }
}