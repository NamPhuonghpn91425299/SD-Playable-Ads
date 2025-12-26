#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using static GameConstants;
/// <summary>
/// Cửa sổ Editor đa năng để tạo ra các tuyến đường (PointGroup) cho AI.
/// Cung cấp 2 chế độ:
/// 1. Simple Path: Tạo một đường thẳng với các điểm cách đều.
/// 2. Path From Flags: Tạo một "hành lang" ngẫu nhiên nối các điểm cờ (flag points).
/// Để mở: Tools > Ultimate Path Creator
/// </summary>
public class PathCreatorWindow : EditorWindow
{
    // Biến để theo dõi xem tab nào đang được chọn (0 hoặc 1)
    private int currentTab = 0;

    #region Biến cho Tab "Simple Path" (Chế độ đơn giản)
    // Các biến này được đặt tiền tố 'sp_' (Simple Path) để phân biệt
    private GUIContent sp_parentGroupContent = new GUIContent("Parent for New Groups", "Tùy chọn: Kéo một GameObject từ scene vào đây. Tất cả các group mới sẽ được tạo làm con của đối tượng này.");
    private Transform simple_parentGroup;
    private GUIContent sp_groupNameContent = new GUIContent("Base Group Name", "Tên cơ sở cho các group (sẽ tự động thêm số thứ tự, ví dụ: 'Simple_Path_0').");
    private string simple_groupName = "Simple_Path";
    private GUIContent sp_basePointNameContent = new GUIContent("Base Point Name", "Tên cơ sở cho các điểm con (sẽ được tự động thêm số thứ tự, ví dụ: 'Waypoint_0').");
    private string simple_basePointName = "Waypoint";
    private GUIContent sp_numberOfPointsContent = new GUIContent("Number of Points", "Số lượng điểm (waypoints) cần tạo.");
    private int simple_numberOfPoints = 3;
    private GUIContent sp_routeTypeContent = new GUIContent("Bot Move Type", "Loại đường đi này dành cho loại bot nào?");
    private BotMoveType _simpleRouteMoveType = BotMoveType.Infantry;
    private GUIContent sp_initialSpacingContent = new GUIContent("Spacing", "Khoảng cách giữa các điểm khi tạo.");
    private float simple_initialSpacing = 1.0f;
    private GUIContent sp_gizmoColorContent = new GUIContent("Gizmo Color", "Màu sắc của Gizmo trong Scene View.");
    private Color simple_gizmoColor = Color.cyan;
    private GUIContent sp_snapToGroundContent = new GUIContent("Snap to Ground", "Tự động đặt các điểm bám vào bề mặt địa hình bên dưới.");
    private bool simple_snapToGround = true;
    private GUIContent sp_groundLayerContent = new GUIContent("Ground Layer", "Chọn layer được coi là mặt đất. Raycast sẽ chỉ va chạm với layer này.");
    private int simple_groundLayerIndex = 3;

    // --- TÍNH NĂNG MỚI ---
    private GUIContent sp_batchCountContent = new GUIContent("Number of Groups", "Tạo ra bao nhiêu group đường đi giống nhau?");
    private int simple_batchCount = 1;
    // ----------------------
    #endregion

    #region Biến cho Tab "Path From Flags" (Chế độ theo cờ)
    // Các biến này được đặt tiền tố 'cf_' (Connect Flags) để phân biệt
    private GUIContent cf_parentGroupContent = new GUIContent("Parent for New Groups", "Tùy chọn: Giữ cho Hierarchy gọn gàng bằng cách đặt các group mới vào trong một đối tượng cha.");
    private Transform connect_parentGroup;
    private GUIContent cf_groupNameContent = new GUIContent("Group Name", "Tên cơ sở cho các group sẽ được tạo.");
    private string connect_groupName = "Infantry";
    private GUIContent cf_basePointNameContent = new GUIContent("Waypoint Name", "Tên cơ sở cho các waypoints (sẽ tự động thêm số thứ tự group và điểm).");
    private string connect_basePointName = "WayPoint";
    
