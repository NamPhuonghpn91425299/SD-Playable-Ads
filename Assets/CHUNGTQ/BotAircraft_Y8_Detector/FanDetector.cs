using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanDetector : MonoBehaviour
{
    [SerializeField] private WeaknessHealthBar healthBar;
    [SerializeField] private int hpFan;
    [SerializeField] private BotNetwork _bot;
    [SerializeField] private GameObject model_fan;
    [SerializeField] private GameObject exlosion;
    [SerializeField] private GameObject fan_detector;
    [SerializeField] private AudioSource _sourceAudio;
    private bool onDead;
    public bool IsDead => hpFan <= 0;
    public int RemainHealth => hpFan;

    private void Start()
    {
        hpFan = (int)_bot.BotConfigSO.WeaknessHealth;
    }
    public void Initialize(float maxHealth)
    {
        hpFan = (int)maxHealth;
        onDead = false;
        healthBar.Initialize(maxHealth);
    }
    public void Init(int health)
    {
        hpFan = health;
        model_fan.SetActive(true);
        exlosion.SetActive(false);
        fan_detector.SetActive(true);
    }    
    public bool TryHandleDamage(int damage)
    {
        if (onDead) return false;
        hpFan = Mathf.Max(0,hpFan - damage);
        healthBar.UpdateHealth(hpFan);
        if (hpFan <= 0)
        {
            onDead = true;
            Dead();
            return false;
        }

        return true;
    }

    void Dead()
    {
        _sourceAudio.Play();
        model_fan.SetActive(false);
        exlosion.SetActive(true);
        fan_detector.SetActive(false);

    }   
}
