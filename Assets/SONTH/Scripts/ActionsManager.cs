using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsManager : MonoBehaviour
{
    private IDictionary<Behaviour, Coroutine> _dic = new Dictionary<Behaviour, Coroutine>();
    public void StartAction(Behaviour behaviour, IEnumerator coroutine)
    {
        StopAction(behaviour);
        _dic[behaviour] = StartCoroutine(coroutine);
    }
    public void StopAction(Behaviour behaviour)
    {
        if (_dic.ContainsKey(behaviour))
        {
            StopCoroutine(_dic[behaviour]);
            _dic.Remove(behaviour);
        }
    }
    public void StopAllActions()
    {
        foreach (var action in _dic.Values)
        {
            StopCoroutine(action);
        }
        _dic.Clear();
    }
}

public enum Behaviour
{
    Transform,
    Rotate,
    Scale
}