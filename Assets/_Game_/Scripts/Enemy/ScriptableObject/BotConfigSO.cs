using UnityEngine;

[CreateAssetMenu(fileName = "BotConfigSO", menuName = "ScriptableObjects/BotConfig")]
public class BotConfigSO : ScriptableObject
{
    public int health;
    public int damage;
    public int armor;
    public bool isImportant; // Indicates if the bot is important
}