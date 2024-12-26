using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechMoveBase : MonoBehaviour
{
    [SerializeField] protected BotNetwork botNet;
    [SerializeField] protected BotConfigSO BotConfigSO;
    [SerializeField] protected Animator ator;
    public bool isHaveParent;
    public Transform myTrans;
    public bool isTakeDame;


    private void Awake()
    {
        myTrans = transform;
    }
    protected virtual void OnEnable()
    {
        botNet.OnTakeDamage += OnTakeDame;
        botNet.OnBotDead += OnBotDead;

    }

    protected void Update()
    {

    }

    private void OnTakeDame(int dame)
    {
        isTakeDame = true;
        Invoke(nameof(ResetTakeDame), 0.17f);
    }
    private void OnBotDead()
    {
    }
    private void ResetTakeDame()
    {
        isTakeDame = false;
    }

    public void SetBotMove(Transform point)
    {
        if (!isTakeDame && !botNet.IsDead)
        {
            var targetRotation = Quaternion.LookRotation(point.position - myTrans.position);
            myTrans.rotation = Quaternion.Slerp(myTrans.rotation, targetRotation, 10 * Time.deltaTime);
            myTrans.position = Vector3.MoveTowards(myTrans.position, point.position, BotConfigSO.moveSpeed * Time.deltaTime);
            ator.SetBool("IsMove", true);
            ator.speed = 0.5f;
            
        }

    }
}
