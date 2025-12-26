using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ScriptableObject để lưu Pool Data
[CreateAssetMenu(fileName = "PoolData", menuName = "Pool/Pool Data", order = 1)]
public class PoolData : ScriptableObject
{
    [Header("Pool Configuration")]
    public List<EnumPool> enumPools = new List<EnumPool>();
}
