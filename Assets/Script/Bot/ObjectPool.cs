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
                GameObject gameObject = new GameObject("ObjectPool");
                instance = gameObject.AddComponent<ObjectPool>();
            }
            return instance;
        }

    }
    
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    public GameObject GetPooledObject(string tag, GameObject prefab, Transform parent = null)
    {
        // Tạo pool mới nếu chưa tồn tại
        if (!poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag] = new Queue<GameObject>();
        }

        // Kiểm tra trong pool có object không
        Queue<GameObject> pool = poolDictionary[tag];
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
            obj.name = $"{tag}_pooled";
        }

        // Setup object
        if (parent != null)
            obj.transform.SetParent(parent);
        
        obj.SetActive(true);
        
        return obj;
    }

    public void ReturnToPool(string tag, GameObject prefab)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag] = new Queue<GameObject>();
        }
        prefab.SetActive(false);
        poolDictionary[tag].Enqueue(prefab);
    }
    
    
}
