using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

public static class SimplePool<TEnum> where TEnum : System.Enum
{
    private static Dictionary<TEnum, Pool<TEnum>> poolInsstance = new Dictionary<TEnum, Pool<TEnum>>();

    public static void Preload(GameUnit<TEnum> prefab, int amount, Transform parent)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefabs Is Empty !!!");
            return;
        }

        if (!poolInsstance.ContainsKey(prefab._poolType) || poolInsstance[prefab._poolType] == null)
        {
            Pool<TEnum> p = new Pool<TEnum>();
            p.Preload(prefab, amount, parent);
            poolInsstance[prefab._poolType] = p;
        }
    }

    public static T Spawn<T>(TEnum poolType, Vector3 pos, Quaternion rot, Transform parent = null) where T : GameUnit<TEnum>
    {
        if (!poolInsstance.ContainsKey(poolType))
        {
            Debug.LogError(poolType + " Is Not Reload !!!");
            return null;
        }
        if(parent == null)
            return poolInsstance[poolType].Spawn(pos, rot) as T;
        else
            return poolInsstance[poolType].Spawn(pos, rot, parent) as T;
    }

    public static void Despawn(GameUnit<TEnum> unit)
    {
        if (!poolInsstance.ContainsKey(unit._poolType))
        {
            Debug.LogError(unit._poolType + " Is Not Reload !!!");
            unit.gameObject.SetActive(false);
            return;
        }

        poolInsstance[unit._poolType].Despawn(unit);
    }

    public static void Despawn(GameUnit<TEnum> unit, float delay)
    {
        unit.Invoke(nameof(Despawn), delay);
    }
    

    

    public static void Collect(TEnum poolType)
    {
        if (!poolInsstance.ContainsKey(poolType))
        {
            Debug.LogError(poolType + " Is Not Reload !!!");
        }

        poolInsstance[poolType].Collect();
    }

    public static void CollectAll()
    {
        foreach (var pool in poolInsstance.Values)
        {
            pool.Collect();
        }
    }

    public static void Release(TEnum poolType)
    {
        if (!poolInsstance.ContainsKey(poolType))
        {
            Debug.LogError(poolType + " Is Not Reload !!!");
        }

        poolInsstance[poolType].Release();
    }

    public static void ReleaseAll()
    {
        foreach (var pool in poolInsstance.Values)
        {
            pool.Release();
        }
    }

}

public class Pool<TEnum> where TEnum : System.Enum
{
    Transform parent;
    GameUnit<TEnum> prefabs;


    Queue<GameUnit<TEnum>> inactives = new Queue<GameUnit<TEnum>>();
    List<GameUnit<TEnum>> actives = new List<GameUnit<TEnum>>();
    
    public void Preload(GameUnit<TEnum> prefab, int amount, Transform parent)
    {
        this.parent = parent;
        this.prefabs = prefab;

        for (int i = 0; i < amount; i++)
        {
            Despawn(GameObject.Instantiate(prefabs, parent));
        }
    }

    public GameUnit<TEnum> Spawn(Vector3 pos, Quaternion rot, Transform parent = null)
    {
        GameUnit<TEnum> unit;

        if (inactives.Count <= 0)
        {
            unit = GameObject.Instantiate(prefabs, parent);
        }
        else
        {
            unit = inactives.Dequeue();
        }

        if (parent == null)
        {
            //unit.TF.SetLocalPositionAndRotation(pos, rot);
            unit.TF.localPosition = pos;
            unit.TF.localRotation = rot;
        }
        else
        {
            unit.TF.parent = parent;
            unit.TF.localRotation = Quaternion.Euler(Vector3.zero);
            unit.TF.localPosition = Vector3.zero;
        }

        actives.Add(unit);
        unit.gameObject.SetActive(true);
        return unit;
    }

    public void Despawn(GameUnit<TEnum> unit)
    {
        if (unit != null && unit.gameObject.activeSelf)
        {
            actives.Remove(unit);
            inactives.Enqueue(unit);
            unit.gameObject.SetActive(false);
        }
    }

    public void Collect()
    {
        while (inactives.Count > 0)
        {
            Despawn(actives[0]);
        }
    }

    public void Release()
    {
        Collect();
        while (inactives.Count > 0)
        {
            GameObject.Destroy(inactives.Dequeue().gameObject);
        }

        inactives.Clear();
    }
}