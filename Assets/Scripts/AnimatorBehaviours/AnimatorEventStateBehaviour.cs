using System.Collections.Generic;
using UnityEngine;

namespace AnimatorBehaviours
{
    public class AnimatorEventStateBehaviour : CustomStateMachineBehaviour
    {
        public List<EventTriggerd> EventTriggerds = new List<EventTriggerd>();
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            EventTriggerds.ForEach(e => e.Setup());
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.loop)
                EventTriggerds.ForEach(e => e.EvaluteLoop(animator, stateInfo.normalizedTime));
            else
                EventTriggerds.ForEach(e => e.Evalute(animator, stateInfo.normalizedTime));
        }
    }
}