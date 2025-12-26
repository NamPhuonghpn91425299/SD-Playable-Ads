using UnityEngine;

public  interface ITakeDamage
{
    void OnTakeDamage(DamageInfo damageInfo);
    Transform GetTransformThis();
    
    Transform GetTransformCenter();
}

public enum DamageType
{
    Normal,
    Weakness,
    Explosion,
}

public struct DamageInfo
{
    public Vector3 posExplosion;
    public int damage;
    public DamageType damageType;
}