using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bot_Audio : AudioPlayable
{
    [SerializeField] private SoundSource _audioSourceIdle;
    [SerializeField] private SoundSource _audioSourceDeath;
    [SerializeField] private SoundSource _audioSourceAttack;
    [SerializeField] private SoundSource _audioSourceGetHit;
    [SerializeField] private float _audioCanHearDistance = 50f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _audioSourceIdle.audioSource ??= GetComponent<AudioSource>();
        _audioSourceDeath.audioSource ??= GetComponent<AudioSource>();
        _audioSourceAttack.audioSource ??= GetComponent<AudioSource>();
        _audioSourceGetHit.audioSource ??= GetComponent<AudioSource>();
    }
#endif
    
    void OnEnable()
    {
        if(_audioSourceIdle.audioSource != null)
            _audioSourceIdle.PlayByIndexWithLoop(0);
        
    }

    private void Update()
    {
        if (GameController.Instance.CurrentGameState != GameConstants.GameState.InGame)
        {
            StopAllAudio();
            return;
        }
        SetVolumeByDistance(_audioSourceIdle);
        SetVolumeByDistance(_audioSourceDeath);
        SetVolumeByDistance(_audioSourceAttack);
        SetVolumeByDistance(_audioSourceGetHit);

        

    }

    private void SetVolumeByDistance(SoundSource soundSource)
    {
        if (soundSource.isCanHearByDistance)
        {
            if (soundSource.audioSource.volume <= soundSource.volume)
            {
                float distance = Vector3.Distance(transform.position, PlayerInstant.Instance.transform.position);
                float normalizedDistance = distance / _audioCanHearDistance;
                soundSource.audioSource.volume = Mathf.Clamp01(1f - normalizedDistance) * soundSource.volume;
            }
        }
    }

    private void PlayDeathSound()
    {
        if (_audioSourceDeath.audioSource == null)
            return;
        _audioSourceDeath.PlayOneShotByIndex(Random.Range(0, _audioSourceDeath.soundClips.Length));
    }

    public override void PlayAudioIndexLoop(GameConstants.AudioType audioType, int index,bool _Loop = true)
    {
        if (audioType == GameConstants.AudioType.BotDeath)
        {
            if (_audioSourceDeath.audioSource == null)
                return;
            if(_Loop)
                _audioSourceDeath.PlayByIndexWithLoop(index);
            else
                _audioSourceDeath.PlayByIndex(index);
        }
        else if (audioType == GameConstants.AudioType.BotAttack)
        {
            if(_audioSourceAttack.audioSource == null)
                return;
            if(_Loop)
                _audioSourceAttack.PlayByIndexWithLoop(index);
            else
                _audioSourceAttack.PlayByIndex(index);
        }
        else if (audioType == GameConstants.AudioType.GetHit)
        {
            if(_audioSourceGetHit.audioSource == null)
                return;
            if(_Loop)
                _audioSourceGetHit.PlayByIndexWithLoop(index);
            else
                _audioSourceGetHit.PlayByIndex(index);
        }
    }

    private void PlayAttackSound()
    {
        if(_audioSourceAttack.audioSource == null)
            return;
        _audioSourceAttack.PlayOneShotByIndex(Random.Range(0, _audioSourceAttack.soundClips.Length));
    }

    private void PlayGetHitSound()
    {
        if (_audioSourceGetHit.audioSource == null)
            return;
        _audioSourceGetHit.PlayOneShotByIndex(Random.Range(0, _audioSourceGetHit.soundClips.Length));
    }

    private void OnEnableAudioAttack(bool isEnable)
    {
        if (_audioSourceAttack.audioSource.clip == null)
        {
            _audioSourceAttack.audioSource.clip = _audioSourceAttack.soundClips[0];
            _audioSourceAttack.audioSource.loop = true;
        }
        _audioSourceAttack.audioSource.enabled = isEnable;

    }

    public override void PlayAudio(GameConstants.AudioType audioType)
    {
        if (audioType == GameConstants.AudioType.BotDeath)
        {
            PlayDeathSound();
        }
        else if (audioType == GameConstants.AudioType.BotAttack)
        {
            PlayAttackSound();
        }
        else if (audioType == GameConstants.AudioType.GetHit)
        {
            PlayGetHitSound();
        }
    }

    public override void OnlyEnableAudio(GameConstants.AudioType audioType, bool isEnable)
    {
        if (audioType == GameConstants.AudioType.BotDeath)
        {
           
        }
        else if (audioType == GameConstants.AudioType.BotAttack)
        {
            OnEnableAudioAttack(isEnable);
        }
        else if (audioType == GameConstants.AudioType.GetHit)
        {
            
        }

    }

    public override void StopAllAudio()
    {
        if (_audioSourceDeath.audioSource != null)
            _audioSourceDeath.audioSource.enabled = false;
        if (_audioSourceAttack.audioSource != null)
            _audioSourceAttack.audioSource.enabled = false;
        if (_audioSourceGetHit.audioSource != null)
            _audioSourceGetHit.audioSource.enabled = false;
        if (_audioSourceIdle.audioSource != null)
            _audioSourceIdle.audioSource.enabled = false;

    }

    public override void StopAllAudioDontEnbleFalse()
    {
        if (_audioSourceDeath.audioSource != null)
            _audioSourceDeath.audioSource.Stop();
        if (_audioSourceAttack.audioSource != null)
            _audioSourceAttack.audioSource.Stop();
        if (_audioSourceGetHit.audioSource != null)
            _audioSourceGetHit.audioSource.Stop();
        if (_audioSourceIdle.audioSource != null)
            _audioSourceIdle.audioSource.Stop();
    }
}
