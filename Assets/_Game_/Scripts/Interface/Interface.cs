using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Develop_.ThanhNT.Scripts.Observer
{
    // Interface cơ bản cho observer
    public interface IObserver<T>
    {
        void OnNotify(T data);
    }

    /// <summary>
    /// Interface cho các đối tượng có thể quan sát
    /// </summary>
    public interface ISubject<T>
    {
        void Subscribe(IObserver<T> observer);
        void Unsubscribe(IObserver<T> observer);
        void NotifyObservers(T data);
    }

    // Interface cho Event Manager
    public interface IEventManager
    {
        void Subscribe<T>(IObserver<T> observer) where T : IGameEvent;
        void Unsubscribe<T>(IObserver<T> observer) where T : IGameEvent;
        void Publish<T>(T gameEvent) where T : IGameEvent;

    }

    // Base interface cho tất cả game events
    public interface IGameEvent
    {
        float Timestamp { get; }
    }

    // Base interface for subject operations
    public interface ISubjectBase
    {
        void Clear();
    }


}

public abstract class AudioPlayable : MonoBehaviour
{
    public abstract void PlayAudio(GameConstants.AudioType audioType);

    public abstract void PlayAudioIndexLoop(GameConstants.AudioType audioType, int index,bool _Loop = true);
    public abstract void OnlyEnableAudio(GameConstants.AudioType audioType, bool isEnable);
    public abstract void StopAllAudio();
    public abstract void StopAllAudioDontEnbleFalse();
}