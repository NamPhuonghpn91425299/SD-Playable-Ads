using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Weapon : MonoBehaviour
{
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public Animation animation;
    [SerializeField] public Transform spawmBulletPoint;
    [SerializeField] public ParticleSystem[] shootEffects;

    public Dictionary<string, AnimationData> animationDatas = new Dictionary<string, AnimationData>();
    
     public void PlayEffect()
    {
        foreach (var effect in shootEffects)
        {
            effect.Play();
        }
    }

    public void SetUp(List<AnimationData> animationData)
    {
        foreach(var animation in animationData)
        {
            animationDatas.Add(animation.keyName, animation);
            if (animation.clip == null) continue;
            this.animation.AddClip(animation.clip, animation.keyName);
        }
    }

    public void PlayAnimation(string clipname, bool isLoopAudio = false)
    {
        animation.Play(clipname);
        animation[clipname].speed = 2;
        if(animationDatas.TryGetValue(clipname, out var data))
        {
            audioSource.clip = data.audio;
            audioSource.loop = isLoopAudio;
            audioSource.Play();
        }
    }

    public void CrossFadeAnimation(string clipname, float fadeLength)
    {
        //animation.CrossFade(clipname, fadeLength);
        if(animationDatas.TryGetValue(clipname, out var data))
        {
            audioSource.PlayOneShot(data.audio);
            animation.clip = data.clip;
            animation.Play();
        }
    }
    
    public void PlayShoot()
    {
        //animation.CrossFade("Fire", 1f);
        animation["Fire"].speed = 2;
        if (animationDatas.TryGetValue("Fire", out var data))
        {
            audioSource.clip = data.audio;
            audioSource.loop = true;
            audioSource.Play();
        }
    }   
    public void WaitPlayShoot()
    {
        animation.CrossFade("Fire", 1f);
        animation["Fire"].speed = 1.5f;

        if (animationDatas.TryGetValue("FireIn", out var data))
        {
            audioSource.clip = data.audio;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    public void StopPlayShoot()
    {
        animation.CrossFade("Idle",0.8f);
        if (animationDatas.TryGetValue("FireOut", out var data))
        {
            audioSource.clip = data.audio;
            audioSource.volume = 0.5f;
            audioSource.loop = false;
            audioSource.Play();
        }
    }

}
