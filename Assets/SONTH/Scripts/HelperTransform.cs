using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperTransform : MonoBehaviour
{
    public  ActionsManager actionManager;
    public void MoveBySpeed(Transform objToMove, Vector3 targetPos, float speed, System.Action onComplete)
    {
        actionManager.StartAction(Behaviour.Transform,MoveCoroutineBySpeed(objToMove, targetPos, speed, onComplete));
    }
    public void MoveByDuration(Transform objtToMove, Vector3 targetPos, float duration, System.Action onComplete)
    {
        actionManager.StartAction(Behaviour.Transform, MoveCoroutineByDuration(objtToMove, targetPos, duration, onComplete));
    }
    public void RotateBySpeed(Transform objToRotate, Vector3 axis, float targetAngle, float speed, System.Action onComplete)
    {
        actionManager.StartAction(Behaviour.Rotate, RotateCoroutineBySpeed(objToRotate, axis, targetAngle, speed, onComplete));
    }
    public void RotateBySpeed(Transform objToRotate, Quaternion targetQuaternion, float speed, System.Action onComplete)
    {
        actionManager.StartAction(Behaviour.Rotate, RotateCoroutineBySpeed(objToRotate, targetQuaternion, speed, onComplete));
    }
    public void RotateByDuration(Transform objToRotate, Vector3 axis, float targetAngle, float duration, System.Action onComplete)
    {
        actionManager.StartAction(Behaviour.Rotate, RotateCoroutineByDuration(objToRotate, axis, targetAngle, duration, onComplete));
    }
    public void RotateByDuration(Transform objToRotate, Quaternion targetQuaternion, float duration, System.Action onComplete)
    {
        actionManager.StartAction(Behaviour.Rotate, RotateCoroutineByDuration(objToRotate, targetQuaternion, duration, onComplete));
    }
    public void MovePathBySpeed(Transform objToMove, Transform[] path, float speed, System.Action onComplete)
    {
        actionManager.StartAction(Behaviour.Transform, MovePathCoroutineBySpeed(objToMove, path, speed, onComplete));
    }
    public void MovePathByDuration(Transform objToMove, Transform[] path, float duration, System.Action onComplete)
    {
        actionManager.StartAction(Behaviour.Transform, MovePathCoroutinByDuration(objToMove, path, duration, onComplete));
    }
    private IEnumerator MoveCoroutineBySpeed(Transform objToMove, Vector3 targetPos, float speed, System.Action onComplete)
    {
        while(Vector3.Distance(objToMove.position, targetPos) > .01f) { 
            objToMove.position = Vector3.MoveTowards(objToMove.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.position = targetPos;
        onComplete?.Invoke();
    }
    private IEnumerator MoveCoroutineByDuration(Transform objtToMove, Vector3 targetPos, float duration, System.Action onComplete)
    {
        Vector3 startPos = objtToMove.position;
        float time = 0;
        while(time < duration)
        {
            time += Time.deltaTime;
            float t = time/ duration;
            objtToMove.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        objtToMove.position = targetPos;
        onComplete?.Invoke();
    }
    private IEnumerator RotateCoroutineBySpeed(Transform objToRotate, Vector3 axis, float targetAngle, float speed, System.Action onComplete)
    {
        Quaternion startRotation = objToRotate.rotation;
        Quaternion endRotation = Quaternion.AngleAxis(targetAngle, axis);
        while(Quaternion.Angle(objToRotate.rotation, endRotation) > .1f)
        {
            objToRotate.rotation = Quaternion.RotateTowards(objToRotate.rotation, endRotation, speed * Time.deltaTime);
            yield return null;
        }
        objToRotate.rotation = endRotation;
        onComplete?.Invoke();
    }
    private IEnumerator RotateCoroutineBySpeed(Transform objToRotate, Quaternion targetQuaternion, float speed, System.Action onComplete)
    {
        while (Quaternion.Angle(objToRotate.rotation, targetQuaternion) > .1f)
        {
            objToRotate.rotation = Quaternion.RotateTowards(objToRotate.rotation, targetQuaternion, speed * Time.deltaTime);
            yield return null;
        }
        objToRotate.rotation = targetQuaternion;
        onComplete?.Invoke();
    }
    private IEnumerator RotateCoroutineByDuration(Transform objToRotate, Vector3 axis, float targetAngle, float duration, System.Action onComplete)
    {
        Quaternion startRotation = objToRotate.rotation;
        Quaternion endRotation = Quaternion.AngleAxis(targetAngle, axis);
        float time = 0;
        while(time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            objToRotate.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
        objToRotate.rotation = endRotation;
        onComplete?.Invoke();
    }
    private IEnumerator RotateCoroutineByDuration(Transform objToRotate, Quaternion targetQuaternion, float duration, System.Action onComplete)
    {
        Quaternion startRotation = objToRotate.rotation;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            objToRotate.rotation = Quaternion.Slerp(startRotation, targetQuaternion, t);
            yield return null;
        }
        objToRotate.rotation = targetQuaternion;
        onComplete?.Invoke();
    }
    private IEnumerator MovePathCoroutineBySpeed(Transform objToMove,Transform[] path, float speed, System.Action onComplete)
    {
        int index = 0;
        while(index < path.Length)
        {
            while(Vector3.Distance(objToMove.position, path[index].position) > .01f)
            {
                objToMove.position = Vector3.MoveTowards(objToMove.position, path[index].position, speed * Time.deltaTime);
                yield return null;
                if (Vector3.Distance(objToMove.position, path[index].position) <= .01f)
                {
                    index++;
                    if (index == path.Length) break;
                }
            }
        }
        objToMove.transform.position = path[path.Length - 1].position;
        onComplete?.Invoke();
    }
    private IEnumerator MovePathCoroutinByDuration(Transform objToMove, Transform[] path, float duration, System.Action onComplete)
    {
        if (path.Length == 0)
        {
            yield break; 
        }

        float pathLength = 0f;
        for (int i = 0; i < path.Length - 1; i++)
        {
            pathLength += Vector3.Distance(path[i].position, path[i + 1].position);
        }

        float time = 0f;
        int index = 0;
        while (index < path.Length - 1)
        {
            Vector3 startPosition = path[index].position;
            Vector3 targetPosition = path[index + 1].position;

            float distance = Vector3.Distance(startPosition, targetPosition);
            float moveDuration = duration * (distance / pathLength);

            while (time < moveDuration)
            {
                time += Time.deltaTime;
                float fractionOfPath = time / moveDuration;
                objToMove.position = Vector3.Lerp(startPosition, targetPosition, fractionOfPath);

                yield return null;
            }

            objToMove.position = targetPosition;
            time = 0f;
            index++;

            if (index >= path.Length - 1)
            {
                onComplete?.Invoke();
                yield break;
            }
        }
    }
    public void Kill(Behaviour behaviour)
    {
        actionManager.StopAction(behaviour);
    }
    public void StopAllActions()
    {
        actionManager.StopAllActions();
    }
}
