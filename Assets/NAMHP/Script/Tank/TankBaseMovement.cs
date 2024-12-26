using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class TankBaseMovement : MonoBehaviour
{
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] private BotConfigSO botConfigSO;
    [SerializeField] private Renderer wheelRotation;
    [SerializeField] private float wheelframe = 0;
    public Transform myTrans;
    public Transform target;
    public Transform acttackTurret;
    [SerializeField] private bool isTakeDame;
    private float _countTime = 0;
    //public float RotationSpeed = 5f;

    private void Awake()
    {
        myTrans = transform;
        botNetwork = GetComponent<BotNetwork>();
        ///botNetwork.OnTakeDamage += OnTakeDame;
        botNetwork.OnBotDead += OnBotDead;

    }
    private void OnEnable()
    {
        target = LocalPlayer.Instance.GetTranformPlayer();
    }

    public void SetBotTankMove(Transform point)
    {
        if (!botNetwork.IsDead)
        {
            _countTime += Time.deltaTime;
            wheelframe += botConfigSO.wheelSpeed * Time.deltaTime;
            wheelRotation.material.mainTextureOffset = new Vector2(0, 0.02f * wheelframe);         
            var targetRotation = Quaternion.LookRotation(point.position - myTrans.position);
            myTrans.rotation = Quaternion.Slerp(myTrans.rotation, targetRotation, botConfigSO.targetRotation * Time.deltaTime);
            this.DelaySeconds(1.5f, () =>
            {
                if (!botNetwork.IsDead)
                {
                    myTrans.position = Vector3.MoveTowards(myTrans.position, point.position, botConfigSO.moveSpeed * Time.deltaTime);
                }
            }
            );
            if (_countTime >= 1.5f)
            {
                myTrans.position = Vector3.MoveTowards(myTrans.position, point.position, botConfigSO.moveSpeed * Time.deltaTime);
            }
            float distance = Vector3.Distance(myTrans.position, point.position);    
            if(distance <= .15f && _countTime >= 1.5f)
            {
                _countTime = 0;
            }
        }


    }
    //private void OnTakeDame(int dame)
    //{
    //    isTakeDame = true;
    //    //ator.SetBool("isHit", true);
    //    Invoke(nameof(ResetTakeDame), 0.17f);
    //}
    private void OnBotDead()
    {

    }
    //private void ResetTakeDame()
    //{
    //    isTakeDame = false;

    //}    //private void OnTakeDame(int dame)
    //{
    //    isTakeDame = true;
    //    //ator.SetBool("isHit", true);
    //    Invoke(nameof(ResetTakeDame), 0.17f);
    //}
    //private void OnBotDead()
    //{

    //}
    //private void ResetTakeDame()
    //{
    //    isTakeDame = false;

    //}

}
