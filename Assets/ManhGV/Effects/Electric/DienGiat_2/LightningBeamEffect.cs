using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBeamEffect : MonoBehaviour,IPoolObject
{
    [SerializeField] LineRenderer line;
    [SerializeField] float lifeTime = 0.5f;
    // Start is called before the first frame update
    float xOffset = 0;


    // Update is called once per frame
    void Update()
    {
        xOffset += Time.deltaTime*9;
        if (xOffset>=0.6) xOffset = 0;
        line.material.mainTextureOffset = new Vector2(xOffset, 0);
        Invoke(nameof(OnDespawn), lifeTime);
    }

    public GameObject Prefab { get; set; }
    public void Init(Vector3 start,Vector3 end) 
    {
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    public void Init()
    {
        
    }

    public void OnPushToPool()
    {
        
    }

    public void OnDespawn()
    {
        ObjectPool.Instance.PushToPool(this, gameObject);
    }
}
