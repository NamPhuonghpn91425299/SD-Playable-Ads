using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VFXBase : MonoBehaviour
{
    public abstract void Play();
    public abstract void Stop();
    public abstract void SetActive(bool active);

    public virtual void UnactiveAim()
    {

    }
    
    public virtual void ActiveAim()
    {

    }
}
