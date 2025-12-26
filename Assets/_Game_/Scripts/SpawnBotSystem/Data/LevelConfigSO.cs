using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRound", menuName = "Spawning/Gameplay/1. Round Kịch Bản")]
public class LevelConfigSO : ScriptableObject
{
    public List<RoundSO> levelRounds;
}