using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameResultData", menuName = "ScriptableObjects/GameResultData", order = 1)]
public class GameResultData : ScriptableObject
{
    [Header("Đếm số màn chơi")]
    [Tooltip("Màn chơi để ShowEndCard")]
    public int TurnEnd;
    [Tooltip("Biến để đếm số màn chơi")]
    public int TurnCount;
    public bool IsCountTurn;

    [Header("Đếm số lần Bot nhảy dù chạm đất")]
    [Tooltip("Đếm số lần Bot nhảy dù chạm đất")]
    public int BotLandingCount;
    [Tooltip("Số lần Bot đáp đất để ShowEndCard")]
    public int BotLandingCountConfig;
    public bool IsCountLandingBot;

    [Header("Đếm số BotKill")]
    [Tooltip("Đếm số lần BotKill")]
    public int BotKillCount;
    public bool IsCountBotKill;
}
