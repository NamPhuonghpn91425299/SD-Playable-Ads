using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

public class HealthObject<TEnum> : GameUnit<TEnum>, ITakeDamage where TEnum : System.Enum
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("VFX Play On Death")]
    public ParticleSystem[] vfxOnDeath;
    
    public virtual void OnInit()
    {
        currentHealth = maxHealth;
    }
    
    public virtual void OnTakeDamage(DamageInfo damageInfo)
    {
        currentHealth -= damageInfo.damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        
    }

    public virtual void OnDespawn()
    {
        SimplePool<TEnum>.Despawn(this);
    }

    public Transform GetTransformThis() => null;

    public Transform GetTransformCenter() => null;
}