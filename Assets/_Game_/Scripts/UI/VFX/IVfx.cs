using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVfx
{
    void Play<T>(T parameter);
    void Stop<T>(T parameter);
    void Pause<T>(T parameter);
    void Resume<T>(T parameter);
    void SetTime(float time);
    float GetTime();
    bool IsPlaying();
    bool IsPaused();
    void SetSpeed(float speed);
    float GetSpeed();
    void SetLoop(bool loop);
    bool IsLooping();

}
