using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Một công cụ Editor nâng cao để sao chép và ánh xạ các component Collider từ một Prefab nguồn sang một Prefab đích.
/// Công cụ này sử dụng nhiều thuật toán để tự động tìm các đối tượng tương ứng giữa hai Prefab.
/// </summary>
public class AdvancedColliderCloneTool : EditorWindow
{
    //================================================================================
    // Fields - Các biến cấu hình trên giao diện
    //================================================================================

    [Header("Source và Target Prefabs")]
    [Tooltip("Prefab nguồn chứa các colliders gốc cần được sao chép.")]
    public GameObject sourcePrefab;

    [Tooltip("Prefab đích sẽ nhận các colliders được sao chép.")]
    public GameObject targetPrefab;

    [Header("Auto Mapping Options")]
    [Tooltip("Nếu được chọn, công cụ sẽ ưu tiên tìm các đối tượng ở Target có tên giống hệt hoặc tương tự với đối tượng ở Source.")]
    public bool matchByName = true;

    [Tooltip("Nếu được chọn, công cụ sẽ so sánh vị trí tương đối của các đối tượng con để tìm cặp khớp.")]
    public bool matchByPosition = true;

    [Tooltip("Nếu được chọn, công cụ sẽ so sánh cấu trúc cây phân cấp (độ sâu, tên cha) để tìm sự tương đồng.")]
    public bool matchByHierarchy = true;

    [Tooltip("Nếu được chọn, công cụ sẽ so sánh các component khác (ngoài Transform) có trên GameObject để tìm sự tương đồng.")]
    public bool matchByComponent = false;

    [Tooltip("Ngưỡng sai số khoảng cách cho phép khi khớp nối bằng vị trí. Chỉ có tác dụng khi 'Match By Position' được bật.")]
    public float positionTolerance = 0.1f;

    [Header("General Options")]
    [Tooltip("Nếu được chọn, công cụ sẽ tìm kiếm colliders trong tất cả các đối tượng con. Nếu không, chỉ tìm ở đối tượng gốc.")]
    public bool includeChildren = true;

    [Tooltip("Nếu được chọn, công cụ sẽ xóa collider đã có trên đối tượng đích trước khi sao chép collider mới. Nếu không, các đối tượng đã có collider sẽ bị bỏ qua.")]
    public bool overwriteExisting = false;

    [Tooltip("Một bộ lọc giao diện, chỉ hiển thị các ánh xạ có vấn đề (vàng, đỏ) hoặc chưa tìm thấy đối tượng đích.")]
    public bool showOnlyUnmapped = false;

    // Biến private để lưu trạng thái của cửa sổ
    private Vector2 scrollPosition;
    private List<ColliderMapping> mappings = new List<ColliderMapping>();
    private Dictionary<string, GameObject> sourceObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> targetObjects = new Dictionary<string, GameObject>();
    
    // Khai báo GUIContent để tái sử dụng và tránh tạo mới trong OnGUI
    private readonly GUIContent sourcePrefabContent = new GUIContent("Source Prefab (có colliders)", "Kéo Prefab hoặc GameObject chứa các colliders bạn muốn sao chép vào đây.");
    private readonly GUIContent targetPrefabContent = new GUIContent("Target Prefab (chưa có colliders)", "Kéo Prefab hoặc GameObject sẽ nhận các colliders vào đây.");
    private readonly GUIContent matchByNameContent = new GUIContent("Match by Name", "Tìm các đối tượng có tên giống hệt hoặc tương tự.");
    private readonly GUIContent matchByPosContent = new GUIContent("Match by Position", "Tìm các đối tượng có vị trí tương đối gần giống nhau.");
    private readonly GUIContent matchByHierarchyContent = new GUIContent("Match by Hierarchy", "Tìm các đối tượng có cùng cấu trúc cha-con.");
    private readonly GUIContent matchByCompContent = new GUIContent("Match by Components", "Tìm các đối tượng có cùng bộ component (ví dụ: cùng có MeshRenderer).");
    private readonly GUIContent posToleranceContent = new GUIContent("Position Tolerance", "Khoảng cách tối đa cho phép để coi hai vị trí là khớp nhau.");
    private readonly GUIContent includeChildrenContent = new GUIContent("Include Children", "Tìm kiếm colliders trên tất cả các đối tượng con, không chỉ đối tượng gốc.");
    private readonly GUIContent overwriteExistingContent = new GUIContent("Overwrite Existing", "Nếu được chọn, sẽ thay thế các colliders đã có trên đối tượng đích.");
    private readonly GUIContent showUnmappedContent = new GUIContent("Show Only Unmapped/Problems", "Chỉ hiển thị các ánh xạ chưa tìm thấy hoặc có cảnh báo.");


