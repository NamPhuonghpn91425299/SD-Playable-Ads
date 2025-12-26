using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterFire : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float amplitude = 0.5f;
    [SerializeField] private Vector3 baseScale = Vector3.one;
    [SerializeField] private Transform effectTrans;
    
    private float timeCount;

    // Start is called before the first frame update
    void Start()
    {
        if (effectTrans == null)
        {
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeCount += Time.deltaTime * speed;
        
        // Sử dụng Mathf.Sin thay thế AnimationCurve
        float scaleX = baseScale.x + Mathf.Sin(timeCount) * amplitude;
        float scaleY = baseScale.y + Mathf.Sin(timeCount * 1.2f) * amplitude;
        float scaleZ = baseScale.z + Mathf.Sin(timeCount * 0.8f) * amplitude;
        
        effectTrans.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }
}
