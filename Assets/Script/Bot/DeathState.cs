using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : IState
{
    private Bot bot;
    private float despawnTime = 2f;
    public DeathState(Bot bot)
    {
        this.bot = bot;
    }

    public void Enter()
    {
        // bot.animator.SetBool("isDead",true);
        bot.animator.SetTrigger("isDead1");
        bot.animator.SetFloat("DeadStyle",Random.Range(0f,2f));
        bot.StartCoroutine(HideBotOnDeath());
        Debug.Log("Entering Death State");
        // Có thể thêm animation chết ở đây
    }

    IEnumerator HideBotOnDeath()
    {
        yield return new WaitForSeconds(despawnTime);
        BotManager.Instance.DespawnBot(bot);
    }
    public void Update()
    {
        // deathTimer += Time.deltaTime;
        // if (deathTimer >= despawnTime)
        // {
        //     BotManager.Instance.DespawnBot(bot);
        // }
    }

    public void Exit()
    {
        Debug.Log("Exiting Death State");
    }
}
