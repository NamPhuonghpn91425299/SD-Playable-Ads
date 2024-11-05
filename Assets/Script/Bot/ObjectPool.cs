using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;

    public static ObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject("ObjectPoolManager");
                instance = gameObject.AddComponent<ObjectPool>();
            }
            return instance;
        }

    }
    
    private Dictionary<PoolType, Queue<GameObject>> poolDictionary = new();
    public GameObject GetPooledObject(PoolType type, GameObject prefab, Transform parent = null)
    {
        // Tạo pool mới nếu chưa tồn tại
        if (!poolDictionary.ContainsKey(type))
        {
            poolDictionary[type] = new Queue<GameObject>();
        }

        // Kiểm tra trong pool có object không
        Queue<GameObject> pool = poolDictionary[type];
        GameObject obj = null;

        // Tìm object inactive trong pool
        while (pool.Count > 0 && obj == null)
        {
            obj = pool.Dequeue();
            if (obj == null) continue; // Skip destroyed objects
        }

        // Tạo object mới nếu pool trống
        if (obj == null)
        {
            obj = Instantiate(prefab);
            obj.name = $"{type}_{prefab.name}";
        }

        // Setup object
        if (parent != null)
            obj.transform.SetParent(parent);
        
        obj.SetActive(true);
        
        return obj;
    }

    public void ReturnToPool(PoolType type, GameObject prefab)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            poolDictionary[type] = new Queue<GameObject>();
        }
        prefab.SetActive(false);
        poolDictionary[type].Enqueue(prefab);
        
    }
    
    
}

public enum PoolType
{
    Bot,
    Bullet
}