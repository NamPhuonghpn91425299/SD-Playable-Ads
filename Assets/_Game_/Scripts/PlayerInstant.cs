using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerInstant : MonoBehaviour
{
    public static PlayerInstant Instance { get; private set; }
    private Transform tf;

    public Transform explosionPos;
    public Transform TF
    {
        get
        {
            if (tf == null)
                tf = transform;
            return tf;
        }
    }
    public Transform ExplosionPos
    {
        get
        {
            if (explosionPos == null)
            {
                return TF;
            }
            return explosionPos;
        }
    }
    
    private void Awake()
    {
        tf = transform;
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Initialization code here
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
