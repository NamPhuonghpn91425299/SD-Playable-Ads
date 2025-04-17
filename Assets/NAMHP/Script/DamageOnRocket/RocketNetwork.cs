using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketNetwork : MonoBehaviour,ITakeDamage
{
    [SerializeField] private float _currentRocketHealth;
    [SerializeField] private float _maxRocketHealth = 40f;
    public Action<int> OnTakeDamageRocket { get; set; }
    public Action<bool> OnRocketExplosion { get; set; }
    private bool _isRocketExplosion = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        gameObject.SetActive(true);
        _currentRocketHealth = _maxRocketHealth;
        OnTakeDamageRocket += CalculateHealth;
    }

    private void OnDisable()
    {
        OnTakeDamageRocket -= CalculateHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        int damage = damageInfo.damage;
        OnTakeDamageRocket?.Invoke(damage);
        Debug.Log($"Rocket Health: {damage}");
    }
    public void CalculateHealth(int damage)
    {
        _currentRocketHealth -= damage;
        if (_currentRocketHealth <= 0)
        {
            _isRocketExplosion = true;
            OnRocketExplosion?.Invoke(_isRocketExplosion);
        }
    }
}