    //================================================================================
    // Data Structures - Các cấu trúc dữ liệu
    //================================================================================

    /// <summary>
    /// Đại diện cho một liên kết (ánh xạ) giữa một collider ở nguồn và một GameObject tiềm năng ở đích.
    /// Đây là cấu trúc dữ liệu trung tâm của công cụ.
    /// </summary>
    [System.Serializable]
    public class ColliderMapping
    {
        public string sourcePath;
        public string targetPath;
        public Collider sourceCollider;
        public GameObject sourceObject;
        public GameObject targetObject;
        public bool willCopy = true;
        public string status = "";
        public MatchType matchType;
        public float confidence = 0f;
        public bool manualOverride = false;
        public GameObject manualTargetObject;
    }

    /// <summary>
    /// Định nghĩa các phương pháp mà thuật toán có thể sử dụng để tìm ra một cặp khớp nối.
    /// </summary>
    public enum MatchType
    {
        ExactName,
        SimilarName,
        Position,
        Hierarchy,
        Component,
        Manual,
        NotFound
    }

    //================================================================================
    // Editor Window Logic - Logic của cửa sổ Editor
    //================================================================================

    /// <summary>
    /// Mở cửa sổ công cụ từ menu 'Tools/Advanced Collider Clone Tool'.
    /// </summary>
    [MenuItem("Tools/Advanced Collider Clone Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<AdvancedColliderCloneTool>("Advanced Collider Clone Tool");
        window.minSize = new Vector2(600, 400);
    }

    /// <summary>
    /// Được Unity gọi liên tục để vẽ giao diện người dùng cho cửa sổ editor.
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Advanced Collider Clone Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawPrefabSelection();
        DrawAutoMappingOptions();
        DrawGeneralOptions();
        EditorGUILayout.Space();
        DrawActionButtons();

        if (mappings.Count > 0)
        {
            DrawMappingResults();
        }
    }

    /// <summary>
    /// Vẽ phần giao diện để chọn Prefab nguồn và đích.
    /// </summary>
    void DrawPrefabSelection()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Prefab Selection", EditorStyles.boldLabel);

        var newSourcePrefab = (GameObject)EditorGUILayout.ObjectField(sourcePrefabContent, sourcePrefab, typeof(GameObject), false);
        var newTargetPrefab = (GameObject)EditorGUILayout.ObjectField(targetPrefabContent, targetPrefab, typeof(GameObject), false);

