using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

[CreateAssetMenu(fileName = "BotConfigSO", menuName = "ScriptableObjects/BotConfig")]
public class BotConfigSO : ScriptableObject
{
    public int id;
    public string BotName;
    public int health;
    public float HealthThreshold; // ngưỡng máu để kích hoạt bất tử, scale damage
    public float WeaknessHealth;
    public bool isCanImmortal;
    public int attackDuration;
    public float damage;
    public float fireRate;
    public float timeAttack;
    public float timeReload;
    public BotType botType;
    public GameObject Model;
    public float moveSpeed;
    [Header("Tank Attributes")]
    public float rocketSpeed;
    public float targetRotation;
    public float turretRotation;
    public float wheelSpeed;

    public List<DamageScale> damageScales;
 

    [SerializeField]public CarryAttributes[] carryAttributes;

    public float GetDamageScale(DamageType DamageType) =>  damageScales.Find(e  => e.DamageType == DamageType)?.ScaleValue ?? 1;

}

[Serializable]
public class CarryAttributes
{
    public BotConfigSO botConfig;
    public int Quantity;
}

[System.Serializable]
public class  DamageScale
{
    public DamageType DamageType;
    [Range(0, 5)]
    public float ScaleValue;
}