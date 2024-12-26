using System;
using UnityEngine;

public abstract class BaseState<EState> : MonoBehaviour where EState : Enum
{
    public EState StateKey { get; private set; }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract EState GetNextState();
    public void Initialize(EState state)
    {
        this.StateKey = state;
    }
}
