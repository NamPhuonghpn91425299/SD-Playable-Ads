using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ReverseAnimationCreator : EditorWindow
{
    private AnimationClip sourceClip;
    private string newClipName = "";
    private string savePath = "Assets/";

    [MenuItem("Tools/Reverse Animation Creator")]
    public static void ShowWindow()
    {
        GetWindow<ReverseAnimationCreator>("Reverse Animation Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tạo Animation Đảo Ngược", EditorStyles.boldLabel);

        sourceClip = EditorGUILayout.ObjectField("Animation gốc", sourceClip, typeof(AnimationClip), false) as AnimationClip;
        
        newClipName = EditorGUILayout.TextField("Tên Animation mới", newClipName);
        
        savePath = EditorGUILayout.TextField("Đường dẫn lưu", savePath);
        
        if (GUILayout.Button("Chọn thư mục lưu"))
        {
            string path = EditorUtility.SaveFolderPanel("Chọn thư mục lưu Animation", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Chuyển đổi từ đường dẫn hệ thống sang đường dẫn project
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                savePath = path;
            }
        }

        EditorGUI.BeginDisabledGroup(sourceClip == null || string.IsNullOrEmpty(newClipName));
        if (GUILayout.Button("Tạo Animation Đảo Ngược"))
        {
            CreateReverseAnimation();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void CreateReverseAnimation()
    {
        if (sourceClip == null)
        {
            EditorUtility.DisplayDialog("Lỗi", "Vui lòng chọn Animation gốc", "OK");
            return;
        }

        // Tạo AnimationClip mới
        AnimationClip newClip = new AnimationClip();
        newClip.name = newClipName;
        
        // Đặt các thuộc tính giống với clip gốc
        newClip.frameRate = sourceClip.frameRate;
        newClip.wrapMode = sourceClip.wrapMode;
        
        // Lấy tất cả các đường cong từ clip gốc
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(sourceClip);
        
        // Đảo ngược từng đường cong
        foreach (EditorCurveBinding binding in curveBindings)
        {
            AnimationCurve originalCurve = AnimationUtility.GetEditorCurve(sourceClip, binding);
            AnimationCurve reversedCurve = new AnimationCurve();
            
            // Lấy thời gian của keyframe cuối cùng
            float clipLength = originalCurve.keys[originalCurve.length - 1].time;
            
            // Tạo các keyframe mới theo thứ tự đảo ngược
            for (int i = 0; i < originalCurve.length; i++)
            {
                Keyframe originalKey = originalCurve.keys[i];
                float newTime = clipLength - originalKey.time;
                
                // Đảo ngược tangent để đảm bảo animation mượt mà
                Keyframe newKey = new Keyframe(newTime, originalKey.value, -originalKey.outTangent, -originalKey.inTangent);
                reversedCurve.AddKey(newKey);
            }
            
            // Áp dụng đường cong đảo ngược vào clip mới
            AnimationUtility.SetEditorCurve(newClip, binding, reversedCurve);
        }
        
        // Xử lý các ObjectReferenceBinding (nếu có)
        EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(sourceClip);
        foreach (EditorCurveBinding binding in objectBindings)
        {
            ObjectReferenceKeyframe[] originalKeyframes = AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);
            List<ObjectReferenceKeyframe> reversedKeyframes = new List<ObjectReferenceKeyframe>();
            
            // Lấy thời gian của keyframe cuối cùng
            float clipLength = originalKeyframes[originalKeyframes.Length - 1].time;
            
            for (int i = 0; i < originalKeyframes.Length; i++)
            {
                ObjectReferenceKeyframe originalKey = originalKeyframes[i];
                ObjectReferenceKeyframe newKey = new ObjectReferenceKeyframe
                {
                    time = clipLength - originalKey.time,
                    value = originalKey.value
                };
                reversedKeyframes.Add(newKey);
            }
            
            // Sắp xếp lại các keyframe theo thời gian
            reversedKeyframes.Sort((a, b) => a.time.CompareTo(b.time));
            
            // Áp dụng vào clip mới
            AnimationUtility.SetObjectReferenceCurve(newClip, binding, reversedKeyframes.ToArray());
        }
        
        // Lưu clip mới
        string finalPath = savePath;
        if (!finalPath.EndsWith("/"))
            finalPath += "/";
            
        finalPath += newClipName + ".anim";
        
        AssetDatabase.CreateAsset(newClip, finalPath);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Thành công", "Đã tạo Animation đảo ngược tại: " + finalPath, "OK");
        
        // Chọn asset mới tạo trong Project view
        Selection.activeObject = newClip;
    }
}