using Luna.Unity.FacebookInstantGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardNetwork : MonoBehaviour, IReward
{
    [SerializeField] int _maxHealth = 100;
    [SerializeField] List<Transform> _fireAssistCheckPos = new List<Transform>();
    [SerializeField] SpawnRewardPoint _path;
    [SerializeField] int _currentHealth;
    [SerializeField] private bool isCollected;
    public bool IsCollected => isCollected;
    public Action<int> OnTakeDamage { get; set; }
    public Action OnRewardCollected { get; set; }
    public SpawnRewardPoint Path => _path;
    public List<Transform> FireAssistCheckPos => _fireAssistCheckPos;

    private void Awake()
    {
        OnRewardCollected += Collect;
        OnTakeDamage += CalculateHealth;
        _currentHealth = _maxHealth;
    }

    public void TakeCollect(int damage)
    {
        if (isCollected) return;
        OnTakeDamage.Invoke(damage);
    }

    public void CalculateHealth(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            isCollected = true;
            OnRewardCollected.Invoke();
        }
    }

    public void Collect()
    {
        isCollected = true;
    }

    public void SetPath(SpawnRewardPoint path)
    {
        _path = path;
    }
}
