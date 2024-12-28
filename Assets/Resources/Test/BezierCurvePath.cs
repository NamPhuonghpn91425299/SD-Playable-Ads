using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Component chính để lưu trữ và quản lý đường cong
public class BezierCurvePath : MonoBehaviour
{
    public List<Transform> controlPoints = new List<Transform>();
    public Color curveColor = Color.green;
    public Color handleColor = Color.yellow;
    public float handleSize = 0.5f;
    public bool showControlPoints = true;
    public bool showCurve = true;

    public Vector3 GetPointOnCurve(float t, int segmentIndex)
    {
        if (segmentIndex * 3 + 3 >= controlPoints.Count) return Vector3.zero;

        Vector3 p0 = controlPoints[segmentIndex * 3].position;
        Vector3 p1 = controlPoints[segmentIndex * 3 + 1].position;
        Vector3 p2 = controlPoints[segmentIndex * 3 + 2].position;
        Vector3 p3 = controlPoints[segmentIndex * 3 + 3].position;

        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    public int SegmentCount
    {
        get
        {
            return (controlPoints.Count - 1) / 3;
        }
    }

    public void AddSegment(Vector3 position)
    {
        // Tạo các điểm điều khiển mới
        GameObject point1 = CreateControlPoint("Point " + (controlPoints.Count + 1));
        GameObject point2 = CreateControlPoint("Point " + (controlPoints.Count + 2));
        GameObject point3 = CreateControlPoint("Point " + (controlPoints.Count + 3));

        // Tính toán vị trí cho các điểm mới
        Vector3 lastPos = controlPoints[controlPoints.Count - 1].position;
        Vector3 direction = (position - lastPos).normalized;
        float spacing = 2f;

        point1.transform.position = lastPos + direction * spacing;
        point2.transform.position = position - direction * spacing;
        point3.transform.position = position;

        // Thêm vào list
        controlPoints.Add(point1.transform);
        controlPoints.Add(point2.transform);
        controlPoints.Add(point3.transform);
    }

    private GameObject CreateControlPoint(string name)
    {
        GameObject point = new GameObject(name);
        point.transform.parent = transform;
        return point;
    }
}

// Custom Editor cho BezierCurvePath
#if UNITY_EDITOR
[CustomEditor(typeof(BezierCurvePath))]
public class BezierCurvePathEditor : Editor
{
    private BezierCurvePath curve;
    private Transform selectedPoint;
    private int selectedIndex = -1;

    private void OnEnable()
    {
        curve = (BezierCurvePath)target;
        Tools.hidden = true;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        // Draw default properties
        DrawDefaultInspector();

        // Add segment button
        if (GUILayout.Button("Add Segment"))
        {
            Undo.RecordObject(curve, "Add Segment");
            Vector3 position = curve.controlPoints.Count > 0 
                ? curve.controlPoints[curve.controlPoints.Count - 1].position + Vector3.right * 2
                : Vector3.zero;
            curve.AddSegment(position);
            EditorUtility.SetDirty(curve);
        }

        if (selectedIndex >= 0 && selectedIndex < curve.controlPoints.Count)
        {
            GUILayout.Space(10);
            GUILayout.Label("Selected Point: " + selectedIndex);
            
            if (GUILayout.Button("Remove Point"))
            {
                RemovePoint(selectedIndex);
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    private void RemovePoint(int index)
    {
        if (index < 0 || index >= curve.controlPoints.Count) return;

        Undo.RecordObject(curve, "Remove Control Point");
        
        // Remove the control point and its handles if they exist
        int groupStart = (index / 3) * 3;
        int pointsToRemove = 3;
        
        // Don't remove more points than we have
        pointsToRemove = Mathf.Min(pointsToRemove, curve.controlPoints.Count - groupStart);
        
        for (int i = 0; i < pointsToRemove; i++)
        {
            if (groupStart < curve.controlPoints.Count)
            {
                DestroyImmediate(curve.controlPoints[groupStart].gameObject);
                curve.controlPoints.RemoveAt(groupStart);
            }
        }

        selectedIndex = -1;
        EditorUtility.SetDirty(curve);
    }

    private void OnSceneGUI()
    {
        if (curve == null) return;

        // Draw the curve
        if (curve.showCurve)
        {
            Handles.color = curve.curveColor;
            for (int i = 0; i < curve.SegmentCount; i++)
            {
                DrawCurveSegment(i);
            }
        }

        // Draw control points
        if (curve.showControlPoints)
        {
            for (int i = 0; i < curve.controlPoints.Count; i++)
            {
                if (curve.controlPoints[i] == null) continue;

                // Draw handle
                Handles.color = curve.handleColor;
                Vector3 position = curve.controlPoints[i].position;

                // Check if this point is selected
                bool isSelected = (i == selectedIndex);
                float size = curve.handleSize * (isSelected ? 1.5f : 1f);

                // Draw control point handle
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.FreeMoveHandle(
                    position,
                    Quaternion.identity,
                    size,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );

                // Handle selection
                if (Handles.Button(position, Quaternion.identity, size, size, Handles.SphereHandleCap))
                {
                    selectedIndex = i;
                    selectedPoint = curve.controlPoints[i];
                    Repaint();
                }

                // Update position if changed
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(curve.controlPoints[i], "Move Point");
                    curve.controlPoints[i].position = newPosition;
                    EditorUtility.SetDirty(curve.controlPoints[i]);
                }

                // Draw labels
                Handles.Label(position + Vector3.up * size, "Point " + i);
            }
        }
    }

    private void DrawCurveSegment(int index)
    {
        int steps = 20;
        Vector3 previousPoint = curve.GetPointOnCurve(0, index);

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 currentPoint = curve.GetPointOnCurve(t, index);
            Handles.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
}
#endif