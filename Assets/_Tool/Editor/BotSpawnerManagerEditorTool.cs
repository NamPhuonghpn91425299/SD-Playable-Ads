// #if UNITY_EDITOR
// using UnityEngine;
// using UnityEditor;
// using System.Linq; // Cần cho Enumerable.Range
//
// public class BotSpawnerManagerEditorTool : EditorWindow
// {
//     private BotSpawnerManager spawnerManager;
//     private Vector2 scrollPositionStates;
//     private string stateIndexToKillInput = "0";
//     private string ruleNameToSpawnInput = "";
//     private string stateIndexForSpawnRuleInput = "0";
//
//     [MenuItem("Tools/Bot Spawner Manager Tool")] // Thêm menu item để mở cửa sổ
//     public static void ShowWindow()
//     {
//         GetWindow<BotSpawnerManagerEditorTool>("Bot Spawner Tool");
//     }
//
//     void OnGUI()
//     {
//         GUILayout.Label("Bot Spawner Manager Tool", EditorStyles.boldLabel);
//         GUILayout.Space(10);
//
//         // Cố gắng tìm BotSpawnerManager nếu chưa có hoặc nếu scene thay đổi
//         if (spawnerManager == null || spawnerManager.gameObject == null) // Kiểm tra gameObject để xử lý khi scene thay đổi
//         {
//             spawnerManager = FindObjectOfType<BotSpawnerManager>();
//         }
//
//         if (spawnerManager == null)
//         {
//             EditorGUILayout.HelpBox("Không tìm thấy BotSpawnerManager trong Scene hiện tại. Hãy đảm bảo có một instance đang hoạt động.", MessageType.Warning);
//             if (GUILayout.Button("Thử tìm lại BotSpawnerManager"))
//             {
//                 spawnerManager = FindObjectOfType<BotSpawnerManager>();
//             }
//             return;
//         }
//
//         // Nút để "ping" GameObject của SpawnerManager trong Hierarchy
//         if (GUILayout.Button("Ping BotSpawnerManager GameObject"))
//         {
//             EditorGUIUtility.PingObject(spawnerManager.gameObject);
//         }
//         GUILayout.Space(10);
//
//         // Hiển thị thông tin trạng thái chi tiết
//         EditorGUILayout.LabelField("--- Thông tin Trạng thái Chi tiết ---", EditorStyles.boldLabel);
//         scrollPositionStates = EditorGUILayout.BeginScrollView(scrollPositionStates, GUILayout.Height(150));
//         EditorGUILayout.TextArea(spawnerManager.GetDetailedStateInfo());
//         EditorGUILayout.EndScrollView();
//
//         if (GUILayout.Button("In thông tin chi tiết ra Console"))
//         {
//             spawnerManager.PrintDetailedStateInfo();
//         }
//         GUILayout.Space(10);
//         
//         // Kill bots trong một state
//         EditorGUILayout.LabelField("--- Kill Bots ---", EditorStyles.boldLabel);
//         EditorGUILayout.BeginHorizontal();
//         EditorGUILayout.LabelField("State Index to Kill:", GUILayout.Width(120));
//         stateIndexToKillInput = EditorGUILayout.TextField(stateIndexToKillInput, GUILayout.Width(50));
//         if (GUILayout.Button("Kill All Bots in State"))
//         {
//             if (Application.isPlaying)
//             {
//                 if (int.TryParse(stateIndexToKillInput, out int stateIdx))
//                 {
//                     if (spawnerManager.botSpawnStates != null && stateIdx >= 0 && stateIdx < spawnerManager.botSpawnStates.Length)
//                     {
//                         spawnerManager.KillAllBotsInState(stateIdx);
//                         Debug.Log($"(Editor) Đã yêu cầu kill tất cả bot trong state {stateIdx}");
//                     }
//                     else
//                     {
//                         Debug.LogWarning($"(Editor) State index '{stateIdx}' không hợp lệ.");
//                     }
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"(Editor) Input '{stateIndexToKillInput}' không phải là số hợp lệ cho state index.");
//                 }
//             }
//             else
//             {
//                 EditorUtility.DisplayDialog("Lỗi", "Chức năng này chỉ hoạt động khi game đang chạy.", "OK");
//             }
//         }
//         EditorGUILayout.EndHorizontal();
//         if (GUILayout.Button("Clear All Bots (Toàn bộ)"))
//         {
//             if (Application.isPlaying)
//             {
//                 spawnerManager.ClearAllBots();
//                 Debug.Log("(Editor) Đã yêu cầu Clear All Bots.");
//             }
//             else
//             {
//                 EditorUtility.DisplayDialog("Lỗi", "Chức năng này chỉ hoạt động khi game đang chạy.", "OK");
//             }
//         }
//  
//     }
//
//     // Được gọi khi EditorWindow được focus hoặc khi scene thay đổi
//     void OnFocus()
//     {
//         // Cập nhật lại tham chiếu spawnerManager nếu cần
//         if (spawnerManager == null && BotSpawnerManager.Instance != null)
//         {
//             spawnerManager = BotSpawnerManager.Instance;
//         }
//         else if (BotSpawnerManager.Instance == null)
//         {
//             spawnerManager = null; // Xóa tham chiếu nếu instance không còn
//         }
//         // Hoặc bạn có thể tìm lại mỗi khi focus:
//         // spawnerManager = FindObjectOfType<BotSpawnerManager>();
//     }
//
//     // Cũng hữu ích để cập nhật khi có thay đổi trong hierarchy
//     void OnHierarchyChange()
//     {
//          spawnerManager = FindObjectOfType<BotSpawnerManager>();
//          Repaint();
//     }
//     // Được gọi khi Play Mode State thay đổi
//     void OnPlayModeStateChanged(PlayModeStateChange state)
//     {
//         if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
//         {
//             spawnerManager = null; // Xóa tham chiếu khi thoát Play Mode
//         }
//         else if (state == PlayModeStateChange.EnteredPlayMode)
//         {
//             // Cố gắng tìm lại khi vào Play Mode
//             // Cần delay một chút để đảm bảo các đối tượng đã Awake
//             EditorApplication.delayCall += () => {
//                 spawnerManager = BotSpawnerManager.Instance ?? FindObjectOfType<BotSpawnerManager>();
//                 Repaint();
//             };
//         }
//     }
//
//     void OnEnable()
//     {
//         EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
//     }
//
//     void OnDisable()
//     {
//         EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
//     }
// }
// #endif