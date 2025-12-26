using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootUIVfx : VFXBase
{
    [SerializeField] private Animator _animator_aim_normal;
    [SerializeField] private Animator _animator_aim_take_damage;
    
    private Coroutine _currentCoroutine;

    public override void Play<T>(T parameter)
    {
      
        if (parameter.ToString() == "HitEnemy")
        {
            _currentCoroutine = StartCoroutine(IEPlayShootVFX(_animator_aim_take_damage, "CrossShot"));
        }
        
   
         _currentCoroutine = StartCoroutine(IEPlayShootVFX(_animator_aim_normal, "Cross"));
        

    }

    private IEnumerator IEPlayShootVFX(Animator animator, string animationName)
    {
        animator.Play(animationName, 0, 0f); // layer 0, normalized time 0
        yield return HelperCoroutine.GetWait(animator.GetCurrentAnimatorStateInfo(0).length);
        _currentCoroutine = null;
    }
}
