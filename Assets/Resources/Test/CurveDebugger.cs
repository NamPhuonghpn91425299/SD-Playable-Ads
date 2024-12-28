using UnityEngine;

public class CurveDebugger : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private int resolution = 100;
    [SerializeField] private float curveWidth = 5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (curve == null || resolution <= 0) return;

        Vector3 prevPoint = transform.position;

        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            float x = t * curveWidth;
            float y = curve.Evaluate(t);
            Vector3 point = transform.position + new Vector3(x, y, 0);

            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, point);
            }

            prevPoint = point;
        }
    }
}