using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

[System.Serializable]
public class GameDataChangedEvent : IGameEvent
{
    public float Timestamp => Time.time;
    public int? Bullet { get; }
    public int? Rocket { get; }
    public int? Enemy { get; }
    public string CurrentRound { get; }
    public int? EmemyRemaning { get; }
    public int? BulletRemaning { get; }
    public int? RocketRemaning { get; }
    public float? ReloadTime { get; }
    public string hitEnemy { get; }
    public float? Fov { get; }
    public bool isInfinityBullet = false;

    public GameDataChangedEvent(string currentRound = "", int? ememyRemaning = null, int? bulletRemaning = null, int? rocketRemaning = null, int? bullet = null, int? rocket = null, int? enemy = null, float? reloadTime = null, string hitEnemy = null, float? fov = null, bool isInfinityBullet = false)
    {
        this.EmemyRemaning = ememyRemaning;
        this.BulletRemaning = bulletRemaning;
        this.RocketRemaning = rocketRemaning;
        this.Bullet = bullet;
        this.Rocket = rocket;
        this.Enemy = enemy;
        this.ReloadTime = reloadTime;
        this.hitEnemy = hitEnemy;
        this.Fov = fov;
        this.CurrentRound = currentRound;
        this.isInfinityBullet = isInfinityBullet;
    }
}