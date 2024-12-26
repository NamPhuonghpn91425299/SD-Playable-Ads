using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TUtiliti : MonoBehaviour
{
    public static float GetRotaSpeed(Transform myTrans, Vector3 targetPos, float currentMoveSpeed,
     float campAngle = 135)
    {
        //  (hệ số tốc độ)      * ( Hệ số khoảng cách) * (Hệ số góc)
        //(moveSpeed * consRota) * (50 / distance) * Mathf.Sin((angleOfTarget * * Mathf.Deg2Rad));   Công thức Mẫu

        Vector3 dir = targetPos - myTrans.position;
        float angleOfTarget = Vector3.Angle(myTrans.forward, dir);
        float distance = Vector3.Distance(myTrans.position, targetPos);
        //vì angleOfTarget càng lớn RotaSpeed càng nhỏ dẫn đến vòng bay càng to để tránh vòng bay quá lớn sẽ cố định góc phải nhỏ hơn 135
        //góc cố định càng nhỏ thì vòng bay sẽ càng nhỏ và ngược lại tối đa là 180
        return currentMoveSpeed * 115 / distance * Mathf.Sin((Mathf.Min(angleOfTarget, campAngle) * Mathf.Deg2Rad));
    }

    /// <summary>
    /// Ngiêng thân máy bay theo trục Z tuỳ thuốc vào điểm đến
    /// </summary>
    /// <param name="airPlaneTrans">Transform Gốc của AirPlane</param>
    /// <param name="rotaBody">Transform của phần thân sẽ nghiêng</param>
    /// <param name="destination">Điểm đến máy bay đang hướng tới</param>
    /// <param name="TiltSpeed">Tốc độ nghiêng thâm</param>
    /// <param name="campRotaAngle">Ghim góc nghiêng tránh nghiêng quá đà</param>
    public static void TiltBodyOnMoveZ(Transform airPlaneTrans, Transform rotaBody, Vector3 destination,
        float TiltSpeed = 0.2f, float clampRotaAngle = 25)
    {
        float angle = GetAngleYOfDirAndPoin(airPlaneTrans, destination);
        rotaBody.localEulerAngles = new Vector3(rotaBody.localEulerAngles.x, rotaBody.localEulerAngles.y,
            Mathf.Lerp(ConvertAngle180(rotaBody.localEulerAngles.z),
                Mathf.Clamp(angle, -clampRotaAngle, clampRotaAngle), TiltSpeed * Time.deltaTime));
    }
    /// <summary>
    /// Trả về độ lệch góc gữa 1 Obj Dir và Một điểm Độ lệch Góc xét trên trục Y
    ///  giá trị từ -180=>180
    /// </summary>
    /// <param name="dirObj"></param>
    /// <param name="poinPos"></param>
    /// <returns></returns>
    public static float GetAngleYOfDirAndPoin(Transform dirObj, Vector3 poinPos)
    {
        float angle = GetAngleOfTwoPoinY(dirObj.position, dirObj.position + dirObj.forward) -
                      GetAngleOfTwoPoinY(dirObj.position, poinPos);
        return ConvertAngle180(angle);
    }
    public static float GetAngleOfTwoPoinY(Vector3 Startpos, Vector3 endPos)
    {
        Startpos.y = endPos.y;
        var dir = endPos - Startpos;
        return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
    }


    public static float ConvertAngle180(float angleInput)
    {
        return -Mathf.DeltaAngle(angleInput, 0);
    }

    /// <summary>
    /// smooth rota to targetPos sử dụng RotateTowards
    /// cái này nó quay đầu 1 cách tuyến tính
    /// </summary>
    /// <param name="myTrans">Transform của object muốn quay</param>
    /// <param name="targetPos">vị trí muốn hướng tới</param>
    /// <param name="rotaSpeed">tốc độ quay</param>
    public static void RotaToTarget(Transform myTrans, Vector3 targetPos, float rotaSpeed)
    {
        RotaToTarget(myTrans, targetPos, rotaSpeed, Vector3.up);
    }

    public static void RotaToTarget(Transform myTrans, Vector3 targetPos, float rotaSpeed, Vector3 UpWards)
    {
        var dir = targetPos - myTrans.position; //a vector pointing from pointA to pointB

        //Fix Look rotation viewing vector is zero
        //UnityEngine.Quaternion:LookRotation(UnityEngine.Vector3, UnityEngine.Vector3)
        //TUtilities: RotaToTarget(UnityEngine.Transform, UnityEngine.Vector3, single, UnityEngine.Vector3)(at
        if (dir.x != 0 || dir.y != 0 || dir.z != 0)
        {
            var rot = Quaternion.LookRotation(dir, UpWards); //calc a rotation that
            myTrans.rotation = Quaternion.RotateTowards(myTrans.rotation, rot, rotaSpeed * Time.deltaTime);
        }
    }
    /// <summary>
    /// Nghiêng thân báy bay theo vận tốc chỉ nghiêng khi tăng tốc hoặc giảm tốc
    /// 
    /// </summary>
    /// <param name="rotaBody">Transform của phần thân sẽ nghiêng</param>
    /// <param name="curentSpeed">Vận tốc hiện tại</param>
    /// <param name="EndSpeed">Vận tốc sẽ đạt tới</param>
    /// <param name="multConst">Trong trương hợp máy bay tăng tốc ít mà muốn nghiêng thân nhiều hoặc ngược lại thì tăng hoặc giảm multConst</param>
    /// <param name="TiltSpeed">//Tốc độ ngiêng thân</param>
    /// <param name="clampRotaAngle">Góc nghiêng tối đa</param>
    public static void TiltBodyOnMoveX(Transform rotaBody, float curentSpeed, float EndSpeed, float multConst = 1,
        float TiltSpeed = 1f, float clampRotaAngle = 15)
    {
        float angleValue = Mathf.Clamp((EndSpeed - curentSpeed) * multConst, -clampRotaAngle, clampRotaAngle);
        rotaBody.localEulerAngles =
            new Vector3(
                Mathf.Lerp(ConvertAngle180(rotaBody.localEulerAngles.x), angleValue, TiltSpeed * Time.deltaTime),
                rotaBody.localEulerAngles.y, rotaBody.localEulerAngles.z);
    }

}
