using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState :IState
{
    [SerializeField] private Bot bot;
    public MoveState(Bot bot)
    {
        this.bot = bot;
    }

    public void Enter()
    {
        Debug.Log("Entering Move State");
    }

    public void Update()
    {
        bot.MoveToTarget();
        if (bot.IsTargetInRange())
        {
            bot.ChangeState(new AttackState(bot));
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Move State");
    }
}
