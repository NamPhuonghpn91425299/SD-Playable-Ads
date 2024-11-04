using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadState : IState
{
    private Bot bot;
    private float reloadTimer;
    private float reloadDuration = 3f;

    public ReloadState(Bot bot)
    {
        this.bot = bot;
    }

    public void Enter()
    {
        bot.animator.SetBool("isReload",true);
        Debug.Log("Bot bắt đầu thay đạn");
        reloadTimer = 0f;
    }

    public void Update()
    {
        reloadTimer += Time.deltaTime;

        if (reloadTimer >= reloadDuration)
        {
            bot.ChangeState(new AttackState(bot));
        }
    }

    public void Exit()
    {
        bot.animator.SetBool("isReload",false);
        Debug.Log("Bot thay đạn xong");
    }


}
