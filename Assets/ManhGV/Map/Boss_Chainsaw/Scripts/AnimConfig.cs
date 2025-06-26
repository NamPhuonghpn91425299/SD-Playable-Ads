using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AnimConfig", menuName = "ScriptableObjects/AnimConfig")]
public class AnimConfig : ScriptableObject
{
    public List<AnimStruct> anims = new List<AnimStruct>();
}

[Serializable]
public struct AnimStruct
{
    public int style;
    public float timerTakeDamage;
    public float timerEndAnim;
    
}