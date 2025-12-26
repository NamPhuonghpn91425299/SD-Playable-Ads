using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using static GameConstants;


public class PlayerController : MonoBehaviour, IObserver<PlayerHealthChangedEvent>

{
    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private bool isImmortal = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Flag to prevent multiple GameOver events
    private bool hasTriggeredGameOver = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;
    void Start()
    {
        currentHealth = maxHealth;
        EventManager.Instance?.Publish(new PlayerHealthChangedEvent(maxHealth: maxHealth, state: "Initialized"));
        EventManager.Instance?.Subscribe<PlayerHealthChangedEvent>(this);
    }
    void OnDestroy()
    {
        EventManager.Instance?.Unsubscribe<PlayerHealthChangedEvent>(this);
    }

    public void OnNotify(PlayerHealthChangedEvent data)
    {
        if (!hasTriggeredGameOver)
        {
            if (isImmortal)
            {
                if (data.State == "Heal")
                {
                    currentHealth += data.Damage ?? 0;
                    if (currentHealth > maxHealth)
                    {
                        currentHealth = maxHealth;
                    }
//                    Debug.Log($"Player healed: {data.Damage}. Current health: {currentHealth}/{maxHealth}");
                    EventManager.Instance?.Publish(new PlayerHealthChangedEvent(maxHealth: maxHealth, currentHealth: currentHealth, state: "Healed"));
                }
//                Debug.LogWarning("Player is immortal, health changes will not be applied.");
                return;
            }
            else
            {
                if (data.State == "OnlyDamage")
                {
                    currentHealth -= data.Damage ?? 0;
//                    Debug.Log($"Player took damage: {data.Damage}. Current health: {currentHealth}/{maxHealth}");
                    if (currentHealth <= 0)
                    {
                        currentHealth = 0;
//                        Debug.Log("Player has died.");
                        hasTriggeredGameOver = true;
                        EventManager.Instance?.Publish(new GameStateChangedEvent(GameState.GameOver));
                    }
                    EventManager.Instance?.Publish(new PlayerHealthChangedEvent(maxHealth: maxHealth, currentHealth: currentHealth, state: "Damaged"));
                }

            }

        }


    }

}