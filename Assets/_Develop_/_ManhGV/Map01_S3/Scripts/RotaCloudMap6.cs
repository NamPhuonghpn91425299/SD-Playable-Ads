using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaCloudMap6 : MonoBehaviour
{
    Transform myTrans;
    [SerializeField] float moveSpeed = 0.1f;
    Quaternion rotaSpeed;
    private void OnEnable()
    {
        myTrans = transform;
        rotaSpeed = Quaternion.Euler(0, moveSpeed, 0);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        myTrans.rotation*= Quaternion.Euler(0, moveSpeed, 0);
    }
}
