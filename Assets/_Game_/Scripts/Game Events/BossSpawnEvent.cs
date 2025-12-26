using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

[System.Serializable]
public class BossSpawnEvent : IGameEvent
{
    public Vector3 SpawnPosition;
    public GameObject BossPrefab;
    public float Timestamp => Time.time;

    public bool Trigger { get; set; }

    public BossSpawnEvent(Vector3 spawnPosition, GameObject bossPrefab)
    {
        SpawnPosition = spawnPosition;
        BossPrefab = bossPrefab;
    }

    public BossSpawnEvent(bool trigger)
    {
        this.Trigger = trigger;
    }





}
