using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementSystemRunner : MonoBehaviour
{
    public MedalsUI medalsUI;
    public AchievementEvaluator achievementEvaluator;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)||Input.GetKeyDown(KeyCode.Space))
        {
            achievementEvaluator.OnBotKilled(1f, isSubBoss: false);
        }
    }
}

