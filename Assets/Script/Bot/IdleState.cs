using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IState
{
    public Bot bot;
    [SerializeField] private float idleTimer;
    
    [SerializeField] private float maxIdleTime = 3f;

    public IdleState(Bot bot)
    {
        this.bot = bot;
    }
    
    public void Enter()
    {
        Debug.Log("Entering Idle State");
        idleTimer = 0;
    }

    public void Update()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= maxIdleTime)
        {
            bot.ChangeState(new MoveState(bot));
        }
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }
}
