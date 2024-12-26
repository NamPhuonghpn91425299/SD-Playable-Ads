using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AnimatorBehaviours
{
    [System.Serializable]
    public class EventTriggerd
    {
        public string      EventName;
        [Range(0, 1f)]
        public List<float> TriggerTimes;
        
        [HideInInspector]
        public UnityEvent EventTrigger;

        private int  _triggerIdx;
        private bool _isTriggerDone => _triggerIdx >= TriggerTimes.Count;

        private int _loopTimes = 0;

        public void Setup()
        {
            _triggerIdx = 0;
            _loopTimes  = 0;
            TriggerTimes.Sort();
        }

        public void Evalute(Animator target, float normalizedTime)
        {
            if (_isTriggerDone) return;
            TryInvoke(normalizedTime);
        }
        
        public void EvaluteLoop(Animator target, float normalizedTime)
        {
            if (_isTriggerDone)
            {
                if (_loopTimes < (int)normalizedTime)
                {
                    _loopTimes++;
                    _triggerIdx = 0;
                }
                return;
            }
            
            TryInvoke(normalizedTime % 1);
        }

        private void TryInvoke(float normalizedTime)
        {
            if (normalizedTime >= TriggerTimes[_triggerIdx])
            {
                EventTrigger?.Invoke();
                _triggerIdx++;
            }
        }
    }
}
