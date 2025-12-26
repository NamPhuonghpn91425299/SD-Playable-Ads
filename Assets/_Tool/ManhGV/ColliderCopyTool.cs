#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class ColliderCopyToolDragAndDrop : EditorWindow
{
    private List<GameObject> targetObjects = new List<GameObject>(); // Bên trái - chưa có collider
    private List<GameObject> sourceObjects = new List<GameObject>(); // Bên phải - đã có collider

    [MenuItem("Tools/Collider Copy Tool (Multi Drag & Drop)")]
    public static void ShowWindow()
    {
        GetWindow<ColliderCopyToolDragAndDrop>("Collider Copy Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("📦 Collider Copy Tool", EditorStyles.boldLabel);
        GUILayout.Label("⬅️ Target (no collider) | ➡️ Source (has collider)", EditorStyles.helpBox);

        int rows = Mathf.Max(targetObjects.Count, sourceObjects.Count);

        for (int i = 0; i < rows; i++)
        {
            EditorGUILayout.BeginHorizontal();

            GameObject target = i < targetObjects.Count ? targetObjects[i] : null;
            GameObject source = i < sourceObjects.Count ? sourceObjects[i] : null;

            GameObject newTarget = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), true);
            GUILayout.Label("<==>", GUILayout.Width(40));
            GameObject newSource = (GameObject)EditorGUILayout.ObjectField(source, typeof(GameObject), true);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                if (i < targetObjects.Count) targetObjects.RemoveAt(i);
                if (i < sourceObjects.Count) sourceObjects.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                break;
            }

            if (i < targetObjects.Count)
                targetObjects[i] = newTarget;
            else if (newTarget != null)
                targetObjects.Add(newTarget);

            if (i < sourceObjects.Count)
                sourceObjects[i] = newSource;
            else if (newSource != null)
                sourceObjects.Add(newSource);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🧹 Clear Left", GUILayout.Height(30)))
            for (int i = 0; i < targetObjects.Count; i++)
                targetObjects[i] = null;
        if (GUILayout.Button("🧹 Clear Right", GUILayout.Height(30)))
        for (int i = 0; i < sourceObjects.Count; i++)
            sourceObjects[i] = null;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(15);
        GUILayout.Label("🎯 Drag GameObjects Below", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        DrawDropArea("Drop TARGETs here", AddTargets, Color.gray);
        GUILayout.Label("<==>", GUILayout.Width(40), GUILayout.Height(50));
        DrawDropArea("Drop SOURCEs here", AddSources, Color.cyan);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("📦 Copy All Colliders", GUILayout.Height(30)))
        {
            int count = Mathf.Min(targetObjects.Count, sourceObjects.Count);

            for (int i = 0; i < count; i++)
            {
                var target = targetObjects[i];
                var source = sourceObjects[i];

                if (target == null || source == null)
                {
                    Debug.LogWarning($"⚠️ Cặp {i} thiếu object, bỏ qua.");
                    continue;
                }

                CopyCollidersFromTo(source, target);
            }

            Debug.Log("✅ Collider đã được sao chép.");
        }

        if (GUILayout.Button("🧹 Clear All", GUILayout.Height(30)))
        {
            targetObjects.Clear();
            sourceObjects.Clear();
            Debug.Log("🧼 Danh sách đã được xóa sạch.");
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawDropArea(string label, Action<Object[]> onDrop, Color bgColor)
    {
        Color prevColor = GUI.color;
        GUI.color = bgColor;

        GUILayout.BeginVertical("box", GUILayout.Width(200), GUILayout.Height(50));
        GUI.color = prevColor;

        GUILayout.FlexibleSpace();
        GUILayout.Label(label, EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        Rect dropArea = GUILayoutUtility.GetLastRect();
        Event evt = Event.current;

        if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                onDrop?.Invoke(DragAndDrop.objectReferences);
                Event.current.Use();
            }
        }
    }

    private void AddTargets(Object[] objs)
    {
        foreach (var obj in objs)
        {
            if (obj is GameObject go && !targetObjects.Contains(go))
            {
                targetObjects.Add(go);
            }
        }
    }

    private void AddSources(Object[] objs)
    {
        foreach (var obj in objs)
        {
            if (obj is GameObject go && !sourceObjects.Contains(go))
            {
                sourceObjects.Add(go);
            }
        }
    }

    private void CopyCollidersFromTo(GameObject source, GameObject target)
    {
        foreach (var col in target.GetComponents<Collider>())
        {
            DestroyImmediate(col);
        }

        foreach (var sourceCol in source.GetComponents<Collider>())
        {
            Type type = sourceCol.GetType();
            Collider newCol = target.AddComponent(type) as Collider;

            if (type == typeof(BoxCollider))
            {
                var src = (BoxCollider)sourceCol;
                var dst = (BoxCollider)newCol;
                dst.center = src.center;
                dst.size = src.size;
                dst.isTrigger = src.isTrigger;
                dst.material = src.material;
            }
            else if (type == typeof(SphereCollider))
            {
                var src = (SphereCollider)sourceCol;
                var dst = (SphereCollider)newCol;
                dst.center = src.center;
                dst.radius = src.radius;
                dst.isTrigger = src.isTrigger;
                dst.material = src.material;
            }
            else if (type == typeof(CapsuleCollider))
            {
                var src = (CapsuleCollider)sourceCol;
                var dst = (CapsuleCollider)newCol;
                dst.center = src.center;
                dst.radius = src.radius;
                dst.height = src.height;
                dst.direction = src.direction;
                dst.isTrigger = src.isTrigger;
                dst.material = src.material;
            }
            else if (type == typeof(MeshCollider))
            {
                var src = (MeshCollider)sourceCol;
                var dst = (MeshCollider)newCol;
                dst.sharedMesh = src.sharedMesh;
                dst.convex = src.convex;
                dst.isTrigger = src.isTrigger;
                dst.material = src.material;
            }
        }
    }
}
#endif