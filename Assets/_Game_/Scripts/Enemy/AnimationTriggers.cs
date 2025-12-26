using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTriggers : MonoBehaviour
{
    [SerializeField] private StateControllerBase _stateController;
    public void AnimationTrigger() => _stateController._currentState.AnimationFinishTrigger();
    public void TriggerCenterAnimation() => _stateController._currentState.TriggerCenterAnimation();
}
