using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

[System.Serializable]
public class ChangeProjectileGunEvent : IGameEvent
{
    public GameConstants.ProjecttilePlayer typeProjectile { get;  }
    public float fireRate { get;  }
    public float Timestamp => Time.time;

    public ChangeProjectileGunEvent(GameConstants.ProjecttilePlayer _typeProjectile, float _fireRate)
    {
        fireRate = _fireRate;
        typeProjectile = _typeProjectile;
    }
}
