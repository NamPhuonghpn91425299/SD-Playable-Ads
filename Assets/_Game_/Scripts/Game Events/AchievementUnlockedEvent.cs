using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

public class AchievementUnlockedEvent : IGameEvent
{
    public float Timestamp => Time.time;

    public GameConstants.AchievementType AchievementType { get; set; }
    public string Description { get; set; }


    public AchievementUnlockedEvent(GameConstants.AchievementType achievementType, string description)
    {
        AchievementType = achievementType;
        Description = description;
    }
}
