using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IState
{
    [SerializeField] private Bot bot;
    [SerializeField] private float acttackTimer;
    [SerializeField] private float acttackCooldown = 1f;
    [SerializeField] private float acttackTime = 8f;

    public AttackState(Bot bot)
    {
        this.bot = bot;
    }

    // Update is called once per frame
    public void Enter()
    {

        Debug.Log("Entering Attack State");
        bot.animator.SetBool("isMoveDone",true);
        acttackTimer = 0;
    }

    void IState.Update()
    {
        bot.LockAtTager();
        acttackTimer += Time.deltaTime;
  
        if (acttackTimer >= acttackCooldown)
        {
            bot.isAttacking = true;
            bot.ActtackToTarget();
            // Thực hiện tấn công
            Debug.Log("Performing Attack");
            //acttackTimer = 0f;
        }

        if (acttackTimer >= acttackTime)
        {
            bot.isAttacking = false;
            bot.ActtackToTarget();
            acttackTimer = 0;
            bot.ChangeState(new ReloadState(bot));
        }
        if (!bot.IsTargetInRange())
        {
            bot.ChangeState(new IdleState(bot));
        }
        
    }
    
    
    public void Exit()
    {
        bot.animator.SetBool("isMoveDone", false);
        bot.isAttacking = false;
        bot.ActtackToTarget();
        Debug.Log("Exiting Attack State");
    }

    void Update()
    {
        
    }
}