    // --- TÍNH NĂNG MỚI: ATTACK POINTS ---
    private GUIContent cf_attackPointNameContent = new GUIContent("Attack Point Name", "Tên cơ sở cho các attack points.");
    private string connect_attackPointName = "AttackPoint";
    [SerializeField] private List<Transform> connect_attackFlags = new List<Transform>();
    private GUIContent cf_attackFlagsContent = new GUIContent("Attack Flag Points", "Kéo các attack flags từ Scene vào đây theo thứ tự.");
    // ----------------------------------------
    
    private GUIContent cf_routeTypeContent = new GUIContent("Bot Move Type", "Gán một loại chung cho toàn bộ tuyến đường này.");
    private BotMoveType _connectRouteMoveType = BotMoveType.Infantry;
    private GUIContent cf_gizmoColorContent = new GUIContent("Gizmo Color", "Màu sắc của Gizmo cho tuyến đường này.");
    private Color connect_gizmoColor = Color.green;
    [SerializeField] private List<Transform> connect_flagPoints = new List<Transform>();
    private GUIContent cf_flagPointsContent = new GUIContent("Waypoint Flag Points", "Kéo các waypoint flags từ Scene vào đây theo đúng thứ tự bạn muốn.");
    private GUIContent cf_pointsPerSegmentContent = new GUIContent("Points Between Flags", "Số lượng điểm ngẫu nhiên sẽ được chèn vào giữa hai lá cờ liên tiếp.");
    private int connect_pointsPerSegment = 0;
    private GUIContent cf_pathWidthContent = new GUIContent("Path Width/Radius", "Độ rộng của 'hành lang' đường đi, cũng là bán kính ngẫu nhiên xung quanh mỗi điểm cờ.");
    private float connect_pathWidth = 1.0f;
    private GUIContent cf_snapToGroundContent = new GUIContent("Snap to Ground", "Tự động đặt các điểm bám vào bề mặt địa hình.");
    private bool connect_snapToGround = true;
    private GUIContent cf_groundLayerContent = new GUIContent("Ground Layer", "Chọn layer được coi là mặt đất.");
    private int connect_groundLayerIndex = 3;

    // --- TÍNH NĂNG MỚI ---
    private GUIContent cf_batchCountContent = new GUIContent("Number of Groups", "Tạo ra bao nhiêu biến thể đường đi ngẫu nhiên từ các lá cờ này?");
    private int connect_batchCount = 1;
    // ----------------------
    #endregion
    // SerializedObject dùng để hiển thị các danh sách (List<T>) một cách đúng đắn trong EditorWindow.
    private SerializedObject serializedObject;
    private SerializedProperty connect_flagsProperty;
    private SerializedProperty connect_attackFlagsProperty;

    /// <summary>
    /// Tạo một mục trên thanh menu chính của Unity để mở cửa sổ này.
    /// </summary>
    [MenuItem("Tools/Ultimate Path Creator")]
    public static void ShowWindow()
    {
        GetWindow<PathCreatorWindow>("Path Creator");
    }
    
