using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace AnimatorBehaviours
{
    public class AnimatorBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private EventBehaviour[] _eventBehaviours;

        private Dictionary<string, UnityEvent> _queryEventBehaviours;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            _animator ??= GetComponentInChildren<Animator>();
        }
#endif

        private void Awake()
        {
            _queryEventBehaviours = _eventBehaviours.ToDictionary(e => e.Name, e => e.Event);
        }

        private void OnEnable()
        {
            if (_queryEventBehaviours.Count > 0)
            {
                var behaviours = _animator.GetBehaviours<AnimatorEventStateBehaviour>();
                foreach (var behaviour in behaviours.SelectMany(e => e.EventTriggerds))
                {
                    if (!_queryEventBehaviours.TryGetValue(behaviour.EventName, out var result)) continue;
                    
                    behaviour.EventTrigger = result;
                }
            }
        }

        //public void DebugEvent(string eventName) => Debug.LogError("Test Animator StateBehaviour: " + eventName);
    }

    [System.Serializable]
    public class EventBehaviour
    {
        public string     Name;
        public UnityEvent Event;
    }
}
