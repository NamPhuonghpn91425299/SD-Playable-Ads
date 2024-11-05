using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState :IState
{
    [SerializeField] private Bot bot;
    [SerializeField] private Vector3 destination;
    //[SerializeField] private Animator animator;
    public MoveState(Bot bot)
    {
        this.bot = bot;
    }

    public void Enter()
    {
        bot.animator.SetBool("isReload",true);
        destination = bot.GetPoint();
        Debug.Log("Entering Move State");
            //bot.animator.SetBool("isMove",true);
            
    }

    public bool checkDistance => Vector3.Distance(destination, bot.transform.position) < 1f;
    
    
    

    public void Update()
    {
        if (checkDistance)
        {
            
            if (!bot.isLastPoint()) 
            {
                
                bot.ChangeState(new AttackState(bot));
            }
            else
            {
                bot.ChangeState(this);
            }
            
        }
        else
        {
            bot.MoveToTarget(destination);
        }
        
        //animator.SetBool("isMoveDone",true);
        // bot.MoveToNextPoint();
        //bot.MoveToTarget();
        // if (bot.IsTargetInRange())
        // {
        //     bot.ChangeState(new AttackState(bot));
        // }
    }

    public void Exit()
    {
        bot.animator.SetBool("isReload",false);
        Debug.Log("Exiting Move State");
    }
}
