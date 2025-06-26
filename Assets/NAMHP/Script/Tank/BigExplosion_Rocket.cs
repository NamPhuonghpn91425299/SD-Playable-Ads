using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HelperCoroutine;
public class BigExplosion_Rocket : MonoBehaviour,IPoolObject
{
    public GameObject Prefab { get; set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float timeLifeEx = 3f;
    private Coroutine pushToPoolCoroutine;
    private void OnEnable()
    {
        if (audioSource != null)
        {
            audioSource?.Play();
        }
        if (pushToPoolCoroutine != null)
        {
            StopCoroutine(pushToPoolCoroutine);
        }
        pushToPoolCoroutine = StartCoroutine(PushToPoolCoroutine());
    }
    
    private IEnumerator PushToPoolCoroutine()
    {
        yield return WaitSeconds(timeLifeEx);
        ObjectPool.Instance.PushToPool(this, gameObject);
    }
    public void Init()
    {
        if (Prefab == null)
        {
            Debug.LogError("[BigExplosion_Rocket] Prefab is null! Pooling sẽ lỗi nếu không gán Prefab khi tạo.");
        }
        gameObject.SetActive(true);

        //this.DelaySeconds(timeLifeEx,() => ObjectPool.Instance.PushToPool(this,gameObject));
    }

    public void OnPushToPool()
    {
        //gameObject.SetActive(false);
    }

}
