using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BotConfigSO", menuName = "Game/Bot Config")]
public class BotConfigSO : ScriptableObject
{
    public string botId;
    public GameObject prefab;
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackSpeed = 1f;
}
