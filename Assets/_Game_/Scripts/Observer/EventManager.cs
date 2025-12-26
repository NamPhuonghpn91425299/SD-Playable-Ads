using System;
using System.Collections.Generic;

using UnityEngine;

namespace Assets._Develop_.ThanhNT.Scripts.Observer
{
    public class EventManager : MonoBehaviour, IEventManager
    {
        private readonly Dictionary<Type, object> subjects = new Dictionary<Type, object>();
        public static EventManager Instance { get; private set; }

        private void Awake()
        {
            // Set this instance as the singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Optional: Keep across scenes
//                Debug.Log("EventManager Instance initialized");
            }
            else if (Instance != this)
            {
                // If another instance already exists, destroy this one
                Debug.LogWarning("Multiple EventManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
        }

        public void Subscribe<T>(IObserver<T> observer) where T : IGameEvent
        {
            var subject = GetOrCreateSubject<T>();
            subject.Subscribe(observer);
        }

        public void Unsubscribe<T>(IObserver<T> observer) where T : IGameEvent
        {
            var subject = GetOrCreateSubject<T>();
            subject.Unsubscribe(observer);
        }

        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            var subject = GetOrCreateSubject<T>();
            subject.NotifyObservers(gameEvent);
        }

        private Subject<T> GetOrCreateSubject<T>() where T : IGameEvent
        {
            var eventType = typeof(T);

            if (!subjects.TryGetValue(eventType, out var subject))
            {
                subject = new Subject<T>();
                subjects[eventType] = subject;
            }

            return (Subject<T>)subject;
        }

        private void OnDestroy()
        {
            // Clear the instance reference if this is the current instance
            if (EventManager.Instance == this)
            {
                EventManager.Instance = null;
                Debug.Log("EventManager Instance cleared");
            }

            // Clear all subjects when EventManager is destroyed
            foreach (var subject in subjects.Values)
            {
                if (subject is Subject<IGameEvent> gameEventSubject)
                {
                    gameEventSubject.Clear();
                }
            }
            subjects.Clear();
        }
    }
}