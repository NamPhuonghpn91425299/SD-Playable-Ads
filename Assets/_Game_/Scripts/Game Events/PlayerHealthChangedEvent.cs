using System;
using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

[Serializable]
public class PlayerHealthChangedEvent : IGameEvent
{
    public float Timestamp { get; private set; }
    public int? CurrentHealth { get; private set; }
    public int? MaxHealth { get; private set; }
    public int? Damage { get; private set; }
    public string State { get; private set; }
    public PlayerHealthChangedEvent(int? maxHealth = null, int? currentHealth = null, int? damage = null, string state = "OnlyDamage")
    {
        Timestamp = Time.time;
        MaxHealth = maxHealth;
        CurrentHealth = currentHealth;
        Damage = damage;
        State = state;
        if (state == "Heal")
        {
            Debug.Log($"Player healed: {damage} HP. Current health: {currentHealth}/{maxHealth}");
            
        }
        
    }
    

   
}