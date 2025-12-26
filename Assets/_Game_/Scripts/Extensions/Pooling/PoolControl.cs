using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolControl : MonoBehaviour 
{
    [SerializeField] private List<PoolAmount> prefabsToPreload = new List<PoolAmount>();

    void Awake()
    {
        foreach (PoolAmount prefab in prefabsToPreload)
        {
            prefab.gameUnitBase.Preload(prefab.amount, prefab.parent);
        }
    }
}

[Serializable]
public class PoolAmount
{
    public GameUnitBase gameUnitBase;
    public Transform parent;
    public int amount;
}
[Serializable]
public abstract class GameUnitBase : MonoBehaviour ,IPoolable
{
    public virtual void Preload(int amount, Transform parent)
    {
        
    }
}

public interface IPoolable
{
    void Preload(int amount, Transform parent);
}

