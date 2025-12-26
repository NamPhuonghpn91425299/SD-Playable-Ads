#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq; // Cần cho LINQ (ví dụ: ToList)

public class PrefabMissingScriptRemover : EditorWindow
{
    private List<GameObject> prefabs = new List<GameObject>();
    private ReorderableList reorderableList;
    private Vector2 scrollPosition;
    private Vector2 listScrollPosition;
    private string colliderGroupName = "Colliders_Root"; // Tên cho GameObject cha chứa collider

    [MenuItem("Tools/Prefab Utilities Extended")] // Đổi tên menu một chút
    public static void ShowWindow()
    {
        GetWindow<PrefabMissingScriptRemover>("Prefab Utilities Ext");
    }

    private void OnEnable()
    {
        // ... (OnEnable giữ nguyên như trước) ...
        reorderableList = new ReorderableList(prefabs, typeof(GameObject), true, true, true, true);
        reorderableList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Prefabs / GameObjects in Scene");
        };
        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            prefabs[index] = (GameObject)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), prefabs[index], typeof(GameObject), true); // allowSceneObjects = true
        };
        reorderableList.onAddCallback = (ReorderableList list) => {
            prefabs.Add(null);
        };
        reorderableList.onRemoveCallback = (ReorderableList list) => {
            if (list.index >= 0 && list.index < prefabs.Count) // Thêm kiểm tra index hợp lệ
            {
                prefabs.RemoveAt(list.index);
            }
        };
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.Label("Prefab/GameObject Utilities", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        GUILayout.Label("Prefabs to Process:", EditorStyles.boldLabel);
        listScrollPosition = GUILayout.BeginScrollView(listScrollPosition, GUILayout.Height(200)); // Giảm chiều cao một chút
        reorderableList.DoLayoutList();
        GUILayout.EndScrollView();

        HandleDragAndDrop();
        EditorGUILayout.Space();

        Color defaultColor = GUI.backgroundColor;

        // --- Chức năng Missing Scripts ---
        GUILayout.Label("Missing Script Utilities", EditorStyles.boldLabel);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Scan and Remove Missing Scripts"))
        {
            ProcessPrefabs(RemoveAllMissingScripts, "Removing Missing Scripts");
        }
        GUI.backgroundColor = defaultColor;
        EditorGUILayout.Space();

        // --- Chức năng Collider ---
        GUILayout.Label("Collider Utilities", EditorStyles.boldLabel);
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Disable Colliders"))
        {
            ProcessPrefabs(DisableCollidersInGameObject, "Disabling Colliders");
        }
        GUI.backgroundColor = defaultColor;

        EditorGUILayout.Space();
        colliderGroupName = EditorGUILayout.TextField("Collider Group Name:", colliderGroupName);
        GUI.backgroundColor = new Color(0.5f, 0.8f, 1f); // Màu xanh nhạt
        if (GUILayout.Button($"Group Objects with Colliders into '{colliderGroupName}'"))
        {
            if (string.IsNullOrEmpty(colliderGroupName))
            {
                EditorUtility.DisplayDialog("Error", "Collider Group Name cannot be empty.", "OK");
            }
            else
            {
                ProcessPrefabs(GroupCollidersInGameObject, $"Grouping Colliders into '{colliderGroupName}'");
            }
        }
        GUI.backgroundColor = defaultColor;
        EditorGUILayout.Space(20); // Thêm khoảng cách

        // --- Nút Clear List ---
        if (GUILayout.Button("Clear List"))
        {
            if (EditorUtility.DisplayDialog("Clear List", "Are you sure you want to clear the list of prefabs?", "Yes", "No"))
            {
                prefabs.Clear();
            }
        }

        GUILayout.EndScrollView();
    }

    // Hàm chung để xử lý danh sách prefabs
    private void ProcessPrefabs(System.Action<GameObject> processAction, string progressBarTitle)
    {
        if (prefabs.Count == 0)
        {
            EditorUtility.DisplayDialog("No Prefabs", "Please add prefabs to the list first.", "OK");
            return;
        }

        int processedCount = 0;
        int totalPrefabs = prefabs.Count;
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var obj in prefabs) // Đổi tên biến để rõ ràng hơn nó có thể là prefab hoặc scene object
            {
                processedCount++;
                if (obj != null)
                {
                    EditorUtility.DisplayProgressBar(progressBarTitle, $"Processing {obj.name} ({processedCount}/{totalPrefabs})", (float)processedCount / totalPrefabs);
                    processAction(obj);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Finished: {progressBarTitle}. Processed {processedCount - prefabs.Count(p => p == null)} valid objects."); // Đếm chính xác hơn
        }
    }


    // --- Logic cho Group Colliders ---
    private void GroupCollidersInGameObject(GameObject rootObject)
    {
        string path = AssetDatabase.GetAssetPath(rootObject);
        bool isSceneObject = string.IsNullOrEmpty(path);

        GameObject targetToProcess = rootObject;
        GameObject prefabInstance = null;

        if (!isSceneObject) // Nếu là Prefab Asset
        {
            prefabInstance = PrefabUtility.LoadPrefabContents(path);
            if (prefabInstance == null)
            {
                Debug.LogError($"Failed to load prefab contents for: {rootObject.name} at path {path}");
                return;
            }
            targetToProcess = prefabInstance;
            Undo.RegisterCompleteObjectUndo(targetToProcess, $"Group Colliders in {rootObject.name}"); // Ghi nhận cho toàn bộ prefab instance
        }
        else // Nếu là GameObject trong Scene
        {
            Undo.RegisterFullObjectHierarchyUndo(targetToProcess, $"Group Colliders in {rootObject.name}"); // Ghi nhận cho scene object và các con
        }


        // Tìm hoặc tạo GameObject cha cho các collider
        Transform colliderRootTransform = targetToProcess.transform.Find(colliderGroupName);
        if (colliderRootTransform == null)
        {
            GameObject newColliderRoot = new GameObject(colliderGroupName);
            Undo.RegisterCreatedObjectUndo(newColliderRoot, $"Create {colliderGroupName}");
            newColliderRoot.transform.SetParent(targetToProcess.transform, false); // false để giữ local transform
            newColliderRoot.transform.SetAsFirstSibling(); // Đưa lên đầu cho dễ nhìn (tùy chọn)
            colliderRootTransform = newColliderRoot.transform;
        }

        // Thu thập danh sách các GameObject cần di chuyển (để tránh lỗi khi thay đổi collection trong lúc duyệt)
        List<Transform> objectsToMove = new List<Transform>();
        FindCollidersToMoveRecursive(targetToProcess.transform, colliderRootTransform, objectsToMove);

        // Di chuyển các đối tượng
        foreach (Transform objToMove in objectsToMove)
        {
            // Kiểm tra lại một lần nữa phòng trường hợp cấu trúc đã thay đổi
            if (objToMove != null && objToMove.parent != colliderRootTransform)
            {
                Undo.SetTransformParent(objToMove, colliderRootTransform, $"Move {objToMove.name} to {colliderGroupName}");
                objToMove.SetAsLastSibling(); // Đưa xuống cuối trong group (tùy chọn)
            }
        }

        if (!isSceneObject && prefabInstance != null)
        {
            PrefabUtility.SaveAsPrefabAsset(targetToProcess, path); // targetToProcess chính là prefabInstance đã được sửa
            PrefabUtility.UnloadPrefabContents(prefabInstance);
            Debug.Log($"Colliders grouped in prefab: {rootObject.name}");
        }
        else
        {
            Debug.Log($"Colliders grouped in Scene GameObject: {rootObject.name}");
            if(isSceneObject) EditorUtility.SetDirty(rootObject); // Đánh dấu scene object đã thay đổi
        }
    }

    private void FindCollidersToMoveRecursive(Transform currentTransform, Transform colliderRoot, List<Transform> objectsToMove)
    {
        // Duyệt qua các con trực tiếp trước khi đi sâu hơn
        // Tạo một bản sao của danh sách con để duyệt, vì chúng ta có thể thay đổi parent của chúng
        List<Transform> children = new List<Transform>();
        foreach (Transform child in currentTransform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            // Bỏ qua chính cái root chứa collider và các con của nó (nếu có)
            if (child == colliderRoot) continue;

            // Kiểm tra xem child có Collider không
            if (child.GetComponent<Collider>() != null)
            {
                // Chỉ thêm vào danh sách nếu nó chưa phải là con của colliderRoot
                // và nó không phải là một root của một prefab lồng nhau (nếu muốn xử lý phức tạp hơn)
                if (child.parent != colliderRoot) // Quan trọng: chỉ di chuyển nếu chưa ở đúng chỗ
                {
                    objectsToMove.Add(child);
                }
            }
            // Tiếp tục đệ quy cho các con của child này,
            // TRỪ KHI child này sắp được di chuyển (vì các con của nó sẽ đi theo)
            // Hoặc nếu child này chính là colliderRoot
            if (!objectsToMove.Contains(child) && child != colliderRoot)
            {
                 FindCollidersToMoveRecursive(child, colliderRoot, objectsToMove);
            }
        }
    }


    // --- Logic cho Disable Colliders --- (Giữ nguyên và cải thiện Undo)
    private void DisableCollidersInGameObject(GameObject rootObject)
    {
        string path = AssetDatabase.GetAssetPath(rootObject);
        bool isSceneObject = string.IsNullOrEmpty(path);

        if (isSceneObject)
        {
            Undo.RegisterFullObjectHierarchyUndo(rootObject, "Disable Colliders in Scene Object");
            DisableCollidersRecursive(rootObject);
            Debug.Log("All colliders disabled in GameObject: " + rootObject.name);
            EditorUtility.SetDirty(rootObject);
        }
        else
        {
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);
            if (prefabInstance != null)
            {
                Undo.RegisterCompleteObjectUndo(prefabInstance, "Disable Colliders in Prefab");
                DisableCollidersRecursive(prefabInstance);
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                PrefabUtility.UnloadPrefabContents(prefabInstance);
                Debug.Log("All colliders disabled in prefab: " + rootObject.name);
            }
            else { Debug.LogError("Failed to load prefab: " + rootObject.name); }
        }
    }

    private void DisableCollidersRecursive(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            DisableCollidersRecursive(child.gameObject);
        }
        Collider[] colliders = obj.GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != null && col.enabled) // Chỉ record và thay đổi nếu đang enabled
            {
                Undo.RecordObject(col, "Disable Collider");
                col.enabled = false;
            }
        }
    }

    // --- Logic cho Missing Scripts --- (Giữ nguyên và cải thiện Undo)
    private void RemoveAllMissingScripts(GameObject rootObject)
    {
        string path = AssetDatabase.GetAssetPath(rootObject);
        bool isSceneObject = string.IsNullOrEmpty(path);

        if (isSceneObject)
        {
            Undo.RegisterFullObjectHierarchyUndo(rootObject, "Remove Missing Scripts in Scene Object");
            RemoveMissingScriptsRecursive(rootObject);
            Debug.Log("All missing scripts removed from GameObject: " + rootObject.name);
            EditorUtility.SetDirty(rootObject);
        }
        else
        {
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);
            if (prefabInstance != null)
            {
                Undo.RegisterCompleteObjectUndo(prefabInstance, "Remove Missing Scripts in Prefab");
                RemoveMissingScriptsRecursive(prefabInstance);
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                PrefabUtility.UnloadPrefabContents(prefabInstance);
                Debug.Log("All missing scripts removed from prefab: " + rootObject.name);
            }
            else { Debug.LogError("Failed to load prefab: " + rootObject.name); }
        }
    }

    private void RemoveMissingScriptsRecursive(GameObject obj)
    {
        // Ghi nhận Undo cho việc xóa component script bị thiếu
        // GameObjectUtility.RemoveMonoBehavioursWithMissingScript sẽ tự xử lý Undo
        int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
        if (removedCount > 0)
        {
            // Không cần Undo.RecordObject ở đây vì GameObjectUtility đã xử lý
             Debug.Log($"Removed {removedCount} missing scripts from {obj.name}");
        }

        foreach (Transform child in obj.transform) // Duyệt qua bản sao để tránh lỗi collection modified
        {
            RemoveMissingScriptsRecursive(child.gameObject);
        }
    }


    // --- Drag and Drop --- (Giữ nguyên)
    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Ném Prefabs hoặc GameObjects từ Scene vào đây!"); // Sửa text một chút

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition)) return;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        GameObject go = draggedObject as GameObject;
                        if (go != null && !prefabs.Contains(go)) // Kiểm tra trùng lặp
                        {
                            prefabs.Add(go);
                        }
                    }
                    // Sắp xếp lại danh sách nếu muốn, hoặc để ReorderableList tự xử lý
                    Repaint(); // Vẽ lại cửa sổ để cập nhật danh sách
                }
                Event.current.Use();
                break;
        }
    }
}
#endif