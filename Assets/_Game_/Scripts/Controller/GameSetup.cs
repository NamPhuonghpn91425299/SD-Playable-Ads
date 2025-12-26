using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
    public static GameSetup Instance { get; private set; }
    [SerializeField] private MonoBehaviour eventManager;

    public MonoBehaviour EventManager => eventManager;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}