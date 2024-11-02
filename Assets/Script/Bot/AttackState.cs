using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IState
{
    [SerializeField] private Bot bot;
    [SerializeField] private float acttackTimer;
    [SerializeField] private float acttackCooldown = 1f;
    public AttackState(Bot bot)
    {
        this.bot = bot;
    }

    // Update is called once per frame
    public void Enter()
    {
        Debug.Log("Entering Attack State");
        acttackTimer = 0;
    }

    void IState.Update()
    {
        acttackTimer += Time.deltaTime;
  
        if (acttackTimer >= acttackCooldown)
        {
            // Thực hiện tấn công
            Debug.Log("Performing Attack");
            acttackTimer = 0f;
        }

        if (!bot.IsTargetInRange())
        {
            bot.ChangeState(new IdleState(bot));
        }
    }
    
    
    public void Exit()
    {
        Debug.Log("Exiting Attack State");
    }

    void Update()
    {
        
    }
}
