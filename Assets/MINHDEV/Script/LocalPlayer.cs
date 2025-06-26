using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    public static LocalPlayer Instance;
    public Transform  _localPlayer;
    public Transform _posExplosion;

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
    public Transform GetTransformExplosion()
    {
        return _posExplosion;
    }
}
