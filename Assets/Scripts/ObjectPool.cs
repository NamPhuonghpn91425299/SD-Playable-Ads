using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class ObjectPool : MonoBehaviour
{
    private static readonly Dictionary<GameObject, Queue<GameObject>> container =
        new Dictionary<GameObject, Queue<GameObject>>();

    private static ObjectPool instance = null;

    public static ObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ObjectPool>();
            }
#if DEVELOPER_MODE && UNITY_EDITOR
            if (instance == null)
            {
                UtilsHr.SpawnGameSingleton();
            }
#endif
            return instance;
        }
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene scene, LoadSceneMode loadSceneMode)
    {
        ReleasePool();
    }

    /// <summary>
    /// Reset the pool but does not destroy the content.
    /// </summary>
    public void Reset()
    {
        instance = null;
    }

    /// <summary>
    /// Adds to pool.
    /// </summary>
    /// <returns><c>true</c>, if item was successfully created, <c>false</c> otherwise.</returns>
    /// <param name="prefab">The prefab to instantiate new items.</param>
    /// <param name="count">The amount of instances to be created.</param>
    /// <param name="parent">The Transform container to store the items. If null, items are placed as parent</param>
    public bool AddToPool(GameObject prefab, int count, Transform parent = null)
    {
        if (prefab == null || count <= 0)
        {
            return false;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = PopFromPool(prefab, true, false, parent);
            PushToPool(ref obj, true, parent);
        }

        return true;
    }

    /// <summary>
    /// Pops item from pool.
    /// </summary>
    /// <returns>The from pool.</returns>
    /// <param name="prefab">Prefab to be used. Matches the prefab used to create the instance</param>
    /// <param name="forceInstantiate">If set to <c>true</c> force instantiate regardless the pool already contains the same item.</param>
    /// <param name="instantiateIfNone">If set to <c>true</c> instantiate if no item is found in the pool.</param>
    /// <param name="container">The Transform container to store the popped item.</param>
    public GameObject PopFromPool(GameObject prefab, bool forceInstantiate = false, bool instantiateIfNone = false,
        Transform container = null)
    {
        //Debug.Log("PopFromPool " + prefab);
        GameObject obj = null;

        if (forceInstantiate)
        {
            obj = CreateObject(prefab, null);
        }
        else
        {
            Queue<GameObject> queue = FindInContainer(prefab);
            if (queue == null)
                return null;
            if (queue.Count > 0)
            {
                obj = queue.Dequeue();
                if (obj != null)
                {
                    obj.SetActive(true);
                    obj.transform.SetParent(container, false);
                }
            }
        }

        if (obj == null && instantiateIfNone)
        {
            obj = CreateObject(prefab, container);
        }

        if (obj != null)
        {
            obj.GetComponent<IPoolObject>().Init();
        }

        return obj;
    }

    private Queue<GameObject> FindInContainer(GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        if (!container.TryGetValue(prefab, out Queue<GameObject> prefabQueue))
        {
            container[prefab] = prefabQueue = new Queue<GameObject>();
        }

        return prefabQueue;
    }

    private GameObject CreateObject(GameObject prefab, Transform parent)
    {
        IPoolObject poolObjectPrefab = prefab.GetComponent<IPoolObject>();
        if (poolObjectPrefab == null)
        {
            Debug.Log("Wrong type of object");
            return null;
        }

        GameObject obj = (GameObject)Object.Instantiate(prefab);
        IPoolObject poolObject = obj.GetComponent<IPoolObject>();
        obj.name = prefab.name;
        poolObject.Prefab = prefab;

        obj.transform.SetParent(parent, false);
        return obj;
    }

    /// <summary>
    /// Pushes back the item to the pool.
    /// </summary>
    /// <param name="obj">A reference to the item to be pushed back.</param>
    /// <param name="retainObject">If set to <c>true</c> retain object.</param>
    /// <param name="newParent">The Transform container to store the item.</param>
    public void PushToPool(ref GameObject obj, bool retainObject = true, Transform newParent = null)
    {
        if (this == null || obj == null)
        {
            return;
        }

        if (!retainObject)
        {
            Object.Destroy(obj);
            obj = null;
            return;
        }

        if (newParent != null)
        {
            obj.transform.SetParent(newParent, false);
        }

        IPoolObject poolObject = obj.GetComponent<IPoolObject>();
        if (poolObject != null)
        {
            GameObject prefab = poolObject.Prefab;

            Queue<GameObject> queue = FindInContainer(prefab);
            queue?.Enqueue(obj);

            poolObject.OnPushToPool();
            obj.SetActive(false);
        }

        obj = null;
    }

    public void PushToPool(IPoolObject target, GameObject gameObj)
    {
        if (target != null)
        {
            GameObject prefab = target.Prefab;
            Queue<GameObject> queue = FindInContainer(prefab);
            if (queue != null)
                queue.Enqueue(gameObj);

            target.OnPushToPool();
            gameObj.SetActive(false);
        }
    }

    /// <summary>
    /// Releases the pool from all items.
    /// </summary>
    /// <param name="prefab">The prefab to be used to find the items.</param>
    /// <param name="destroyObject">If set to <c>true</c> destroy object, else object is removed from pool but kept in scene. </param>
    public void ReleaseItems(GameObject prefab, bool destroyObject = false)
    {
        if (prefab == null)
        {
            return;
        }

        Queue<GameObject> queue = FindInContainer(prefab);
        if (queue == null)
        {
            return;
        }

        while (queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            if (destroyObject)
            {
                Object.Destroy(obj);
            }
        }
    }

    /// <summary>
    /// Releases all items from the pool and destroys them.
    /// </summary>
    public void ReleasePool()
    {
        foreach (var kvp in container)
        {
            Queue<GameObject> queue = kvp.Value;
            while (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                Object.Destroy(obj);
            }
        }

        container.Clear();
    }
}

public interface IPoolObject
{
    GameObject Prefab { get; set; }
    void Init();
    void OnPushToPool();
}
