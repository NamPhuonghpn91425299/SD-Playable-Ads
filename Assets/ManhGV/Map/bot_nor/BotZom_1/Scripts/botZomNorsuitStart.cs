using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class botZomNorsuitStart : BaseState<botZomState>
{
    [SerializeField] private HumanMoveBase humanMoveBase;
    [SerializeField] private BotNetwork botNet;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip;
    [SerializeField] bool CanPlayAudio;
    [SerializeField] private float timeDelay;
    [SerializeField] private float delayTimePlayAudio = 1f;
    private float _timer;
    private bool _isActive = false;
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
        //Invoke(nameof(playSound), delayTimePlayAudio);
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
            audioSource.Play();
        }
    }
    
    public override void UpdateState()
    {
        _timer += Time.deltaTime;
        if (_timer >= delayTimePlayAudio && !_isActive)
        {
            _isActive = true;
            playSound();
            _timer = 0f;
            
        }
    }
    public override void ExitState()
    {
        if(delaystart!=null)
            StopCoroutine(delaystart);
    }
    public override botZomState GetNextState()
    {
        if (botNet.DeadExplosion)
            return botZomState.DeadExplosion;
        else
        {
            if(botNet.IsDead)
            {
                return botZomState.Dead;
            }
            else
            {
                if (isStartDone)
                {
                    return botZomState.Move;
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