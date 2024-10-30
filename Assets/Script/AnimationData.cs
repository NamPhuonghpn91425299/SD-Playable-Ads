using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct AnimationData 
{
   public string keyName;
   public AnimationClip clip;
   public AudioClip audio;
    
    public AnimationData(string key, AnimationClip clip, AudioClip audio)
    {
        this.keyName = key;
        this.clip = clip;
        this.audio = audio;
    }
}

enum EAnimation
{

}
