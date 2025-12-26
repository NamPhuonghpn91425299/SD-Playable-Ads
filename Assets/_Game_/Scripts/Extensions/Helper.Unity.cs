using UnityEngine;
public static partial class Helper
{
    public static void RotateToward(this Transform main, Quaternion target, float maxDegreesDelta)
    {
        main.rotation = Quaternion.RotateTowards(main.rotation, target, maxDegreesDelta);
    }

    public static void RotateLocalToward(this Transform main, Quaternion localTarget, float maxDegreesDelta)
    {
        main.localRotation = Quaternion.RotateTowards(main.localRotation, localTarget, maxDegreesDelta);
    }
}