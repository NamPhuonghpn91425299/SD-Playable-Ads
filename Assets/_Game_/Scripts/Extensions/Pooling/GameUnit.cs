using System;
using UnityEngine;
using UnityEngine.Serialization;
using static GameConstants;
public class GameUnit<TEnum> : GameUnitBase where TEnum : System.Enum
{
    public  TEnum _poolType;
    private Transform tf;
    public Transform TF
    {
        get
        {
            if (tf == null)
            {
                tf = transform;
            }

            return tf;
        }
    }
    
    public override void Preload(int amount, Transform parent)
    {
        base.Preload(amount, parent);
        SimplePool<TEnum>.Preload(this, amount, parent);
    }
}
