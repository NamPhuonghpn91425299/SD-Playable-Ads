using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState :IState
{
    [SerializeField] private Bot bot;
    //[SerializeField] private Animator animator;
    public MoveState(Bot bot)
    {
        this.bot = bot;
    }

    public void Enter()
    {
        bot.animator.SetBool("isReload",true);
        Debug.Log("Entering Move State");
            //bot.animator.SetBool("isMove",true);
    }

    public void Update()
    {
        //animator.SetBool("isMoveDone",true);
        bot.MoveToTarget();
        if (bot.IsTargetInRange())
        {
            bot.ChangeState(new AttackState(bot));
        }
    }

    public void Exit()
    {
        bot.animator.SetBool("isReload",false);
        Debug.Log("Exiting Move State");
    }
}
