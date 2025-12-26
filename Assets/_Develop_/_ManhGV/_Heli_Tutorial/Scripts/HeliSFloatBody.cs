using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliSFloatBody : MonoBehaviour
{

    [Header("SETTING VALUE")]
    [SerializeField] float rotaRange = 0.5f;
    [SerializeField] float speed = 1f;
    [SerializeField] float rotaPow = 2f;

   // [Header("DEBUG VALUE")]
  [HideInInspector]  public bool stopFloat = false;
    Vector3 floatPos = new Vector3();
    float t = 0f;
    float sc;
    Transform mytrans;
    private void Awake()
    {
        mytrans = transform;
    }

    private void OnEnable()
    {
        stopFloat = false;
        floatPos = Random.onUnitSphere * rotaRange;
    }
    // Update is called once per frame
    void Update()
    {
          TiltBodyOnMove();
    }


    int count = 0;
    void TiltBodyOnMove()
    {
         sc = SineEaseIn(t, 0f, 1f, speed);
        if (sc < 0.004) 
        {
            if (stopFloat)
            {
                floatPos = new Vector3();
                count++;
                if (count==2)
                {
                    this.enabled = false;
                }
            }
            else
            {
                floatPos = Random.onUnitSphere * rotaRange;
            }
           

        }
        float xValue= Mathf.LerpUnclamped(0, floatPos.x, sc);
        float yValue = Mathf.LerpUnclamped(0, floatPos.y, sc);
        float zValue = Mathf.LerpUnclamped(0, floatPos.z, sc);
        mytrans.localRotation = Quaternion.Euler(0, 0, zValue* rotaPow);
        mytrans.localPosition = new Vector3(xValue, yValue, zValue);
        t += Time.deltaTime;
    }

    public float SineEaseIn(float t, float b, float c, float d)
    {
        return -c * (float)Mathf.Cos(t / d * _HALF_PI) + c + b;
    }
    private const float _HALF_PI = 1.5707963267949f;

    public void ResetRotation()
    {
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
}
