using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotBigHandStart : BaseState<BigHandState>
{
    [SerializeField] private HumanMoveBase humanMoveBase;
    [SerializeField] private BotNetwork botNet;
    [SerializeField] private float timeDelay;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip;
    [SerializeField] bool CanPlayAudio;
    public Animator anim;
    public bool changeAnimEqualBool;
    public bool batTuKhiStart;
    Coroutine delaystart;

    private bool isStartDone;
    public override void EnterState()
    {
        isStartDone = false;
        botNet.BatTu = batTuKhiStart;
        delaystart = StartCoroutine(IEDelayStartDone(timeDelay));
        Invoke(nameof(playSound), 0.1f);
    }
    
    IEnumerator IEDelayStartDone(float time)
    {
        yield return new WaitForSeconds(time);
        isStartDone = true;
        botNet.BatTu = false; 
        
        if(changeAnimEqualBool)
            anim.SetBool("DoneStart", true);
    }

    public void playSound()
    {
        if (CanPlayAudio)
        {
            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    public override void UpdateState()
    {
        
    }
    public override void ExitState()
    {
        if(delaystart!=null)
            StopCoroutine(delaystart);
    }
    public override BigHandState GetNextState()
    {
        if (botNet.DeadExplosion)
            return BigHandState.DeadExplosion;
        else
        {
            if(botNet.IsDead)
            {
                return BigHandState.Dead;
            }
            else
            {
                if (isStartDone)
                {
                    return BigHandState.Move;
                }
                else {
                    return StateKey;
                }

            }
        }
      
    }

    public void OntakeDame()
    {
        if (CanPlayAudio)
        {
            audioSource.clip = null;
            audioSource.loop = false;
            audioSource.Stop();
        }
        isStartDone = true;
        if(changeAnimEqualBool)
            anim.SetBool("DoneStart", true);
    }
}