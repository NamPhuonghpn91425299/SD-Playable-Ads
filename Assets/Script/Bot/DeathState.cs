using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : IState
{
    private Bot bot;
    private float deathTimer;
    private float despawnTime = 2f;
    public DeathState(Bot bot)
    {
        this.bot = bot;
    }

    public void Enter()
    {
        Debug.Log("Entering Death State");
        // Có thể thêm animation chết ở đây
    }

    public void Update()
    {
        deathTimer += Time.deltaTime;
        if (deathTimer >= despawnTime)
        {
            BotManager.Instance.DespawnBot(bot);
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Death State");
    }
}
