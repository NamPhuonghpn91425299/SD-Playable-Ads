
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;


[System.Serializable]
public class RocketEvent : IGameEvent
{
    public float Timestamp => Time.time;
    public string State { get; }
    public float? TimerReload { get; }
    public int? RocketCount { get; }
    public bool IsRocketOn { get; set; }

    public RocketEvent(bool isRocketOn, string state = "", float? timerReload = null, int? rocketCount = null)
    {
        State = state;
        TimerReload = timerReload;
        RocketCount = rocketCount;
        IsRocketOn = isRocketOn;
    }
}
