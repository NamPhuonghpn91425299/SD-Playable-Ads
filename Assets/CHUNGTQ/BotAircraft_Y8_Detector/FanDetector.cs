using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanDetector : MonoBehaviour
{
    [SerializeField] private int hpFan = 150;
    [SerializeField] private GameObject model_fan;
    [SerializeField] private GameObject exlosion;
    [SerializeField] private GameObject fan_detector;
    [SerializeField] private AudioSource _sourceAudio;
    private bool onDead;
    public bool IsDead => hpFan <= 0;
    public int RemainHealth => hpFan;

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
