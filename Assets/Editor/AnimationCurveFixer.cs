using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class AnimationCurveFixer : EditorWindow
{
    [SerializeField] private AnimationClip[] sourceClips;
    private string saveFolder = "Assets";

    [MenuItem("Tools/Animation/Fix Each Animation Clip Separately")]
    public static void ShowWindow()
    {
        GetWindow<AnimationCurveFixer>("Fix Animation Clips");
    }

    private void OnGUI()
    {
        GUILayout.Label("Fix Multiple Animation Clips Individually", EditorStyles.boldLabel);

        SerializedObject so = new SerializedObject(this);
        SerializedProperty sp = so.FindProperty("sourceClips");
        EditorGUILayout.PropertyField(sp, new GUIContent("Source Clips (kéo thả nhiều .anim vào đây)"), true);
        so.ApplyModifiedProperties();

        GUILayout.Space(10);
        GUILayout.Label("Thư mục xuất các clip đã fix:", EditorStyles.label);
        saveFolder = EditorGUILayout.TextField("Lưu Tại", saveFolder);

        GUILayout.Space(5);
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "📥 Kéo thả nhiều AnimationClip (.anim) vào đây", EditorStyles.helpBox);

        if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                List<AnimationClip> clips = new List<AnimationClip>(sourceClips);
                foreach (var dragged in DragAndDrop.objectReferences)
                {
                    if (dragged is AnimationClip clip && !clips.Contains(clip))
                        clips.Add(clip);
                }
                sourceClips = clips.ToArray();
                GUI.changed = true;
            }
            Event.current.Use();
        }

        if (GUILayout.Button("Fix All Clips"))
        {
            if (sourceClips == null || sourceClips.Length == 0)
            {
                Debug.LogWarning("❌ Vui lòng chọn ít nhất một animation clip.");
                return;
            }

            foreach (var clip in sourceClips)
            {
                if (clip == null) continue;

                AnimationClip newClip = new AnimationClip();
                newClip.name = clip.name + "_Fixed";
                newClip.frameRate = clip.frameRate;
                newClip.legacy = clip.legacy;

                // Xoá RootMotion nếu có
                newClip.ClearCurves();

                string path = Path.Combine(saveFolder, newClip.name + ".anim");
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                CopyAndFixCurves(clip, newClip);
                AssetDatabase.CreateAsset(newClip, path);
                Debug.Log("✅ Đã tạo clip fix và xoá RootMotion: " + path);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("🎉 Hoàn tất fix tất cả các animation clips!");
        }
    }

    private void CopyAndFixCurves(AnimationClip source, AnimationClip target)
    {
        var bindings = AnimationUtility.GetCurveBindings(source);
        foreach (var binding in bindings)
        {
            if (binding.propertyName.Contains("RootT") || binding.propertyName.Contains("RootQ"))
                continue; // Bỏ qua RootMotion (position/rotation)

            var sourceCurve = AnimationUtility.GetEditorCurve(source, binding);
            AnimationCurve newCurve = new AnimationCurve();

            foreach (var key in sourceCurve.keys)
            {
                Keyframe cleanKey = new Keyframe(key.time, key.value, key.inTangent, key.outTangent);
                newCurve.AddKey(cleanKey);
            }

            AnimationUtility.SetEditorCurve(target, binding, newCurve);
        }

        var sourceEvents = AnimationUtility.GetAnimationEvents(source);
        AnimationUtility.SetAnimationEvents(target, sourceEvents);
    }

    private void OnEnable()
    {
        if (sourceClips == null) sourceClips = new AnimationClip[0];
    }
}
