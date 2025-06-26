using System;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class VariableReference<T>
{
    [SerializeField] private bool UseConstant = true;
    [SerializeField] private T ConstantValue;
    [SerializeField] private VariableObject<T> Variable;

    public VariableReference()
    {
    }

    public VariableReference(T value)
    {
        UseConstant = true;
        ConstantValue = value;
    }

    public virtual T Value
    {
        get => UseConstant ? ConstantValue : Variable.Value;
        set
        {
            if (UseConstant)
            {
                ConstantValue = value;
            }
            else
            {
                Variable.Value = value;
            }
        }
    }

#if DEVELOPER_MODE
    public void SetVariable(VariableObject<T> variableObject)
    {
        Variable = variableObject;
        UseConstant = false;
    }
#endif

    public void AddListener(UnityAction action)
    {
        if (Variable == null)
        {
            Debug.LogError("Failed to add action to listener list. There are no Variable Object");
            return;
        }

        if (Variable.Listeners.Contains(action))
        {
            Debug.LogWarning("An action is add to listener list twice", Variable);
        }

        Variable.Listeners.Add(action);
    }

    public void RemoveListener(UnityAction action)
    {
        if (Variable == null)
        {
            Debug.LogError("Failed to add action to listener list. There are no Variable Object");
            return;
        }

        if (!Variable.Listeners.Contains(action))
        {
            Debug.LogWarning("Failed to remove action from listener list. The action is not in listener list",
                Variable);
            return;
        }

        Variable.Listeners.Remove(action);
    }

    public static implicit operator T(VariableReference<T> reference)
    {
        return reference.Value;
    }
}