using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(MultiHandleParent))]
public class MultiHandleParentEditor : Editor
{
    void OnSceneGUI()
    {
        MultiHandleParent parent = (MultiHandleParent)target;

        // Move handle cho chính object
        if (parent.includeSelf)
        {
            DrawHandleForTransform(parent.transform);
        }

        // Move handle cho các con
        if (parent.includeChildren)
        {
            foreach (Transform child in parent.transform)
            {
                DrawHandleRecursive(child);
            }

            // Vẽ Catmull-Rom spline nối các điểm con
            DrawSplineForChildren(parent.transform);
        }
    }

    private void DrawHandleRecursive(Transform t)
    {
        DrawHandleForTransform(t);
        foreach (Transform child in t)
        {
            DrawHandleRecursive(child);
        }
    }

    private void DrawHandleForTransform(Transform t)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 newPos = Handles.PositionHandle(t.position, t.rotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(t, "Move Transform");
            t.position = newPos;
        }
    }

    private void DrawSplineForChildren(Transform parent)
    {
        List<Transform> pointsList = new List<Transform>();
        foreach (Transform child in parent)
        {
            pointsList.Add(child);
        }

        if (pointsList.Count < 2)
            return;

        Handles.color = Color.cyan;
        int resolution = 10;

        for (int i = 0; i < pointsList.Count - 1; i++)
        {
            Vector3 p0 = i == 0 ? pointsList[i].position : pointsList[i - 1].position;
            Vector3 p1 = pointsList[i].position;
            Vector3 p2 = pointsList[i + 1].position;
            Vector3 p3 = (i + 2 < pointsList.Count) ? pointsList[i + 2].position : p2;

            Vector3 prevPos = p1;
            for (int j = 1; j <= resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 pos = GetCatmullRomPosition(t, p0, p1, p2, p3);
                Handles.DrawLine(prevPos, pos);
                prevPos = pos;
            }
        }
    }

    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}
