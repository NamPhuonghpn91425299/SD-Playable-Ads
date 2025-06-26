using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEvent : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip audioClip;
    public Transform aoe1;
    public Transform aoe2;
    public Transform aoe3;
    public Transform aoe4;
    public ParticleSystem explosionEffect;
    public Action PlayAudioClip;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;

    }

    private void OnEnable()
    {
        PlayAudioClip += PlayAudio;
        
    }

    private void OnDisable()
    {
        PlayAudioClip -= PlayAudio;
    }

    private void PlayAudio()
    {
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }


    public void TriggerExplosionAoe1()
    {
        if (explosionEffect != null)
        {
            explosionEffect.transform.position = aoe1.position;
            explosionEffect.Play();
            PlayAudioClip?.Invoke();
        }
    }
    public void TriggerExplosionAoe2()
    {
        if (explosionEffect != null)
        {
            explosionEffect.transform.position = aoe2.position;
            explosionEffect.Play();
            PlayAudioClip?.Invoke();
        }
    }
    public void TriggerExplosionAoe3()
    {
        if (explosionEffect != null)
        {
            explosionEffect.transform.position = aoe3.position;
            explosionEffect.Play();
            PlayAudioClip?.Invoke();
        }
    }
    public void TriggerExplosionAoe4()
    {
        if (explosionEffect != null)
        {
            explosionEffect.transform.position = aoe4.position;
            explosionEffect.Play();
            PlayAudioClip?.Invoke();
        }
    }
}