        if (newSourcePrefab != sourcePrefab || newTargetPrefab != targetPrefab)
        {
            sourcePrefab = newSourcePrefab;
            targetPrefab = newTargetPrefab;
            if (sourcePrefab != null && targetPrefab != null)
            {
                AutoAnalyzeMappings();
            }
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Vẽ các tùy chọn cho thuật toán tự động ánh xạ.
    /// </summary>
    void DrawAutoMappingOptions()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Auto Mapping Strategy", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        matchByName = EditorGUILayout.Toggle(matchByNameContent, matchByName);
        matchByPosition = EditorGUILayout.Toggle(matchByPosContent, matchByPosition);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        matchByHierarchy = EditorGUILayout.Toggle(matchByHierarchyContent, matchByHierarchy);
        matchByComponent = EditorGUILayout.Toggle(matchByCompContent, matchByComponent);
        EditorGUILayout.EndHorizontal();

        if (matchByPosition)
        {
            positionTolerance = EditorGUILayout.Slider(posToleranceContent, positionTolerance, 0.01f, 5f);
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Vẽ các tùy chọn chung cho quá trình sao chép.
    /// </summary>
    void DrawGeneralOptions()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("General Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        includeChildren = EditorGUILayout.Toggle(includeChildrenContent, includeChildren);
        overwriteExisting = EditorGUILayout.Toggle(overwriteExistingContent, overwriteExisting);
        EditorGUILayout.EndHorizontal();
        showOnlyUnmapped = EditorGUILayout.Toggle(showUnmappedContent, showOnlyUnmapped);
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Vẽ các nút hành động chính (Analyze, Clone, Reset).
    /// </summary>
    void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = sourcePrefab != null && targetPrefab != null;
        
        // Tạo GUIContent cho các nút
        var analyzeContent = new GUIContent("🔍 Auto Analyze", "Chạy lại thuật toán phân tích từ đầu, dựa trên các cài đặt hiện tại.");
        var smartMatchContent = new GUIContent("🎯 Smart Match", "Cố gắng tìm kiếm lại các ánh xạ chưa thành công hoặc có độ tin cậy thấp.");
        var cloneContent = new GUIContent("📋 Clone Colliders", "Thực hiện sao chép các colliders dựa trên các ánh xạ hợp lệ đã được chọn.");
        var resetContent = new GUIContent("🔄 Reset All", "Xóa tất cả các ánh xạ hiện tại và các thay đổi thủ công, sau đó chạy lại phân tích.");

        if (GUILayout.Button(analyzeContent, GUILayout.Height(30))) AutoAnalyzeMappings();
        if (GUILayout.Button(smartMatchContent, GUILayout.Height(30))) SmartMatch();
        
        GUI.enabled = mappings.Count > 0;
        if (GUILayout.Button(cloneContent, GUILayout.Height(30))) CloneColliders();
        if (GUILayout.Button(resetContent, GUILayout.Height(30))) ResetMappings();
        
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Vẽ bảng kết quả các ánh xạ đã tìm thấy.
    /// </summary>
    void DrawMappingResults()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        var validMappings = showOnlyUnmapped ? mappings.Where(m => m.targetObject == null || m.matchType == MatchType.NotFound).ToList() : mappings;
        GUILayout.Label($"Mapping Results ({validMappings.Count()}/{mappings.Count} items)", EditorStyles.boldLabel);
        var stats = GetMappingStats();
        EditorGUILayout.LabelField($"✓ Ready: {stats.ready} | ⚠ Issues: {stats.issues} | ✗ Missing: {stats.missing}", EditorStyles.miniLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(400));
        foreach (var mapping in validMappings)
        {
            DrawMappingItem(mapping);
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// Vẽ một mục ánh xạ đơn lẻ trong danh sách kết quả.
    /// </summary>
    void DrawMappingItem(ColliderMapping mapping)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        mapping.willCopy = EditorGUILayout.Toggle(mapping.willCopy, GUILayout.Width(20));
        Color originalColor = GUI.color;
        if (!mapping.willCopy) GUI.color = Color.gray;
        else if (mapping.status.Contains("✓")) GUI.color = Color.green;
        else if (mapping.status.Contains("⚠")) GUI.color = Color.yellow;
        else if (mapping.status.Contains("✗")) GUI.color = Color.red;
        EditorGUILayout.LabelField($"[{mapping.matchType}] {GetObjectName(mapping.sourcePath)}", EditorStyles.boldLabel);
        GUI.color = originalColor;
        if (mapping.confidence > 0)
        {
            EditorGUILayout.LabelField($"{mapping.confidence:P0}", GUILayout.Width(40));
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Source:", GUILayout.Width(60));
        EditorGUILayout.LabelField(mapping.sourcePath, EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Target:", GUILayout.Width(60));
        if (!mapping.manualOverride && mapping.targetObject != null)
        {
            EditorGUILayout.LabelField(mapping.targetPath, EditorStyles.miniLabel);
            if (GUILayout.Button(new GUIContent("📝", "Chỉnh sửa thủ công ánh xạ này."), GUILayout.Width(25)))
            {
                mapping.manualOverride = true;
                mapping.manualTargetObject = mapping.targetObject;
            }
        }
        else
        {
            var newTarget = (GameObject)EditorGUILayout.ObjectField(mapping.manualTargetObject, typeof(GameObject), true, GUILayout.ExpandWidth(true));
            if (newTarget != mapping.manualTargetObject)
            {
                mapping.manualTargetObject = newTarget;
                mapping.targetObject = newTarget;
                mapping.manualOverride = true;
                mapping.matchType = MatchType.Manual;
                if (newTarget != null)
                {
                    if (IsObjectInPrefab(newTarget, targetPrefab))
                    {
                        mapping.targetPath = GetGameObjectPath(newTarget, targetPrefab);
                    }
                    else
                    {
                        mapping.targetPath = $"[Scene] {newTarget.name}";
                    }
                    UpdateMappingStatus(mapping);
                }
            }
            if (GUILayout.Button(new GUIContent("🔄", "Quay lại ánh xạ tự động."), GUILayout.Width(25)))
            {
                mapping.manualOverride = false;
                AutoFindBestMatch(mapping);
            }
        }
        EditorGUILayout.EndHorizontal();
        if (!string.IsNullOrEmpty(mapping.status))
        {
            EditorGUILayout.LabelField($"Status: {mapping.status}", EditorStyles.miniLabel);
        }
        if (mapping.sourceCollider != null)
        {
            EditorGUILayout.LabelField($"Collider: {mapping.sourceCollider.GetType().Name}", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();
    }
    
    //================================================================================
    // Core Logic - Các thuật toán và logic chính
    //================================================================================

    /// <summary>
    /// Chạy quy trình phân tích tự động. Hàm này thu thập dữ liệu từ các Prefab và cố gắng tìm các cặp khớp nối.
    /// </summary>
    void AutoAnalyzeMappings()
    {
        mappings.Clear();
        if (sourcePrefab == null || targetPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Vui lòng chọn cả Source và Target prefab!", "OK");
            return;
        }

        sourceObjects = GetAllGameObjectsWithPath(sourcePrefab);
        targetObjects = GetAllGameObjectsWithPath(targetPrefab);
        var sourceColliders = GetAllCollidersWithPath(sourcePrefab);

        foreach (var sourceItem in sourceColliders)
        {
            var mapping = new ColliderMapping
            {
                sourcePath = sourceItem.Key,
                sourceCollider = sourceItem.Value,
                sourceObject = sourceObjects[sourceItem.Key]
            };
            AutoFindBestMatch(mapping);
            mappings.Add(mapping);
        }
        Debug.Log($"Auto analysis complete. Found {mappings.Count} colliders with {mappings.Count(m => m.targetObject != null)} matches.");
    }

    /// <summary>
    /// Tìm đối tượng đích phù hợp nhất cho một ánh xạ nguồn bằng cách sử dụng các chiến lược đã chọn.
    /// </summary>
    void AutoFindBestMatch(ColliderMapping mapping)
    {
        var candidates = new List<(GameObject obj, string path, MatchType type, float confidence)>();
        foreach (var targetItem in targetObjects)
        {
            var targetObj = targetItem.Value;
            var targetPath = targetItem.Key;
            if (targetObj == targetPrefab) continue;

            if (matchByName)
            {
                var sourceName = GetObjectName(mapping.sourcePath);
                var targetName = GetObjectName(targetPath);
                if (sourceName == targetName) candidates.Add((targetObj, targetPath, MatchType.ExactName, 1.0f));
                else if (IsNameSimilar(sourceName, targetName)) candidates.Add((targetObj, targetPath, MatchType.SimilarName, 0.7f));
            }
            if (matchByPosition && mapping.sourceObject != null)
            {
                var distance = Vector3.Distance(mapping.sourceObject.transform.position, targetObj.transform.position);
                if (distance <= positionTolerance)
                {
                    var confidence = 1.0f - (distance / positionTolerance);
                    candidates.Add((targetObj, targetPath, MatchType.Position, confidence * 0.8f));
                }
            }
            if (matchByHierarchy)
            {
                var sourceDepth = mapping.sourcePath.Split('/').Length;
                var targetDepth = targetPath.Split('/').Length;
                if (sourceDepth == targetDepth)
                {
                    var hierarchyMatch = GetHierarchyMatchScore(mapping.sourcePath, targetPath);
                    if (hierarchyMatch > 0.5f) candidates.Add((targetObj, targetPath, MatchType.Hierarchy, hierarchyMatch * 0.6f));
                }
            }
            if (matchByComponent && mapping.sourceObject != null)
            {
                var componentMatch = GetComponentMatchScore(mapping.sourceObject, targetObj);
                if (componentMatch > 0.3f) candidates.Add((targetObj, targetPath, MatchType.Component, componentMatch * 0.5f));
            }
        }

        if (candidates.Count > 0)
        {
            var bestMatch = candidates.OrderByDescending(c => c.confidence).First();
            mapping.targetObject = bestMatch.obj;
            mapping.targetPath = bestMatch.path;
            mapping.matchType = bestMatch.type;
            mapping.confidence = bestMatch.confidence;
        }
        else
        {
            mapping.matchType = MatchType.NotFound;
            mapping.targetPath = "NOT FOUND";
        }
        UpdateMappingStatus(mapping);
    }
    
    /// <summary>
    /// Cập nhật chuỗi trạng thái và cờ 'willCopy' cho một ánh xạ dựa trên kết quả tìm kiếm.
    /// </summary>
    void UpdateMappingStatus(ColliderMapping mapping)
    {
        if (mapping.targetObject == null)
        {
            mapping.status = "✗ No target found";
            mapping.willCopy = false;
            return;
        }
        var existingCollider = mapping.targetObject.GetComponent<Collider>();
        if (existingCollider != null)
        {
            if (overwriteExisting) mapping.status = "⚠ Will overwrite existing collider";
            else
            {
                mapping.status = "✗ Has existing collider (skipped)";
                mapping.willCopy = false;
            }
        }
        else
        {
            mapping.status = "✓ Ready to copy";
        }
    }
    
    /// <summary>
    /// Chạy lại thuật toán tìm kiếm trên các ánh xạ chưa thành công.
    /// </summary>
    void SmartMatch()
    {
        foreach (var mapping in mappings.Where(m => m.targetObject == null || m.matchType == MatchType.NotFound))
        {
            AutoFindBestMatch(mapping);
        }
        var improvedCount = mappings.Count(m => m.targetObject != null && m.status.Contains("✓"));
        EditorUtility.DisplayDialog("Smart Match Complete", $"Improved {improvedCount} mappings", "OK");
    }

    /// <summary>
    /// Đặt lại tất cả các ánh xạ về trạng thái ban đầu và chạy lại phân tích.
    /// </summary>
    void ResetMappings()
    {
        foreach (var mapping in mappings)
        {
            mapping.manualOverride = false;
            mapping.manualTargetObject = null;
        }
        AutoAnalyzeMappings();
    }
    
    /// <summary>
    /// Thực hiện quá trình sao chép các colliders dựa trên các ánh xạ đã được duyệt.
    /// </summary>
    void CloneColliders()
    {
        if (mappings.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "Không có mapping nào. Vui lòng chạy Analyze trước!", "OK");
            return;
        }
        var validMappings = mappings.Where(m => m.willCopy && m.targetObject != null).ToList();
        if (validMappings.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "Không có mapping nào sẵn sàng để copy!", "OK");
            return;
        }

        int successCount = 0;
        int skipCount = 0;
        int directCount = 0;
        
        var prefabMappings = new List<ColliderMapping>();
        var directMappings = new List<ColliderMapping>();
        
        foreach (var mapping in validMappings)
        {
            if (IsObjectInPrefab(mapping.targetObject, targetPrefab)) prefabMappings.Add(mapping);
            else directMappings.Add(mapping);
        }
        
        foreach (var mapping in directMappings)
        {
            var existingCollider = mapping.targetObject.GetComponent<Collider>();
            if (existingCollider != null && !overwriteExisting) { skipCount++; continue; }
            if (existingCollider != null && overwriteExisting) DestroyImmediate(existingCollider);
            if (CopyCollider(mapping.sourceCollider, mapping.targetObject)) { directCount++; successCount++; }
        }
        
        GameObject targetInstance = null;
        if (prefabMappings.Count > 0)
        {
            targetInstance = PrefabUtility.InstantiatePrefab(targetPrefab) as GameObject;
        }
        
        try
        {
            foreach (var mapping in prefabMappings)
            {
                var targetObj = FindObjectInHierarchy(targetInstance, mapping.targetPath);
                if (targetObj == null)
                {
                    Debug.LogWarning($"Could not find target object in prefab: {mapping.targetPath}");
                    continue;
                }
                var existingCollider = targetObj.GetComponent<Collider>();
                if (existingCollider != null && !overwriteExisting) { skipCount++; continue; }
                if (existingCollider != null && overwriteExisting) DestroyImmediate(existingCollider);
                if (CopyCollider(mapping.sourceCollider, targetObj)) successCount++;
            }
            if (targetInstance != null)
            {
                PrefabUtility.ApplyPrefabInstance(targetInstance, InteractionMode.UserAction);
            }
            string message = $"Collider cloning complete!\nSuccess: {successCount}\nSkipped: {skipCount}";
            if (directCount > 0) message += $"\nDirect objects: {directCount}";
            EditorUtility.DisplayDialog("Complete", message, "OK");
        }
        finally
        {
            if (targetInstance != null)
            {
                DestroyImmediate(targetInstance);
            }
        }
        AutoAnalyzeMappings();
    }
    
    //================================================================================
    // Helper Methods - Các hàm hỗ trợ
    //================================================================================
    
    /// <summary>
    /// Kiểm tra xem một GameObject có phải là một phần của một Prefab cụ thể không.
    /// </summary>
    bool IsObjectInPrefab(GameObject obj, GameObject prefab)
    {
        if (obj == null || prefab == null) return false;
        if (obj == prefab) return true;
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.gameObject == prefab) return true;
            current = current.parent;
        }
        var objPrefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
        var targetPrefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
        if (objPrefab != null && targetPrefabAsset != null)
        {
            return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(objPrefab) == 
                   PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(targetPrefabAsset);
        }
        return false;
    }

    /// <summary>
    /// Kiểm tra sự tương đồng giữa hai chuỗi tên.
    /// </summary>
    bool IsNameSimilar(string name1, string name2)
    {
        name1 = name1.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
        name2 = name2.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
        if (name1.Contains(name2) || name2.Contains(name1)) return true;
        var distance = LevenshteinDistance(name1, name2);
        var maxLen = Mathf.Max(name1.Length, name2.Length);
        return maxLen > 0 ? (float)(maxLen - distance) / maxLen > 0.6f : false;
    }
    
    /// <summary>
    /// Tính điểm tương đồng về cấu trúc phân cấp dựa trên đường dẫn.
    /// </summary>
    float GetHierarchyMatchScore(string path1, string path2)
    {
        var parts1 = path1.Split('/');
        var parts2 = path2.Split('/');
        int matches = 0;
        int minLength = Mathf.Min(parts1.Length, parts2.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (parts1[i] == parts2[i]) matches++;
        }
        return minLength > 0 ? (float)matches / minLength : 0f;
    }

    /// <summary>
    /// Tính điểm tương đồng dựa trên các component có trên hai GameObject.
    /// </summary>
    float GetComponentMatchScore(GameObject obj1, GameObject obj2)
    {
        var components1 = obj1.GetComponents<Component>().Select(c => c.GetType()).ToHashSet();
        var components2 = obj2.GetComponents<Component>().Select(c => c.GetType()).ToHashSet();
        var intersection = components1.Intersect(components2).Count();
        var union = components1.Union(components2).Count();
        return union > 0 ? (float)intersection / union : 0f;
    }
    
    /// <summary>
    /// Tính toán khoảng cách Levenshtein giữa hai chuỗi.
    /// </summary>
    int LevenshteinDistance(string s1, string s2)
    {
        var matrix = new int[s1.Length + 1, s2.Length + 1];
        for (int i = 0; i <= s1.Length; i++) matrix[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++) matrix[0, j] = j;
        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Mathf.Min(matrix[i - 1, j] + 1, Mathf.Min(matrix[i, j - 1] + 1, matrix[i - 1, j - 1] + cost));
            }
        }
        return matrix[s1.Length, s2.Length];
    }
    
    /// <summary>
    /// Lấy thống kê về số lượng ánh xạ sẵn sàng, có vấn đề, và bị thiếu.
    /// </summary>
    (int ready, int issues, int missing) GetMappingStats()
    {
        int ready = mappings.Count(m => m.status.Contains("✓"));
        int issues = mappings.Count(m => m.status.Contains("⚠"));
        int missing = mappings.Count(m => m.status.Contains("✗"));
        return (ready, issues, missing);
    }
    
    /// <summary>
    /// Lấy tất cả các component Collider và đường dẫn của chúng từ một GameObject gốc.
    /// </summary>
    Dictionary<string, Collider> GetAllCollidersWithPath(GameObject root)
    {
        var result = new Dictionary<string, Collider>();
        GetCollidersRecursive(root, "", result);
        return result;
    }

    /// <summary>
    /// Hàm đệ quy để thu thập các colliders.
    /// </summary>
    void GetCollidersRecursive(GameObject obj, string path, Dictionary<string, Collider> result)
    {
        string currentPath = string.IsNullOrEmpty(path) ? obj.name : $"{path}/{obj.name}";
        var collider = obj.GetComponent<Collider>();
        if (collider != null) result[currentPath] = collider;
        if (includeChildren)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GetCollidersRecursive(obj.transform.GetChild(i).gameObject, currentPath, result);
            }
        }
    }
    
    /// <summary>
    /// Lấy tất cả các GameObject con và đường dẫn của chúng từ một GameObject gốc.
    /// </summary>
    Dictionary<string, GameObject> GetAllGameObjectsWithPath(GameObject root)
    {
        var result = new Dictionary<string, GameObject>();
        GetGameObjectsRecursive(root, "", result);
        return result;
    }

    /// <summary>
    /// Hàm đệ quy để thu thập các GameObjects.
    /// </summary>
    void GetGameObjectsRecursive(GameObject obj, string path, Dictionary<string, GameObject> result)
    {
        string currentPath = string.IsNullOrEmpty(path) ? obj.name : $"{path}/{obj.name}";
        result[currentPath] = obj;
        if (includeChildren)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GetGameObjectsRecursive(obj.transform.GetChild(i).gameObject, currentPath, result);
            }
        }
    }
    
    /// <summary>
    /// Trích xuất tên của đối tượng từ một chuỗi đường dẫn đầy đủ.
    /// </summary>
    string GetObjectName(string path) => path.Split('/').Last();
    
    /// <summary>
    /// Xây dựng chuỗi đường dẫn phân cấp cho một GameObject, tính từ một đối tượng gốc.
    /// </summary>
    string GetGameObjectPath(GameObject obj, GameObject root)
    {
        if (obj == root) return root.name;
        var path = new List<string>();
        var current = obj.transform;
        while (current != null && current.gameObject != root)
        {
            path.Add(current.name);
            current = current.parent;
        }
        if (current != null)
        {
            path.Add(root.name);
            path.Reverse();
            return string.Join("/", path);
        }
        return obj.name;
    }
    
    /// <summary>
    /// Tìm một GameObject con trong một cây phân cấp dựa trên chuỗi đường dẫn.
    /// </summary>
    GameObject FindObjectInHierarchy(GameObject root, string path)
    {
        var pathParts = path.Split('/');
        GameObject current = root;
        for (int i = 0; i < pathParts.Length; i++)
        {
            if (i == 0 && pathParts[i] == current.name) continue;
            Transform found = null;
            for (int j = 0; j < current.transform.childCount; j++)
            {
                if (current.transform.GetChild(j).name == pathParts[i])
                {
                    found = current.transform.GetChild(j);
                    break;
                }
            }
            if (found == null) return null;
            current = found.gameObject;
        }
        return current;
    }
    
    /// <summary>
    /// Sao chép các thuộc tính từ một collider nguồn sang một component collider mới trên đối tượng đích.
    /// </summary>
    bool CopyCollider(Collider sourceCollider, GameObject targetObject)
    {
        try
        {
            if (sourceCollider is BoxCollider boxSource)
            {
                var boxTarget = targetObject.AddComponent<BoxCollider>();
                boxTarget.center = boxSource.center;
                boxTarget.size = boxSource.size;
                boxTarget.isTrigger = boxSource.isTrigger;
                boxTarget.material = boxSource.material;
            }
            else if (sourceCollider is SphereCollider sphereSource)
            {
                var sphereTarget = targetObject.AddComponent<SphereCollider>();
                sphereTarget.center = sphereSource.center;
                sphereTarget.radius = sphereSource.radius;
                sphereTarget.isTrigger = sphereSource.isTrigger;
                sphereTarget.material = sphereSource.material;
            }
            else if (sourceCollider is CapsuleCollider capsuleSource)
            {
                var capsuleTarget = targetObject.AddComponent<CapsuleCollider>();
                capsuleTarget.center = capsuleSource.center;
                capsuleTarget.radius = capsuleSource.radius;
                capsuleTarget.height = capsuleSource.height;
                capsuleTarget.direction = capsuleSource.direction;
                capsuleTarget.isTrigger = capsuleSource.isTrigger;
                capsuleTarget.material = capsuleSource.material;
            }
            else if (sourceCollider is MeshCollider meshSource)
            {
                var meshTarget = targetObject.AddComponent<MeshCollider>();
                meshTarget.sharedMesh = meshSource.sharedMesh;
                meshTarget.convex = meshSource.convex;
                meshTarget.isTrigger = meshSource.isTrigger;
                meshTarget.material = meshSource.material;
            }
            else
            {
                var targetCollider = targetObject.AddComponent(sourceCollider.GetType()) as Collider;
                EditorUtility.CopySerialized(sourceCollider, targetCollider);
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to copy collider to {targetObject.name}: {e.Message}");
            return false;
        }
    }
}