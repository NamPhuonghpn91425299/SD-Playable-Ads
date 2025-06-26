using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCurve : MonoBehaviour
{
    [SerializeField] private PathBase _pathBase;
    public PathBase pathBase => _pathBase;
    public Vector3 GetPoinInBezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 point = uu * p0;
        point += 2 * u * t * p1;
        point += tt * p2;
        return point;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_pathBase.startPoint == null || _pathBase.controlPoint == null || _pathBase.endPoint == null) return;
        int number = 50;
        Gizmos.color = Color.green;
        for (int i = 0; i < number; i++)
        {
            float t1 = (float)i / number;
            float t2 = (float)(i + 1) / number;
            Vector3 point1 = GetPoinInBezierCurve(t1, _pathBase.startPoint.position, _pathBase.controlPoint.position, _pathBase.endPoint.position);
            Vector3 point2 = GetPoinInBezierCurve(t2, _pathBase.startPoint.position, _pathBase.controlPoint.position, _pathBase.endPoint.position);
            Gizmos.DrawLine(point1, point2);
        }
    }
#endif
}
[System.Serializable]
public class PathBase
{
    public Transform startPoint;
    public Transform controlPoint;
    public Transform endPoint;
    public float moveSpeed;
    public bool isChangeSpeed;
    public float timeChangeSpeed;
    public float rotateAngle;
    public bool isAttackPath;
    public AttackType attackType;
    public TypePoint typePoint;
    public BulletType bulletType;
    public HelicopterMoveType helicopterMoveType;

    [Header("---------------------------")]
    [Range(0f, 1f)]public float percentOfLeghtChangeRotate;
    [Header("MoveForward || MoveBackwardandRotaForward -> MoveandRotatoPlayer")]
    public float durationRotateToPlayer;
    [Header("MoveandRotatoPlayer -> MoveForward")]
    public float durationRotateToNextPath;
    [Header("MoveForward || MoveandRotatoPlayer -> MoveBackwardandRotaForward")]
    [Header("MoveBackwardandRotaForward -> MoveForward")]
    public float durationRotateBackToDefault;
    [Header("Time delay for next path")] 
    public float timeDelay;
    [Header("MoveandRotatoPlayer angle")]
    public float angleChangeZ;
    [Header("MoveForward angle")]
    public float angleChangeX;
}
public enum AttackType
{
    OnMove,
    OnPoint
}
public enum TypePoint
{
    StartPoint,
    EndPoint
}
public enum BulletType
{
    Rocket,
    MachineGun
}
public enum HelicopterMoveType
{
    MoveForward,
    MoveandRotatoPlayer,
    MoveBackwardandRotaForward
}

