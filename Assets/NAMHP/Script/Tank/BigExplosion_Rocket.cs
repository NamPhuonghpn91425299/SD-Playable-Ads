using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigExplosion_Rocket : MonoBehaviour,IPoolObject
{
    public GameObject Prefab { get; set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float timeLifeEx = 3f;
    public void Init()
    {
        audioSource?.Play();
        gameObject.SetActive(true);

        this.DelaySeconds(timeLifeEx,() => ObjectPool.Instance.PushToPool(this,gameObject));
    }

    public void OnPushToPool()
    {

    }

}
