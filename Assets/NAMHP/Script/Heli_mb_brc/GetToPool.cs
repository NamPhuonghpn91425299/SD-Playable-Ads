using System.Collections;
using UnityEngine;

public class GetToPool : MonoBehaviour,IPoolObject
{
    [SerializeField] private float timeDaley = 2f; // Thời gian chờ trước khi đưa vào pool
    private void OnEnable()
    {
        // Đưa tên lửa vào pool sau một khoảng thời gian
        StartCoroutine(DelayReturnToPool());
    }
    private void OnDisable()
    {
        
    }

    private IEnumerator DelayReturnToPool()
    {
        yield return new WaitForSeconds(timeDaley);
        ObjectPool.Instance.PushToPool(this, gameObject); // Đưa tên lửa vào pool
        gameObject.SetActive(false); // Đưa tên lửa vào pool
    }

    public GameObject Prefab { get; set; }
    public void Init()
    {
        
    }

    public void OnPushToPool()
    { ;
    }
}