    /// <summary>
    /// Được gọi khi cửa sổ được bật hoặc script được biên dịch lại.
    /// Dùng để khởi tạo các đối tượng cần thiết cho việc vẽ GUI.
    /// </summary>
    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        connect_flagsProperty = serializedObject.FindProperty("connect_flagPoints");
        connect_attackFlagsProperty = serializedObject.FindProperty("connect_attackFlags");
    }
    
    /// <summary>
    /// Hàm chính vẽ toàn bộ giao diện của cửa sổ Editor. Được gọi nhiều lần mỗi giây.
    /// </summary>
    private void OnGUI()
    {
        // Luôn gọi Update() và ApplyModifiedProperties() theo cặp để đảm bảo dữ liệu được đồng bộ.
        serializedObject.Update();

        currentTab = GUILayout.Toolbar(currentTab, new string[] { "Path From Flags", "Simple Path" });
        EditorGUILayout.Space(10);
        
        switch (currentTab)
        {
            case 0: DrawConnectFlagsTab(); break;
            case 1: DrawSimplePathTab(); break;
        }

        serializedObject.ApplyModifiedProperties();
    }
    
    #region Giao diện và Logic của các Tab
    
    /// <summary>
    /// Vẽ giao diện cho Tab "Simple Path".
    /// </summary>
    private void DrawSimplePathTab()
    {
        GUILayout.Label("Create a Straight Line of Points", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // === GROUP SETTINGS ===
        GUILayout.Label("Group Settings", EditorStyles.boldLabel);
        simple_batchCount = EditorGUILayout.IntSlider(sp_batchCountContent, simple_batchCount, 1, 50);
        simple_parentGroup = (Transform)EditorGUILayout.ObjectField(sp_parentGroupContent, simple_parentGroup, typeof(Transform), true);
        
        EditorGUILayout.BeginHorizontal();
        _simpleRouteMoveType = (BotMoveType)EditorGUILayout.EnumPopup(sp_routeTypeContent, _simpleRouteMoveType);
        if (GUILayout.Button("Use as Name", GUILayout.Width(110)))
        {
            simple_groupName = _simpleRouteMoveType.ToString();
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
        
        simple_groupName = EditorGUILayout.TextField(sp_groupNameContent, simple_groupName);
        EditorGUILayout.Space(10);
        
        // === POINT SETTINGS ===
        GUILayout.Label("Point Settings", EditorStyles.boldLabel);
        simple_basePointName = EditorGUILayout.TextField(sp_basePointNameContent, simple_basePointName);
        simple_numberOfPoints = EditorGUILayout.IntSlider(sp_numberOfPointsContent, simple_numberOfPoints, 1, 100);
        simple_initialSpacing = EditorGUILayout.FloatField(sp_initialSpacingContent, simple_initialSpacing);
        EditorGUILayout.Space(10);
        
        // === VISUAL & GROUND SETTINGS ===
        GUILayout.Label("Visual & Ground Settings", EditorStyles.boldLabel);
        simple_gizmoColor = EditorGUILayout.ColorField(sp_gizmoColorContent, simple_gizmoColor);
        simple_snapToGround = EditorGUILayout.Toggle(sp_snapToGroundContent, simple_snapToGround);
        if (simple_snapToGround) simple_groundLayerIndex = EditorGUILayout.LayerField(sp_groundLayerContent, simple_groundLayerIndex);

        EditorGUILayout.Space(20);
        if (GUILayout.Button($"Create {simple_batchCount} Simple Path(s)"))
        {
            Undo.SetCurrentGroupName($"Create {simple_batchCount} Simple Paths");
            int group = Undo.GetCurrentGroup();
            for (int i = 0; i < simple_batchCount; i++)
            {
                CreateSingleSimplePath(i);
            }
            Undo.CollapseUndoOperations(group);
        }
    }

    /// <summary>
    /// Vẽ giao diện cho Tab "Path From Flags".
    /// </summary>
    private void DrawConnectFlagsTab()
    {
        GUILayout.Label("Create a Path by Connecting Random Areas Around Flags", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // === BASIC SETTINGS ===
        GUILayout.Label("Basic Settings", EditorStyles.boldLabel);
        connect_batchCount = EditorGUILayout.IntSlider(cf_batchCountContent, connect_batchCount, 1, 50);
        connect_parentGroup = (Transform)EditorGUILayout.ObjectField(cf_parentGroupContent, connect_parentGroup, typeof(Transform), true);
        
        EditorGUILayout.BeginHorizontal();
        _connectRouteMoveType = (BotMoveType)EditorGUILayout.EnumPopup(cf_routeTypeContent, _connectRouteMoveType);
        if (GUILayout.Button("Use as Name", GUILayout.Width(110)))
        {
            connect_groupName = _connectRouteMoveType.ToString();
            GUI.FocusControl(null); 
        }
        EditorGUILayout.EndHorizontal();
        
        connect_groupName = EditorGUILayout.TextField(cf_groupNameContent, connect_groupName);
        EditorGUILayout.Space(10);
        
        // === FLAG POINTS ===
        GUILayout.Label("Flag Points", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(connect_flagsProperty, cf_flagPointsContent, true);
        EditorGUILayout.PropertyField(connect_attackFlagsProperty, cf_attackFlagsContent, true);
        EditorGUILayout.Space(10);
        
        // === POINT NAMES ===
        GUILayout.Label("Point Names", EditorStyles.boldLabel);
        connect_basePointName = EditorGUILayout.TextField(cf_basePointNameContent, connect_basePointName);
        if (connect_attackFlags.Count > 0)
        {
            connect_attackPointName = EditorGUILayout.TextField(cf_attackPointNameContent, connect_attackPointName);
        }
        EditorGUILayout.Space(10);
        
        // === PATH GENERATION SETTINGS ===
        GUILayout.Label("Path Generation Settings", EditorStyles.boldLabel);
        connect_pointsPerSegment = EditorGUILayout.IntSlider(cf_pointsPerSegmentContent, connect_pointsPerSegment, 0, 20);
        connect_pathWidth = EditorGUILayout.FloatField(cf_pathWidthContent, connect_pathWidth);
        EditorGUILayout.Space(10);
        
        // === GROUND SETTINGS ===
        GUILayout.Label("Ground Settings", EditorStyles.boldLabel);
        connect_snapToGround = EditorGUILayout.Toggle(cf_snapToGroundContent, connect_snapToGround);
        if (connect_snapToGround) connect_groundLayerIndex = EditorGUILayout.LayerField(cf_groundLayerContent, connect_groundLayerIndex);

        EditorGUILayout.Space(20);
        GUI.enabled = connect_flagPoints.Count >= 2 && connect_flagPoints.All(f => f != null);
        
        // Hiển thị thông tin gì sẽ được tạo
        string buttonText = $"Generate {connect_batchCount} Path(s)";
        if (connect_attackFlags.Count > 0 && connect_attackFlags.All(f => f != null))
        {
            buttonText += " (with Attack Points)";
        }
        
        if (GUILayout.Button(buttonText))
        {
            Undo.SetCurrentGroupName($"Generate {connect_batchCount} Flag Paths");
            int group = Undo.GetCurrentGroup();
            for (int i = 0; i < connect_batchCount; i++)
            {
                CreateSinglePathConnectingFlags(i);
            }
            Undo.CollapseUndoOperations(group);
        }
        GUI.enabled = true;
    }
    /// <summary>
    /// Logic chính để tạo MỘT tuyến đường thẳng.
    /// </summary>
    private void CreateSingleSimplePath(int index)
    {
        string currentGroupName = (simple_batchCount > 1) ? $"{simple_groupName}_{index}" : simple_groupName;
        GameObject groupObject = new GameObject(currentGroupName);
        
        // --- NÂNG CẤP: Gán đối tượng cha nếu có ---
        if (simple_parentGroup != null)
        {
            groupObject.transform.SetParent(simple_parentGroup, false); // false để giữ world position
        }
        // ----------------------------------------

        Undo.RegisterCreatedObjectUndo(groupObject, "Create Simple Path");
        PointGroup pointGroup = ConfigurePointGroup(groupObject, _simpleRouteMoveType, simple_basePointName, simple_gizmoColor);

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null) { Debug.LogError("Please open a Scene View."); return; }
        
        Vector3 currentPosition = sceneView.camera.transform.position + sceneView.camera.transform.forward * 20f;
        LayerMask groundMask = (1 << simple_groundLayerIndex);

        for (int i = 0; i < simple_numberOfPoints; i++)
        {
            if (i > 0) currentPosition += sceneView.camera.transform.right * simple_initialSpacing;
            CreatePoint(currentPosition, $"{simple_basePointName}_{i}", groupObject.transform, groundMask, simple_snapToGround);
        }
        FinalizeGroup(groupObject, pointGroup);
    }
    
     /// <summary>
    /// Logic chính để tạo MỘT tuyến đường nối các khu vực ngẫu nhiên quanh các điểm cờ.
    /// Giờ hỗ trợ cả waypoints và attack points.
    /// </summary>
    /// <param name="index">Chỉ số của group đang được tạo (dùng để đánh số).</param>
    private void CreateSinglePathConnectingFlags(int index)
    {
        string currentGroupName = (connect_batchCount > 1) ? $"{connect_groupName}_{index}" : connect_groupName;
        GameObject groupObject = new GameObject(currentGroupName);

        if (connect_parentGroup != null)
        {
            groupObject.transform.SetParent(connect_parentGroup, false); 
        }

        Undo.RegisterCreatedObjectUndo(groupObject, "Create Path Connecting Flags");
        PointGroup pointGroup = ConfigurePointGroup(groupObject, _connectRouteMoveType, connect_basePointName, connect_gizmoColor);
        
        List<Vector3> createdPositions = new List<Vector3>();
        LayerMask groundMask = (1 << connect_groundLayerIndex);

        // Tạo waypoints từ waypoint flags
        for (int i = 0; i < connect_flagPoints.Count - 1; i++)
        {
            if (connect_flagPoints[i] == null || connect_flagPoints[i+1] == null) continue;
            
            Transform startFlag = connect_flagPoints[i];
            Transform endFlag = connect_flagPoints[i+1];
            
            Vector3 startPoint = GetRandomPointAroundFlag(startFlag.position, connect_pathWidth);
            Vector3 endPoint = GetRandomPointAroundFlag(endFlag.position, connect_pathWidth);

            if (i == 0) AddFinalPosition(startPoint, createdPositions, groundMask, connect_snapToGround);
           
            for (int j = 0; j < connect_pointsPerSegment; j++)
            {
                float t = (float)(j + 1) / (connect_pointsPerSegment + 1);
                Vector3 idealPositionOnLine = Vector3.Lerp(startPoint, endPoint, t);
                Vector3 directionOfSegment = (endPoint - startPoint).normalized;
                Vector3 sideDirection = Vector3.Cross(directionOfSegment, Vector3.up).normalized;
                float randomSideOffset = Random.Range(-connect_pathWidth / 4, connect_pathWidth / 4);
                Vector3 finalPosition = idealPositionOnLine + (sideDirection * randomSideOffset);
                AddFinalPosition(finalPosition, createdPositions, groundMask, connect_snapToGround);
            }
            
            AddFinalPosition(endPoint, createdPositions, groundMask, connect_snapToGround);
        }
        
        // Tạo waypoints
        for (int i = 0; i < createdPositions.Count; i++)
        {
            string pointName = GetPointName(connect_basePointName, index, i);
            CreatePoint(createdPositions[i], pointName, groupObject.transform, groundMask, false);
        }
        
        // Tạo attack points (nếu có attack flags)
        if (connect_attackFlags.Count > 0 && connect_attackFlags.All(f => f != null))
        {
            List<Vector3> attackPositions = new List<Vector3>();
            
            // Bắt đầu từ vị trí cuối của waypoints
            Vector3 lastWaypointPos = createdPositions.Count > 0 ? createdPositions[createdPositions.Count - 1] : Vector3.zero;
            
            for (int i = 0; i < connect_attackFlags.Count - 1; i++)
            {
                if (connect_attackFlags[i] == null || connect_attackFlags[i+1] == null) continue;
                
                Transform startFlag = connect_attackFlags[i];
                Transform endFlag = connect_attackFlags[i+1];
                
                Vector3 startPoint, endPoint;
                
                if (i == 0 && createdPositions.Count > 0)
                {
                    // Điểm attack đầu tiên = vị trí waypoint cuối
                    startPoint = lastWaypointPos;
                }
                else
                {
                    startPoint = GetRandomPointAroundFlag(startFlag.position, connect_pathWidth);
                }
                
                endPoint = GetRandomPointAroundFlag(endFlag.position, connect_pathWidth);

                if (i == 0) AddFinalPosition(startPoint, attackPositions, groundMask, connect_snapToGround);
               
                for (int j = 0; j < connect_pointsPerSegment; j++)
                {
                    float t = (float)(j + 1) / (connect_pointsPerSegment + 1);
                    Vector3 idealPositionOnLine = Vector3.Lerp(startPoint, endPoint, t);
                    Vector3 directionOfSegment = (endPoint - startPoint).normalized;
                    Vector3 sideDirection = Vector3.Cross(directionOfSegment, Vector3.up).normalized;
                    float randomSideOffset = Random.Range(-connect_pathWidth / 4, connect_pathWidth / 4);
                    Vector3 finalPosition = idealPositionOnLine + (sideDirection * randomSideOffset);
                    AddFinalPosition(finalPosition, attackPositions, groundMask, connect_snapToGround);
                }
                
                AddFinalPosition(endPoint, attackPositions, groundMask, connect_snapToGround);
            }
            
            // Tạo attack points
            for (int i = 0; i < attackPositions.Count; i++)
            {
                string attackPointName = GetPointName(connect_attackPointName, index, i);
                CreatePoint(attackPositions[i], attackPointName, groupObject.transform, groundMask, false);
            }
            
            Debug.Log($"Created {createdPositions.Count} waypoints and {attackPositions.Count} attack points for '{currentGroupName}'");
        }
        else
        {
            Debug.Log($"Created {createdPositions.Count} waypoints for '{currentGroupName}'");
        }
        
        FinalizeGroup(groupObject, pointGroup);
    }
    
    /// <summary>
    /// Tạo tên điểm dựa trên base name, group index và point index
    /// </summary>
    private string GetPointName(string baseName, int groupIndex, int pointIndex)
    {
        if (connect_batchCount > 1)
        {
            return $"{baseName}_{groupIndex}_{pointIndex}";
        }
        else
        {
            return $"{baseName}_{pointIndex}";
        }
    }

    #endregion
    
    #region Các hàm trợ giúp (Helper Methods)

    /// <summary>
    /// Cấu hình các giá trị ban đầu cho component PointGroup trên GameObject mới.
    /// </summary>
    private PointGroup ConfigurePointGroup(GameObject go, BotMoveType moveType, string baseName, Color color)
    {
        PointGroup pg = go.AddComponent<PointGroup>();
        pg.botMoveType = moveType;
        pg.baseName = baseName;
        pg.lineColor = color;
        return pg;
    }
    
    /// <summary>
    /// Tạo một điểm con, bám đất nếu cần, và gán nó vào cha.
    /// </summary>
    private GameObject CreatePoint(Vector3 position, string name, Transform parent, LayerMask groundMask, bool snap)
    {
        if (snap) position = SnapToGround(position, groundMask);
        GameObject pointObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(pointObject, "Create Point");
        pointObject.transform.position = position;
        pointObject.transform.SetParent(parent);
        return pointObject;
    }
    
    /// <summary>
    /// Lấy một vị trí Vector3 ngẫu nhiên trong một bán kính xung quanh vị trí cờ.
    /// </summary>
    private Vector3 GetRandomPointAroundFlag(Vector3 flagPosition, float radius)
    {
        // Random.insideUnitCircle trả về một điểm ngẫu nhiên trong một vòng tròn có bán kính 1.
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        // Áp dụng offset ngẫu nhiên này trên mặt phẳng XZ.
        return flagPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
    }
    
    /// <summary>
    /// Bám một vị trí xuống đất và thêm nó vào danh sách các vị trí sẽ được tạo.
    /// </summary>
    private void AddFinalPosition(Vector3 position, List<Vector3> positionList, LayerMask groundMask, bool snap)
    {
        if (snap) position = SnapToGround(position, groundMask);
        positionList.Add(position);
    }
    
    /// <summary>
    /// Bắn một tia raycast từ trên cao xuống để tìm vị trí mặt đất.
    /// </summary>
    /// <returns>Vị trí va chạm trên mặt đất, hoặc vị trí ban đầu nếu không tìm thấy.</returns>
    private Vector3 SnapToGround(Vector3 position, LayerMask groundMask)
    {
        RaycastHit hit;
        // Bắn một tia từ 50 đơn vị phía trên vị trí hiện tại, thẳng xuống dưới.
        if (Physics.Raycast(position + Vector3.up * 50f, Vector3.down, out hit, 100f, groundMask))
        {
            // Trả về điểm va chạm chính xác.
            return hit.point;
        }
        // Nếu không trúng gì, trả về vị trí gốc.
        return position;
    }
    
    /// <summary>
    /// Hoàn tất quá trình tạo group: cập nhật danh sách điểm và chọn đối tượng trong scene.
    /// </summary>
    private void FinalizeGroup(GameObject groupObject, PointGroup pointGroup)
    {
        pointGroup.UpdatePoints();
        Selection.activeGameObject = groupObject;
        Debug.Log($"Successfully created '{groupObject.name}' with {pointGroup.points.Count} points.");
    }

    #endregion
}
#endif