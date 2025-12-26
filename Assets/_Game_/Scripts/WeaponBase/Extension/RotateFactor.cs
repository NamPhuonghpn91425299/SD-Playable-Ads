using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RotateFactor
{
    public Transform Destination;
    public Transform ObjRotate;
    public float Speed = 10;
    public float AngleToCheckDone = 2;
    public bool IsEndless;
    Coroutine _cor;
    MonoBehaviour _main;
    Quaternion _originsQuater;

    public void Setup(MonoBehaviour main)
    {
        _main = main;
        _originsQuater = ObjRotate.localRotation;
    }

    public void Rotate(Action doneAct)
    {
        if (_main == null) return;
        if (_cor != null) _main.StopCoroutine(_cor);
        _cor = _main.StartCoroutine(RotateIE(doneAct));
    }

    IEnumerator RotateIE(Action doneAct)
    {
        Quaternion desiredQuater = Quaternion.LookRotation(Destination.position - ObjRotate.position);
        while (IsEndless || Quaternion.Angle(ObjRotate.rotation, desiredQuater) > AngleToCheckDone)
        {
            ObjRotate.rotation = Quaternion.RotateTowards(ObjRotate.rotation, desiredQuater, Speed * Time.deltaTime);
            yield return null;
        }
        doneAct?.Invoke();
    }

    IEnumerator ReturnIE()
    {
        float timer = 0;
        float timeRotate = Quaternion.Angle(ObjRotate.localRotation, _originsQuater) / Speed;
        while (timer < timeRotate)
        {
            timer += Time.deltaTime;
            ObjRotate.localRotation = Quaternion.RotateTowards(ObjRotate.localRotation, _originsQuater, Speed * Time.deltaTime);
            yield return null;
        }
    }

    public void StopRotate()
    {
        if (_main == null) return;
        if (_cor != null) _main.StopCoroutine(_cor);
    }

    public void ReturnOrigin(bool immidiately = false)
    {
        if (_main == null) return;
        if (immidiately)
            ObjRotate.localRotation = _originsQuater;
        else
        {
            if (_cor != null) _main.StopCoroutine(_cor);
            _cor = _main.StartCoroutine(ReturnIE());
        }
       
    }
}
