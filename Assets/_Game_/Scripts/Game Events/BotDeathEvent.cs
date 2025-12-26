using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

[System.Serializable]
public class BotDeathEvent : IGameEvent
{
    public float Timestamp => Time.time;
    public int? totalBots { get; }
    public string GetProperty { get; }
    public BotDeathEvent(int? totalBots = null, string getProperty = "")
    {
        this.totalBots = totalBots;
        GetProperty = getProperty;
    }
}
