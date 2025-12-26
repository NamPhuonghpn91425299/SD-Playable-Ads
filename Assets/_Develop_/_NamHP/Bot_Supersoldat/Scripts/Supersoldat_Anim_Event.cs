using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Supersoldat_Anim_Event : MonoBehaviour
{
    public GameObject Step2Effect;
    public GameObject Step2Expolosion;
    public Default_Dead default_Dead;
    
    private void OnEnable()
    {
        Step2Effect.SetActive(false);
        Step2Expolosion.SetActive(false);
    }
    public void DropWeaponOnDead()
    {
        
    }

    public void DeadStep2_On()
    {
        Step2Effect.SetActive(true);
    }
    public void FinalStep()
    {
        Step2Expolosion.SetActive(true);
    }
    public void DeadCrown_On()
    {
        default_Dead.AnimationFinishTrigger();
    }
}
