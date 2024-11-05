using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseManager : MonoBehaviour
{
    [SerializeField] private List<HousePoint> housePoints;  // Danh sách các điểm để spawn bot
    //[SerializeField] private BotType _botType;        // Prefab của bot
    public static HouseManager instance;

    private void Awake()
    {
        instance = this;
    }
    public HousePoint GetHousePoint(BotType botType)
    {
        return housePoints.Find(point => point.BotType == botType);
    }

}

[Serializable]
public class HousePoint
{
    [SerializeField] List<Transform> housePoints;
    [SerializeField] Transform spawnPoint;
    [SerializeField] private Transform destinationsPoint;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private BotType botType;
    public List<Transform> HousePoints => housePoints;
    public Transform SpawnPoint => spawnPoint;
    public Transform DestinationsPoint => destinationsPoint;
    public Transform AttackPoint => attackPoint;
    public BotType BotType => botType;
}