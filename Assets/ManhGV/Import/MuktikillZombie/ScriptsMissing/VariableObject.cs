using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class VariableObject<T> : ScriptableObject
{
    public string DeveloperDescription = "";
    public DebugType DebugMode;
    [SerializeField] protected T _value;
    public readonly List<UnityAction> Listeners = new List<UnityAction>();

    protected void OnValidate()
    {
        InvokeListeners();
    }

    public virtual T Value
    {
        get
        {
#if DEVELOPER_MODE
            if (DebugMode.HasFlag(DebugType.Get))
                Debug.Log($"{name} is get");
#endif
            return _value;
        }


        set
        {
#if DEVELOPER_MODE
            if (DebugMode.HasFlag(DebugType.Set))
                Debug.Log($"{name} is set");
#endif
            _value = value;
            InvokeListeners();
        }
    }

    protected void InvokeListeners()
    {
        for (var i = Listeners.Count - 1; i >= 0; i--)
            Listeners[i].Invoke();
    }

    public void AddListener(UnityAction act)
    {
        Listeners.Add(act);
    }

    public void RemoveListener(UnityAction act)
    {
        if (Listeners.Contains(act))
        {
            Listeners.Remove(act);
        }
    }

    public void Reset()
    {
        Listeners.Clear();
    }

    public static implicit operator T(VariableObject<T> reference)
    {
        return reference.Value;
    }

    [Flags]
    public enum DebugType
    {
        None = 0,
        Get = 1,
        Set = 1 << 2
    }
}