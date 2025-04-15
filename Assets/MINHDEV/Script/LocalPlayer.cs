using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    public static LocalPlayer Instance;
    public Transform  _localPlayer;
    public Transform _posExplosion;
    public Transform centerTower;
    public Transform centerYTower;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetLocalPlayer()
    {
        return _localPlayer.position;
    }
    public Transform GetTranformPlayer()
    {
        return _localPlayer;
    } 
    public Transform GetTranExplosion()
    {
        return _posExplosion;
    }    
    public Transform GetTranCenter()
    {
        return centerTower;
    }
    public Transform GetTranCenterY()
    {
        return centerYTower;
    }
}
