using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechAnimEvent : MonoBehaviour
{
    public ParticleSystem[] OnLandingEffect;
    public ParticleSystem[] WalkEffect;
    public bool IsInAttack;
    public void OnWalkRight()
    {
        Debug.Log("OnWalkRight");
        WalkEffect[0].Play();
        WalkEffect[1].Stop();
    }    

    public void OnWalkLeft()
    {
        Debug.Log("OnWalkLeft");
        WalkEffect[1].Play();
        WalkEffect[0].Stop();
    }    

    public void OnLanding()
    {
        foreach (var Effect in OnLandingEffect) 
        {
            Effect.Play();
        }
    }    

    public void InAttack()
    {
        IsInAttack = true;
    }    
}
