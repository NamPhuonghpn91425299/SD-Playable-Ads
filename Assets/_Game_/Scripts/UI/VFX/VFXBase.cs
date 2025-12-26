using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXBase : MonoBehaviour, IVfx
{
    public virtual void Init<T>(T parameter)
    {
        // Default initialization behavior
    }
    public virtual float GetSpeed()
    {
        return 1f; // Default speed
    }

    public virtual float GetTime()
    {
        return 0f; // Default time
    }

    public virtual bool IsLooping()
    {
        return false; // Default looping
    }

    public virtual bool IsPaused()
    {
        return false; // Default paused
    }

    public virtual bool IsPlaying()
    {
        return false; // Default playing
    }

    public virtual void Pause<T>(T parameter)
    {
    
    }


    public virtual void Play<T>(T parameter)
    {
    }

    public virtual void Resume<T>(T parameter)
    {
    }
    

    public virtual void SetLoop(bool loop)
    {
        // Default set loop behavior
    }

    public virtual void SetSpeed(float speed)
    {
        // Default set speed behavior
    }

    public virtual void SetTime(float time)
    {
        // Default set time behavior
    }

    public virtual void Stop<T>(T parameter)
    {
        
    }
}

    