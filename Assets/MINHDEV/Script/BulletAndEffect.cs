using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BulletAndEffect", menuName = "ScriptableObjects/BulletAndEffect", order = 1)]
public class BulletAndEffect : ScriptableObject
{
    public GameObject[] _bulletPrefab;
    public GameObject[] EffectBullet;
}
