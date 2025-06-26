using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITakeDamage
{
    void TakeDamage(DamageInfo damageInfo);
}
public interface IReward
{
    void TakeCollect(int damage);
}
public enum DamageType
{
    Normal,
    Weekness,
    Gas,
}
public struct DamageInfo
{
    public string  name;
    public int damage;
    public DamageType damageType;
}