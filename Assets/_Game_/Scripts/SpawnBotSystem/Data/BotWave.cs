// FileName: SpawnStep.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static GameConstants;
[Serializable]
public class BotWave
{
    
    [Header("WHAT & WHERE")]
    [Tooltip("Loại Bot sẽ được spawn trong bước này.")]
    public BotType BotToSpawn;
    // [FormerlySerializedAs("PathToUse")] [Tooltip("Đường đi mà Bot sẽ sử dụng.")]
    // public BotMoveType botMoveType;

    [Header("HOW")]
    [Tooltip("Số lượng Bot sẽ được spawn.")]
    [Range(1, 50)]
    public int Quantity = 1;
    [Tooltip("Thời gian chờ (giây) giữa mỗi lần spawn nếu Quantity > 1.")]
    public float DelayBetweenSpawns = 0.2f;

    [Header("WHEN")]
    [Tooltip("Các điều kiện cần được thỏa mãn ĐỂ BƯỚC NÀY được kích hoạt.")]
    public List<ConditionDefinition> Conditions;
}