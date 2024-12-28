using UnityEditor;
using UnityEngine;

public class CurveWindow : EditorWindow
{
    private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    [MenuItem("Tools/Curve Editor")]
    public static void OpenWindow()
    {
        GetWindow<CurveWindow>("Curve Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Edit Curve", EditorStyles.boldLabel);
        curve = EditorGUILayout.CurveField("Animation Curve", curve);
    }
}