using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamageReceiver : MonoBehaviour, IDamageHit
{
    [SerializeField] protected float hp = 1f;
    [SerializeField] protected float hpMax = 2f;
    [SerializeField] protected bool isDead = false;
    //[SerializeField] private WeaponDataSO _weaponDataSo;
    protected virtual void OnEnable()
    {
        this.Reborn();
    }
  
    public virtual void Reborn()
    {
        this.hp = this.hpMax;
        this.isDead = false;
    }
    public static float HandleDamage(float damage, float armor, float damageCrit)
    {
        return Mathf.Max(0f, (damage - armor)*damageCrit);
    }
    public virtual void Add(float add)
    {
        if (this.isDead) return;
        this.hp += add;
        if (this.hp >= this.hpMax) this.hp = this.hpMax;
    } 
    // public virtual void Deduct(float deduct)
    // {
    //     if (this.isDead) return;
    //     
    //     this.hp -= deduct;
    //     if (this.hp < 0) this.hp = 0;
    //     this.CheckIsDead();
    // }
    protected virtual bool IsDead()
    {
        return this.hp <= 0;
    }
    protected virtual void CheckIsDead()
    {
        if (!IsDead()) return;
        this.isDead = true;
        this.OnDead();
    }
    protected virtual void OnDead()
    {
        this.isDead = true;
    }

   public virtual void OnHit(int damage) 
    {
        if (this.isDead) return;
        this.hp -= damage;
        if (this.hp < 0) this.hp = 0;
        this.CheckIsDead();
    }
    
    
}

