using System;
using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

[Serializable]
public class PlayerDeadEvent : IGameEvent
{
    public float Timestamp { get; private set; }
    public GameObject Player { get; private set; }

    public PlayerDeadEvent(GameObject player)
    {
        Timestamp = Time.time;
        Player = player;
    }
}