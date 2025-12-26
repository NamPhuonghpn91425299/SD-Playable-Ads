using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

public class Powerup_Projectile : Powerup<GameConstants.ProjecttilePlayer>
{
    [Header("Fire Rate")]
    public float fireRate = 0.8f;
    protected override void OnDeath()
    {
        base.OnDeath();
        EventManager.Instance?.Publish(new ChangeProjectileGunEvent(typeGift,fireRate));
        
    }
}