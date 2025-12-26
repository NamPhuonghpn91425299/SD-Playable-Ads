#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static GameConstants;
// Script này tùy chỉnh giao diện của PathManager trong Inspector.
[CustomEditor(typeof(PathManager))]
public class PathManagerEditor : Editor
{
    private Dictionary<BotMoveType, bool> foldoutStates = new Dictionary<BotMoveType, bool>();
    private bool showClassifiedRoutesFoldout = true;

    public override void OnInspectorGUI()
    {
        // Vẽ các trường mặc định (như PlayMode)
        DrawDefaultInspector();
        
        PathManager pathManager = (PathManager)target;

        EditorGUILayout.Space(10);
        
        // Nút bấm để ép hệ thống làm mới ngay trong Editor
        if (GUILayout.Button("Refresh Waypoints in Scene"))
        {
            pathManager.CollectAllRoutesInScene();
            EditorUtility.SetDirty(pathManager);
        }

        EditorGUILayout.Space(10);
        
        // Foldout tổng để gom nhóm
        showClassifiedRoutesFoldout = EditorGUILayout.Foldout(showClassifiedRoutesFoldout, "Show Classified Routes Inspector", true, EditorStyles.boldLabel);

        if (showClassifiedRoutesFoldout)
        {
            // Kiểm tra và hiển thị dữ liệu
            if (pathManager.classifiedRoutes != null && pathManager.classifiedRoutes.Count > 0)
            {
                EditorGUI.indentLevel++;
                
                // Duyệt qua dictionary và vẽ các foldout con
                foreach (var pair in pathManager.classifiedRoutes)
                {
                    BotMoveType moveType = pair.Key;
                    List<PointGroup> routes = pair.Value;
                    
                    if (routes.Count > 0)
                    {
                        if (!foldoutStates.ContainsKey(moveType)) { foldoutStates[moveType] = false; }
                        
                        foldoutStates[moveType] = EditorGUILayout.Foldout(foldoutStates[moveType], $"{moveType} Routes ({routes.Count})", true);

                        if (foldoutStates[moveType])
                        {
                            EditorGUI.indentLevel++;
                            for(int i = 0; i < routes.Count; i++)
                            {
                                // Vẽ một ô cho từng tuyến đường, cho phép nhấn vào để tìm
                                EditorGUILayout.ObjectField($"    - {routes[i].name}", routes[i], typeof(PointGroup), true);
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("Data is empty. Press 'Force Refresh' button to collect data from the scene.", MessageType.Info);
            }
        }
    }
}
#endif