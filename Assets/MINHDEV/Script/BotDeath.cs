using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotDeath : MonoBehaviour
{
    public static BotDeath Instance;
    public int BotDeathCount;

    private void Awake()
    {
        Instance = this;
    }
    private void OnDisable()
    {
        BotDeathCount = 0;
        GameResultInstance.Instance.GetGameResultData().BotKillCount=0;
    }

    public int GetBotDeath()
    {
        GameResultInstance.Instance.GetGameResultData().BotKillCount++;
        //Debug.Log(string.Format("Bot Death Count: {0}", BotDeathCount));
        EventManager.Invoke(EventName.OnBotKillCount, GameResultInstance.Instance.GetGameResultData().BotKillCount);
        return BotDeathCount++;
    }

}
