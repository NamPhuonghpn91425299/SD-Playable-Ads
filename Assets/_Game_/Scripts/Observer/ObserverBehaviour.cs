using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets._Develop_.ThanhNT.Scripts.Observer
{
    public abstract class ObserverBehaviour<T> : MonoBehaviour, IObserver<T> where T : IGameEvent
    {
        protected IEventManager eventManager;

        protected virtual void Start()
        {
            if (eventManager != null)
            {
                eventManager.Subscribe<T>(this);
            }
            else
            {
                Debug.LogError($"EventManager not injected for {GetType().Name}");
            }
        }

        protected virtual void OnDestroy()
        {
            eventManager?.Unsubscribe<T>(this);
        }

        public abstract void OnNotify(T data);
    }

}
