using static GameConstants;
using UnityEngine;

[CreateAssetMenu(fileName = "MissileSO", menuName = "ScriptableObjects/MissileSO", order = 2)]
public class MissileSO : ScriptableObject
{
    public Missile_Player missileType;
    public AudioClip audioFire;
    
    public AnimationClip Idle;
    public AnimationClip Fire;
    public AnimationClip Reload;
    
    public float timeReload = 2f;
    public int AmountRocket = 5;
    public bool isFollow = false;
}
