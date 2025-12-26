#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class PoolSetupTool : EditorWindow
{
    private PoolData poolData;

    private Vector2 scrollPosition;

    // Dictionary để lưu trạng thái checkbox của từng prefab
    private Dictionary<EnumPool, List<bool>> prefabSelections = new Dictionary<EnumPool, List<bool>>();

    // Dictionary để lưu Transform và Amount data riêng biệt (không ảnh hưởng PoolData)
    private Dictionary<EnumPool, List<Transform>> poolTransforms = new Dictionary<EnumPool, List<Transform>>();

    private Dictionary<EnumPool, List<int>> poolAmounts = new Dictionary<EnumPool, List<int>>();

    // Reference đến PoolControl trong scene
    private PoolControl poolControl;
    // Reference đến BotSpawnManager trong scene
    private BotSpawnManager botSpawnManager;

    [MenuItem("Tools/Pool Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<PoolSetupTool>("Pool Setup Tool");
    }

    private void OnEnable()
    {
        LoadPoolDataFromFolder();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Pool Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // Pool Data reference
        EditorGUILayout.BeginHorizontal();
        poolData = (PoolData)EditorGUILayout.ObjectField("Pool Data:", poolData, typeof(PoolData), false);

        if (GUILayout.Button("Create New", GUILayout.Width(100)))
        {
            CreateNewPoolData();
        }

        if (GUILayout.Button("Refresh", GUILayout.Width(80)))
        {
            LoadPoolDataFromFolder();
        }

        EditorGUILayout.EndHorizontal();

        // Pool Control reference
        EditorGUILayout.BeginHorizontal();
        poolControl = (PoolControl)EditorGUILayout.ObjectField("Pool Control:", poolControl, typeof(PoolControl), true);

        if (GUILayout.Button("Find in Scene", GUILayout.Width(100)))
        {
            poolControl = FindObjectOfType<PoolControl>();
            if (poolControl == null)
            {
                Debug.LogWarning("Không tìm thấy PoolControl trong scene!");
            }
            else
            {
                Debug.Log($"Tìm thấy PoolControl: {poolControl.name}");
            }
        }

        EditorGUILayout.EndHorizontal();
        
        // BotSpawnManager reference
        EditorGUILayout.BeginHorizontal();
        botSpawnManager = (BotSpawnManager)EditorGUILayout.ObjectField("BotSpawnManager:", botSpawnManager, typeof(BotSpawnManager), true);

        if (GUILayout.Button("Load Both", GUILayout.Width(100)))
        {
            // Load cả PoolControl và BotSpawnManager cùng lúc
            poolControl = FindObjectOfType<PoolControl>();
            botSpawnManager = FindObjectOfType<BotSpawnManager>();
            
            if (poolControl != null)
                Debug.Log($"✅ Tìm thấy PoolControl: {poolControl.name}");
            else
                Debug.LogWarning("❌ Không tìm thấy PoolControl trong scene!");
                
            if (botSpawnManager != null)
                Debug.Log($"✅ Tìm thấy BotSpawnManager: {botSpawnManager.name}");
            else
                Debug.LogWarning("❌ Không tìm thấy BotSpawnManager trong scene!");
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Hiện thị bảng dữ liệu
        if (poolData != null)
        {
            DrawPoolDataTable();
        }
        else
        {
            EditorGUILayout.HelpBox("Không có Pool Data. Vui lòng tạo hoặc chọn Pool Data.", MessageType.Warning);
        }
    }

    private void LoadPoolDataFromFolder()
    {
        string toolPath = "Assets/_Tool/PoolSetup";
        string[] guids = AssetDatabase.FindAssets("t:PoolData", new[] { toolPath });

        if (guids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            poolData = AssetDatabase.LoadAssetAtPath<PoolData>(assetPath);
            Debug.Log($"Tự động load Pool Data: {assetPath}");
        }
    }

    private void DrawPoolDataTable()
    {
        EditorGUILayout.LabelField("Pool Data Table", EditorStyles.boldLabel);

        // Đồng bộ prefab selections, transforms và amounts
        foreach (var enumPool in poolData.enumPools)
        {
            // Sync prefab selections
            if (!prefabSelections.ContainsKey(enumPool))
            {
                prefabSelections[enumPool] = new List<bool>();
            }

            var selections = prefabSelections[enumPool];
            while (selections.Count < enumPool.prefabUnits.Count)
            {
                selections.Add(false);
            }

            while (selections.Count > enumPool.prefabUnits.Count)
            {
                selections.RemoveAt(selections.Count - 1);
            }

            // Sync transforms
            if (!poolTransforms.ContainsKey(enumPool))
            {
                poolTransforms[enumPool] = new List<Transform>();
            }

            var transforms = poolTransforms[enumPool];
            while (transforms.Count < enumPool.prefabUnits.Count)
            {
                transforms.Add(null);
            }

            while (transforms.Count > enumPool.prefabUnits.Count)
            {
                transforms.RemoveAt(transforms.Count - 1);
            }

            // Sync amounts
            if (!poolAmounts.ContainsKey(enumPool))
            {
                poolAmounts[enumPool] = new List<int>();
            }

            var amounts = poolAmounts[enumPool];
            while (amounts.Count < enumPool.prefabUnits.Count)
            {
                amounts.Add(1); // Default amount = 1
            }

            while (amounts.Count > enumPool.prefabUnits.Count)
            {
                amounts.RemoveAt(amounts.Count - 1);
            }
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Nếu chưa có dữ liệu
        if (poolData.enumPools.Count == 0)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Chưa có Pool nào. Tạo pool mới từ 'Add to this group'.",
                EditorStyles.centeredGreyMiniLabel);

            // Thêm nút tạo pool mới
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Create First Pool", GUILayout.Height(30)))
            {
                poolData.enumPools.Add(new EnumPool { typePool = "DefaultType" });
            }

            EditorGUILayout.EndVertical();
        }
        else
        {
            // Nhóm theo Type Pool
            var groupedPools = poolData.enumPools
                .GroupBy(p => string.IsNullOrEmpty(p.typePool) ? "(Chưa đặt tên)" : p.typePool)
                .ToList();

            foreach (var group in groupedPools)
            {
                // Header cho từng Type Pool group
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginVertical("Box");

                // Type Pool Header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"📦 {group.Key} ({group.Count()} pools)", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add to this group", GUILayout.Width(120)))
                {
                    var newPool = new EnumPool { typePool = group.Key };
                    poolData.enumPools.Add(newPool);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(3);

                // Table Header cho group này
                EditorGUILayout.BeginHorizontal("Button");
                EditorGUILayout.LabelField("Pool Settings", EditorStyles.boldLabel, GUILayout.MinWidth(300));
                EditorGUILayout.LabelField("☑ Prefab Units", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel, GUILayout.MinWidth(150));
                EditorGUILayout.LabelField("Amt", EditorStyles.boldLabel, GUILayout.Width(60));
                EditorGUILayout.LabelField("BotDefinition", EditorStyles.boldLabel, GUILayout.Width(180));
                EditorGUILayout.LabelField("Pool Status", EditorStyles.boldLabel, GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();

                // Các pool items trong group này
                int poolIndex = 0;
                foreach (var enumPool in group)
                {
                    EditorGUILayout.BeginHorizontal("Box");

                    // Cột 1: TypePool & FolderPath & EnumName & Actions gộp lại
                    EditorGUILayout.BeginVertical(GUILayout.MinWidth(300));

                    // TypePool trên dòng 1
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Type:", GUILayout.Width(40));
                    string newTypePool = EditorGUILayout.TextField(enumPool.typePool, GUILayout.ExpandWidth(true));
                    if (newTypePool != enumPool.typePool)
                    {
                        enumPool.typePool = newTypePool;
                    }

                    EditorGUILayout.EndHorizontal();

                    // FolderPath trên dòng 2 CHỈ với nút Browse (không có Reload)
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Path:", GUILayout.Width(40));
                    enumPool.folderPath = EditorGUILayout.TextField(enumPool.folderPath, GUILayout.ExpandWidth(true));

                    // CHỈ nút Browse để chọn folder, KHÔNG có nút Reload
                    if (GUILayout.Button("📁", GUILayout.Width(25)))
                    {
                        string selectedPath = EditorUtility.OpenFolderPanel("Chọn Folder Prefabs", "Assets", "");
                        if (!string.IsNullOrEmpty(selectedPath))
                        {
                            // Chuyển đổi thành relative path
                            if (selectedPath.StartsWith(Application.dataPath))
                            {
                                enumPool.folderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    // BotDefinition folder path trên dòng 3 CHỈ với nút Browse (không có Load)
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Data:", GUILayout.Width(40));
                    enumPool.botDefinitionPath =
                        EditorGUILayout.TextField(enumPool.botDefinitionPath, GUILayout.ExpandWidth(true));

                    // CHỈ nút Browse để chọn folder BotDefinition, KHÔNG có nút Load
                    if (GUILayout.Button("📂", GUILayout.Width(25)))
                    {
                        string selectedPath =
                            EditorUtility.OpenFolderPanel("Chọn Folder BotDefinition", "Assets/_Game_/Scripts/SpawnBotSystem/Data", "");
                        if (!string.IsNullOrEmpty(selectedPath))
                        {
                            // Chuyển đổi thành relative path
                            if (selectedPath.StartsWith(Application.dataPath))
                            {
                                enumPool.botDefinitionPath =
                                    "Assets" + selectedPath.Substring(Application.dataPath.Length);
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    // EnumName và Actions trên dòng 4
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Enum:", GUILayout.Width(40));
                    enumPool.enumName =
                        (EnumBase)EditorGUILayout.EnumPopup(enumPool.enumName, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        int index = poolData.enumPools.IndexOf(enumPool);
                        if (index >= 0)
                        {
                            poolData.enumPools.RemoveAt(index);
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        break;
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();

                    // Cột 2: Prefab Units - mở rộng để lấp đầy
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    var selections = prefabSelections[enumPool];
                    
                    // Đảm bảo có ít nhất một dòng để các cột khác hiển thị đúng
                    int rowCount = Mathf.Max(1, enumPool.prefabUnits.Count);
                    
                    for (int j = 0; j < rowCount; j++)
                    {
                        if (j < enumPool.prefabUnits.Count)
                        {
                            // Kiểm tra trạng thái trong pool để set màu nền
                            var prefab = enumPool.prefabUnits[j];
                            bool isInPool = CheckIfPrefabInPool(prefab);
                            
                            // Set màu nền dựa trên trạng thái
                            if (isInPool)
                            {
                                // Màu xanh nhạt cho prefab đã có trong pool
                                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 0.3f);
                            }
                            else
                            {
                                // Màu đỏ nhạt cho prefab chưa có trong pool
                                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 0.2f);
                            }
                            
                            EditorGUILayout.BeginHorizontal("Box", GUILayout.Height(20));
                            GUI.backgroundColor = Color.white; // Reset màu cho các control bên trong
                            
                            // Checkbox cho từng prefab
                            selections[j] = EditorGUILayout.Toggle(selections[j], GUILayout.Width(20));
                            
                            // Icon trạng thái nhỏ bên cạnh checkbox
                            if (isInPool)
                            {
                                GUI.color = Color.green;
                                EditorGUILayout.LabelField("●", GUILayout.Width(15));
                                GUI.color = Color.white;
                            }
                            else
                            {
                                GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                                EditorGUILayout.LabelField("○", GUILayout.Width(15));
                                GUI.color = Color.white;
                            }
                            
                            // Prefab object field
                            enumPool.prefabUnits[j] = (GameUnitBase)EditorGUILayout.ObjectField(
                                enumPool.prefabUnits[j], typeof(GameUnitBase), false, GUILayout.ExpandWidth(true));
                            if (GUILayout.Button("X", GUILayout.Width(25)))
                            {
                                enumPool.prefabUnits.RemoveAt(j);
                                selections.RemoveAt(j);
                                j--;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            // Dòng trống để giữ layout
                            GUILayout.Space(24);
                        }
                    }

                    // Hai nút: Load từ Path và Add thủ công
                    EditorGUILayout.BeginHorizontal();
                    
                    // Nút Load từ Path
                    if (GUILayout.Button($"🔄 Load Path", GUILayout.Height(20)))
                    {
                        if (!string.IsNullOrEmpty(enumPool.folderPath))
                        {
                            ReloadPrefabsFromFolder(enumPool);
                        }
                        else
                        {
                            Debug.LogWarning("Vui lòng chọn folder Path trước khi load!");
                        }
                        
                        if (!string.IsNullOrEmpty(enumPool.botDefinitionPath))
                        {
                            LoadBotDefinitionsFromFolder(enumPool);
                        }
                    }
                    
                    // Nút Add thủ công
                    if (GUILayout.Button($"+ Add Manual ({enumPool.prefabUnits.Count})", GUILayout.Height(20)))
                    {
                        enumPool.prefabUnits.Add(null);
                    }
                    
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();

                    // Cột 3: Transform
                    EditorGUILayout.BeginVertical(GUILayout.MinWidth(150));
                    var transforms = poolTransforms[enumPool];
                    
                    for (int j = 0; j < rowCount; j++)
                    {
                        if (j < enumPool.prefabUnits.Count)
                        {
                            // Set màu nền tương tự như cột Prefab Units
                            var prefab = enumPool.prefabUnits[j];
                            bool isInPool = CheckIfPrefabInPool(prefab);
                            
                            if (isInPool)
                            {
                                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 0.3f);
                            }
                            else
                            {
                                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 0.2f);
                            }
                            
                            EditorGUILayout.BeginHorizontal("Box", GUILayout.Height(20));
                            GUI.backgroundColor = Color.white;
                            
                            transforms[j] = (Transform)EditorGUILayout.ObjectField(
                                transforms[j], typeof(Transform), true, GUILayout.ExpandWidth(true));
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.Space(24);
                        }
                    }

                    if (GUILayout.Button("Apply All", GUILayout.Height(20)))
                    {
                        if (transforms.Count > 0 && transforms[0] != null)
                        {
                            Transform firstTransform = transforms[0];
                            for (int j = 1; j < transforms.Count; j++)
                            {
                                transforms[j] = firstTransform;
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();

                    // Cột 4: Amount
                    EditorGUILayout.BeginVertical(GUILayout.Width(60));
                    var amounts = poolAmounts[enumPool];
                    
                    for (int j = 0; j < rowCount; j++)
                    {
                        if (j < enumPool.prefabUnits.Count)
                        {
                            // Set màu nền tương tự như cột Prefab Units
                            var prefab = enumPool.prefabUnits[j];
                            bool isInPool = CheckIfPrefabInPool(prefab);
                            
                            if (isInPool)
                            {
                                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 0.3f);
                            }
                            else
                            {
                                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 0.2f);
                            }
                            
                            EditorGUILayout.BeginHorizontal("Box", GUILayout.Height(20));
                            GUI.backgroundColor = Color.white;
                            
                            amounts[j] = EditorGUILayout.IntField(amounts[j], GUILayout.Width(50));
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.Space(24);
                        }
                    }

                    GUILayout.Space(24); // Spacer cho nút
                    EditorGUILayout.EndVertical();

                    // Cột 5: BotDefinition matched (hiển thị tên BotDefinition)
                    EditorGUILayout.BeginVertical(GUILayout.Width(180));
                    
                    // Đồng bộ matchedBotDefs list
                    while (enumPool.matchedBotDefs.Count < enumPool.prefabUnits.Count)
                    {
                        enumPool.matchedBotDefs.Add(null);
                    }

                    while (enumPool.matchedBotDefs.Count > enumPool.prefabUnits.Count)
                    {
                        enumPool.matchedBotDefs.RemoveAt(enumPool.matchedBotDefs.Count - 1);
                    }

                    for (int j = 0; j < rowCount; j++)
                    {
                        if (j < enumPool.prefabUnits.Count)
                        {
                            // Set màu nền tương tự như cột Prefab Units
                            var prefab = enumPool.prefabUnits[j];
                            bool isInPool = CheckIfPrefabInPool(prefab);
                            
                            if (isInPool)
                            {
                                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 0.3f);
                            }
                            else
                            {
                                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 0.2f);
                            }
                            
                            EditorGUILayout.BeginHorizontal("Box", GUILayout.Height(20));
                            GUI.backgroundColor = Color.white;
                            
                            // Hiển thị BotDefinition đã match
                            GUI.enabled = false;
                            if (enumPool.matchedBotDefs[j] != null)
                            {
                                // Hiển thị với màu xanh nếu tìm thấy
                                GUI.color = Color.green;
                                EditorGUILayout.ObjectField(enumPool.matchedBotDefs[j], typeof(BotDefinition), false, GUILayout.Width(170));
                                GUI.color = Color.white;
                            }
                            else
                            {
                                // Hiển thị "Not Found" với màu đỏ nếu không tìm thấy
                                GUI.color = Color.red;
                                EditorGUILayout.TextField("❌ Not Found", GUILayout.Width(170));
                                GUI.color = Color.white;
                            }
                            GUI.enabled = true;
                            
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.Space(24);
                        }
                    }

                    // Nút Reload để load lại BotDefinition
                    if (GUILayout.Button("Reload BotDef", GUILayout.Height(20)))
                    {
                        LoadBotDefinitionsFromFolder(enumPool);
                    }

                    EditorGUILayout.EndVertical();
                    
                    // Cột 6: Pool Status (Có trong pool hay không + Actions)
                    EditorGUILayout.BeginVertical(GUILayout.Width(120));
                    
                    for (int j = 0; j < rowCount; j++)
                    {
                        if (j < enumPool.prefabUnits.Count)
                        {
                            var prefab = enumPool.prefabUnits[j];
                            bool isInPoolControl = CheckIfPrefabInPool(prefab);
                            
                            // Kiểm tra xem BotDefinition có trong BotSpawnManager không
                            bool isBotDefInManager = false;
                            if (j < enumPool.matchedBotDefs.Count)
                            {
                                var botDef = enumPool.matchedBotDefs[j];
                                if (botDef != null)
                                {
                                    isBotDefInManager = CheckIfBotDefinitionInManager(botDef);
                                }
                            }
                            
                            // Set màu nền dựa trên trạng thái
                            if (isInPoolControl && isBotDefInManager)
                            {
                                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 0.3f); // Cả hai đều có
                            }
                            else if (isInPoolControl || isBotDefInManager)
                            {
                                GUI.backgroundColor = new Color(1f, 1f, 0.5f, 0.3f); // Chỉ một trong hai
                            }
                            else
                            {
                                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 0.2f); // Cả hai đều không có
                            }
                            
                            EditorGUILayout.BeginHorizontal("Box", GUILayout.Height(20));
                            GUI.backgroundColor = Color.white;
                            
                            // Status icon - hiển thị trạng thái cho cả PoolControl và BotSpawnManager
                            if (isInPoolControl && isBotDefInManager)
                            {
                                // Cả hai đều có
                                GUI.color = Color.green;
                                EditorGUILayout.LabelField("✓✓", GUILayout.Width(20));
                                GUI.color = Color.white;
                            }
                            else if (isInPoolControl)
                            {
                                // Chỉ có trong PoolControl
                                GUI.color = Color.yellow;
                                EditorGUILayout.LabelField("✓○", GUILayout.Width(20));
                                GUI.color = Color.white;
                            }
                            else if (isBotDefInManager)
                            {
                                // Chỉ có trong BotSpawnManager
                                GUI.color = Color.yellow;
                                EditorGUILayout.LabelField("○✓", GUILayout.Width(20));
                                GUI.color = Color.white;
                            }
                            else
                            {
                                // Không có trong cả hai
                                GUI.color = Color.gray;
                                EditorGUILayout.LabelField("○○", GUILayout.Width(20));
                                GUI.color = Color.white;
                            }
                            
                            // Nút Actions
                            if (isInPoolControl || isBotDefInManager)
                            {
                                // Nút xóa khỏi pool
                                if (GUILayout.Button("❌", GUILayout.Width(25)))
                                {
                                    RemovePrefabFromPool(enumPool, j, prefab);
                                }
                            }
                            else
                            {
                                // Nút thêm vào pool
                                GUI.backgroundColor = Color.green;
                                if (GUILayout.Button("+", GUILayout.Width(25)))
                                {
                                    // Check trùng lặp trước khi thêm
                                    AddSinglePrefabToPool(enumPool, j);
                                }
                                GUI.backgroundColor = Color.white;
                            }
                            
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.Space(24);
                        }
                    }
                    
                    GUILayout.Space(24); // Spacer cho nút
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(2);
                }

                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.EndScrollView();

        // Bottom buttons
        EditorGUILayout.Space(10);

        // Selection buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All Prefabs", GUILayout.Height(25), GUILayout.Width(120)))
        {
            foreach (var kvp in prefabSelections)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    kvp.Value[i] = true;
                }
            }
        }

        if (GUILayout.Button("Deselect All Prefabs", GUILayout.Height(25), GUILayout.Width(130)))
        {
            foreach (var kvp in prefabSelections)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    kvp.Value[i] = false;
                }
            }
        }

        EditorGUILayout.EndHorizontal();
        
        // Nút Load All Path
        EditorGUILayout.Space(5);
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("🔄 Load All Paths", GUILayout.Height(30)))
        {
            LoadAllPaths();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);

        // Main action buttons
        EditorGUILayout.BeginHorizontal();

        // Nút Add to PoolControl thay thế vị trí Add New Pool
        int totalSelectedPrefabs = prefabSelections.Sum(kvp => kvp.Value.Count(x => x));
        if (poolControl != null)
        {
            if (totalSelectedPrefabs > 0)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button($"Add to PoolControl ({totalSelectedPrefabs})", GUILayout.Height(25),
                        GUILayout.ExpandWidth(true)))
                {
                    AddSelectedPrefabsToPoolControl();
                }

                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = Color.gray;
                GUILayout.Button("Select Prefabs First", GUILayout.Height(25), GUILayout.ExpandWidth(true));
                GUI.backgroundColor = Color.white;
            }
        }
        else
        {
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Assign PoolControl First", GUILayout.Height(25), GUILayout.ExpandWidth(true)))
            {
                EditorUtility.DisplayDialog("PoolControl Required",
                    "Vui lòng assign PoolControl trước khi thêm prefabs!", "OK");
            }

            GUI.backgroundColor = Color.white;
        }

        if (GUILayout.Button("Save Changes", GUILayout.Height(25), GUILayout.Width(120)))
        {
            EditorUtility.SetDirty(poolData);
            AssetDatabase.SaveAssets();
            Debug.Log("Pool Data đã được lưu!");
        }

        EditorGUILayout.EndHorizontal();
    }

    private void AddSelectedPrefabsToPoolControl()
    {
        int poolAddedCount = 0;
        int botDefAddedCount = 0;
        
        // PHẦN 1: Thêm vào PoolControl
        if (poolControl != null)
        {
            // Lấy list prefabsToPreload của PoolControl qua reflection
            var poolControlType = poolControl.GetType();
            var prefabsToPreloadField = poolControlType.GetField("prefabsToPreload",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (prefabsToPreloadField != null)
            {
                var prefabsToPreloadList = prefabsToPreloadField.GetValue(poolControl) as System.Collections.IList;
                var poolAmountType = System.Type.GetType("PoolAmount");
                
                if (poolAmountType != null)
                {
                    var gameUnitBaseField = poolAmountType.GetField("gameUnitBase");

                    foreach (var kvp in prefabSelections)
                    {
                        var enumPool = kvp.Key;
                        var selections = kvp.Value;
                        var transforms = poolTransforms[enumPool];
                        var amounts = poolAmounts[enumPool];

                        for (int i = 0; i < selections.Count; i++)
                        {
                            if (selections[i] && enumPool.prefabUnits[i] != null)
                            {
                                var prefab = enumPool.prefabUnits[i];
                                var parentTransform = transforms[i];
                                var amount = amounts[i];

                                // Kiểm tra xem prefab đã tồn tại trong PoolControl chưa
                                bool alreadyExists = false;
                                foreach (var existingPoolAmount in prefabsToPreloadList)
                                {
                                    var existingPrefab = gameUnitBaseField?.GetValue(existingPoolAmount) as GameUnitBase;
                                    if (existingPrefab == prefab)
                                    {
                                        alreadyExists = true;
                                        break;
                                    }
                                }

                                // Nếu chưa tồn tại thì thêm vào
                                if (!alreadyExists)
                                {
                                    var poolAmount = System.Activator.CreateInstance(poolAmountType);

                                    // Set các giá trị
                                    var parentField = poolAmountType.GetField("parent");
                                    var amountField = poolAmountType.GetField("amount");

                                    if (gameUnitBaseField != null) gameUnitBaseField.SetValue(poolAmount, prefab);
                                    if (parentField != null) parentField.SetValue(poolAmount, parentTransform);
                                    if (amountField != null) amountField.SetValue(poolAmount, amount);

                                    // Thêm vào list
                                    prefabsToPreloadList.Add(poolAmount);
                                    poolAddedCount++;
                                    Debug.Log($"✅ [PoolControl] Đã thêm prefab '{prefab.name}' (Amount: {amount})");
                                }
                            }
                        }
                    }
                }
            }
            
            if (poolAddedCount > 0)
                EditorUtility.SetDirty(poolControl);
        }
        
        // PHẦN 2: Thêm BotDefinition vào BotSpawnManager
        if (botSpawnManager != null)
        {
            var botSpawnManagerType = botSpawnManager.GetType();
            var botDefinitionsField = botSpawnManagerType.GetField("botDefinitions",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (botDefinitionsField != null)
            {
                var botDefinitionsList = botDefinitionsField.GetValue(botSpawnManager) as List<BotDefinition>;
                
                if (botDefinitionsList == null)
                {
                    botDefinitionsList = new List<BotDefinition>();
                    botDefinitionsField.SetValue(botSpawnManager, botDefinitionsList);
                }
                
                foreach (var kvp in prefabSelections)
                {
                    var enumPool = kvp.Key;
                    var selections = kvp.Value;
                    
                    for (int i = 0; i < selections.Count; i++)
                    {
                        // Chỉ thêm những prefab được chọn VÀ có BotDefinition match
                        if (selections[i] && i < enumPool.matchedBotDefs.Count)
                        {
                            var botDef = enumPool.matchedBotDefs[i];
                            
                            // Chỉ thêm nếu có BotDefinition (không null)
                            if (botDef != null)
                            {
                                // Kiểm tra trùng lặp
                                if (!botDefinitionsList.Contains(botDef))
                                {
                                    botDefinitionsList.Add(botDef);
                                    botDefAddedCount++;
                                    Debug.Log($"✅ [BotSpawnManager] Đã thêm BotDefinition '{botDef.name}'");
                                }
                            }
                            else
                            {
                                // Log nếu prefab được chọn nhưng không có BotDefinition
                                if (enumPool.prefabUnits[i] != null)
                                {
                                    Debug.LogWarning($"⚠️ Prefab '{enumPool.prefabUnits[i].name}' không có BotDefinition match, không thêm vào BotSpawnManager");
                                }
                            }
                        }
                    }
                }
                
                if (botDefAddedCount > 0)
                    EditorUtility.SetDirty(botSpawnManager);
            }
        }

        // Thông báo kết quả tổng hợp
        Debug.Log($"\n📊 TỔNG KẾT:");
        if (poolControl != null)
            Debug.Log($"   ➤ PoolControl: Đã thêm {poolAddedCount} prefabs");
        else
            Debug.LogWarning($"   ➤ PoolControl: Không được assign");
            
        if (botSpawnManager != null)
            Debug.Log($"   ➤ BotSpawnManager: Đã thêm {botDefAddedCount} BotDefinitions");
        else
            Debug.LogWarning($"   ➤ BotSpawnManager: Không được assign");
    }

    private void RemoveSelectedPrefabs()
    {
        int totalSelected = prefabSelections.Sum(kvp => kvp.Value.Count(x => x));
        if (EditorUtility.DisplayDialog("Xóa Prefab đã chọn",
                $"Bạn có chắc chắn muốn xóa {totalSelected} prefab(s) đã chọn?",
                "Xóa", "Hủy"))
        {
            foreach (var kvp in prefabSelections.ToList())
            {
                var enumPool = kvp.Key;
                var selections = kvp.Value;
                var transforms = poolTransforms[enumPool];
                var amounts = poolAmounts[enumPool];

                // Xóa từ cuối lên đầu để tránh lỗi index
                for (int i = selections.Count - 1; i >= 0; i--)
                {
                    if (selections[i])
                    {
                        enumPool.prefabUnits.RemoveAt(i);
                        selections.RemoveAt(i);
                        transforms.RemoveAt(i);
                        amounts.RemoveAt(i);
                    }
                }
            }

            EditorUtility.SetDirty(poolData);
            Debug.Log($"Đã xóa {totalSelected} prefab đã chọn!");
        }
    }

    private void CreateNewPoolData()
    {
        string defaultPath = "Assets/_Tool/PoolSetup/PoolData.asset";
        var newPoolData = CreateInstance<PoolData>();

        AssetDatabase.CreateAsset(newPoolData, defaultPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        poolData = newPoolData;
        Debug.Log($"Tạo Pool Data mới tại: {defaultPath}");
    }

    // Hàm mới để reload prefabs từ folder
    private void ReloadPrefabsFromFolder(EnumPool enumPool)
    {
        if (string.IsNullOrEmpty(enumPool.folderPath))
        {
            Debug.LogWarning("Vui lòng chỉ định đường dẫn folder trước!");
            return;
        }

        if (!AssetDatabase.IsValidFolder(enumPool.folderPath))
        {
            Debug.LogError($"Folder không tồn tại: {enumPool.folderPath}");
            return;
        }

        // Tìm tất cả prefabs trong folder
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { enumPool.folderPath });

        if (guids.Length == 0)
        {
            Debug.LogWarning($"Không tìm thấy prefab nào trong folder: {enumPool.folderPath}");
            return;
        }

        // Clear list cũ
        enumPool.prefabUnits.Clear();

        // Load và thêm các prefabs
        int addedCount = 0;
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefabGO != null)
            {
                // Kiểm tra xem prefab có component GameUnitBase không
                GameUnitBase gameUnit = prefabGO.GetComponent<GameUnitBase>();
                if (gameUnit != null)
                {
                    enumPool.prefabUnits.Add(gameUnit);
                    addedCount++;
                }
            }
        }

        // Đồng bộ lại selections, transforms và amounts
        if (prefabSelections.ContainsKey(enumPool))
        {
            prefabSelections[enumPool].Clear();
        }

        if (poolTransforms.ContainsKey(enumPool))
        {
            poolTransforms[enumPool].Clear();
        }

        if (poolAmounts.ContainsKey(enumPool))
        {
            poolAmounts[enumPool].Clear();
        }

        Debug.Log($"✅ Đã reload {addedCount} prefabs từ folder: {enumPool.folderPath}");

        // Đánh dấu dirty để lưu thay đổi
        EditorUtility.SetDirty(poolData);
    }
    
    // Hàm mới để load BotDefinitions từ folder và match với prefabs
    private void LoadBotDefinitionsFromFolder(EnumPool enumPool)
    {
        // Clear matched BotDefinitions trước
        enumPool.matchedBotDefs.Clear();
        
        if (string.IsNullOrEmpty(enumPool.botDefinitionPath))
        {
            Debug.LogWarning("Vui lòng chỉ định đường dẫn folder BotDefinition trước!");
            // Set null cho tất cả
            for (int i = 0; i < enumPool.prefabUnits.Count; i++)
            {
                enumPool.matchedBotDefs.Add(null);
            }
            return;
        }
        
        if (!AssetDatabase.IsValidFolder(enumPool.botDefinitionPath))
        {
            Debug.LogError($"Folder không tồn tại: {enumPool.botDefinitionPath}");
            // Set null cho tất cả
            for (int i = 0; i < enumPool.prefabUnits.Count; i++)
            {
                enumPool.matchedBotDefs.Add(null);
            }
            return;
        }
        
        // Tìm tất cả BotDefinition ScriptableObject trong folder
        string[] guids = AssetDatabase.FindAssets("t:BotDefinition", new[] { enumPool.botDefinitionPath });
        
        Debug.Log($"📂 Đang tìm BotDefinition trong: {enumPool.botDefinitionPath}");
        Debug.Log($"   Tìm thấy {guids.Length} file BotDefinition");
        
        // Nếu không tìm thấy theo type, thử tìm tất cả .asset files
        if (guids.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy BotDefinition theo type, thử tìm tất cả .asset files...");
            guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { enumPool.botDefinitionPath });
            Debug.Log($"   Tìm thấy {guids.Length} ScriptableObject files");
        }
        
        // Load tất cả BotDefinitions
        var botDefinitions = new System.Collections.Generic.Dictionary<string, BotDefinition>();
        int loadedCount = 0;
        int nullPrefabCount = 0;
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            BotDefinition botDef = AssetDatabase.LoadAssetAtPath<BotDefinition>(assetPath);
            
            if (botDef != null)
            {
                if (botDef.Prefab != null)
                {
                    // Lưu với key là tên của prefab trong BotDefinition
                    string prefabName = botDef.Prefab.name;
                    if (!botDefinitions.ContainsKey(prefabName))
                    {
                        botDefinitions[prefabName] = botDef;
                        loadedCount++;
                        Debug.Log($"   ✅ Loaded: {botDef.name} -> Prefab: {prefabName}");
                    }
                }
                else
                {
                    nullPrefabCount++;
                    Debug.LogWarning($"   ⚠️ BotDefinition '{botDef.name}' có Prefab = null");
                }
            }
        }
        
        Debug.Log($"📊 Tổng kết: Loaded {loadedCount} BotDefinitions, {nullPrefabCount} có Prefab null");
        
        // Match prefabs với BotDefinitions theo tên
        int matchedCount = 0;
        foreach (var prefab in enumPool.prefabUnits)
        {
            if (prefab != null)
            {
                string prefabName = prefab.name;
                BotDefinition matchedDef = null;
                
                // Tìm chính xác trước
                if (botDefinitions.ContainsKey(prefabName))
                {
                    matchedDef = botDefinitions[prefabName];
                }
                else
                {
                    // Nếu không tìm thấy chính xác, thử các cách khác
                    // 1. Thử tìm với Variant suffix (VD: prefab "Enemy" match với "Enemy (1)")
                    foreach (var kvp in botDefinitions)
                    {
                        string botPrefabName = kvp.Key;
                        
                        // Bỏ qua suffix variant trong ngoặc đơn
                        string cleanBotName = System.Text.RegularExpressions.Regex.Replace(botPrefabName, @"\s*\([^)]*\)$", "").Trim();
                        string cleanPrefabName = System.Text.RegularExpressions.Regex.Replace(prefabName, @"\s*\([^)]*\)$", "").Trim();
                        
                        // So sánh tên đã clean
                        if (cleanBotName.Equals(cleanPrefabName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            matchedDef = kvp.Value;
                            Debug.Log($"🔍 Matched with variant: {prefabName} -> {botPrefabName}");
                            break;
                        }
                        
                        // 2. Thử với underscore và dash
                        string normalizedBotName = cleanBotName.Replace("_", "").Replace("-", "").Replace(" ", "").ToLower();
                        string normalizedPrefabName = cleanPrefabName.Replace("_", "").Replace("-", "").Replace(" ", "").ToLower();
                        
                        if (normalizedBotName == normalizedPrefabName)
                        {
                            matchedDef = kvp.Value;
                            Debug.Log($"🔍 Matched normalized: {prefabName} -> {botPrefabName}");
                            break;
                        }
                    }
                }
                
                if (matchedDef != null)
                {
                    enumPool.matchedBotDefs.Add(matchedDef);
                    matchedCount++;
                    Debug.Log($"✅ Matched: {prefabName} -> {matchedDef.name}");
                }
                else
                {
                    // Không tìm thấy BotDefinition
                    enumPool.matchedBotDefs.Add(null);
                    Debug.Log($"❌ Not found: {prefabName}");
                    
                    // Log thêm để debug
                    Debug.Log($"   Available BotDefs: {string.Join(", ", botDefinitions.Keys)}");
                }
            }
            else
            {
                // Prefab null
                enumPool.matchedBotDefs.Add(null);
            }
        }
        
        Debug.Log($"✅ Đã load và match {matchedCount}/{enumPool.prefabUnits.Count} prefabs với BotDefinitions từ folder: {enumPool.botDefinitionPath}");
        
        // Đánh dấu dirty để lưu thay đổi
        EditorUtility.SetDirty(poolData);
    }
    
    // Hàm mới để Load All Paths - load tất cả các folder path cùng lúc
    private void LoadAllPaths()
    {
        Debug.Log("\n🔄 === BẮT ĐẦU LOAD ALL PATHS ===");
        
        int totalPools = 0;
        int loadedPools = 0;
        int totalPrefabs = 0;
        int totalBotDefs = 0;
        
        foreach (var enumPool in poolData.enumPools)
        {
            totalPools++;
            Debug.Log($"\n📦 Loading pool: {enumPool.typePool}");
            
            // Load Prefabs từ folder
            if (!string.IsNullOrEmpty(enumPool.folderPath))
            {
                int prefabCount = enumPool.prefabUnits.Count;
                ReloadPrefabsFromFolder(enumPool);
                int newPrefabCount = enumPool.prefabUnits.Count;
                totalPrefabs += newPrefabCount;
                Debug.Log($"   ✅ Loaded {newPrefabCount} prefabs từ: {enumPool.folderPath}");
                loadedPools++;
            }
            else
            {
                Debug.LogWarning($"   ⚠️ Chưa có folder Path cho pool này");
            }
            
            // Load BotDefinitions từ folder  
            if (!string.IsNullOrEmpty(enumPool.botDefinitionPath))
            {
                LoadBotDefinitionsFromFolder(enumPool);
                int matchedCount = enumPool.matchedBotDefs.Count(b => b != null);
                totalBotDefs += matchedCount;
                Debug.Log($"   ✅ Matched {matchedCount} BotDefinitions từ: {enumPool.botDefinitionPath}");
            }
            else
            {
                Debug.LogWarning($"   ⚠️ Chưa có folder BotDefinition cho pool này");
            }
        }
        
        // Tổng kết
        Debug.Log($"\n📊 === TỔNG KẾT LOAD ALL ===");
        Debug.Log($"   ➤ Đã load {loadedPools}/{totalPools} pools");
        Debug.Log($"   ➤ Tổng số prefabs: {totalPrefabs}");
        Debug.Log($"   ➤ Tổng số BotDefinitions matched: {totalBotDefs}");
        
        EditorUtility.SetDirty(poolData);
    }
    
    // Hàm kiểm tra prefab có trong PoolControl hay không
    private bool CheckIfPrefabInPool(GameUnitBase prefab)
    {
        if (prefab == null || poolControl == null) return false;
        
        // Lấy list prefabsToPreload qua reflection
        var poolControlType = poolControl.GetType();
        var prefabsToPreloadField = poolControlType.GetField("prefabsToPreload",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (prefabsToPreloadField != null)
        {
            var prefabsToPreloadList = prefabsToPreloadField.GetValue(poolControl) as System.Collections.IList;
            var poolAmountType = System.Type.GetType("PoolAmount");
            
            if (poolAmountType != null && prefabsToPreloadList != null)
            {
                var gameUnitBaseField = poolAmountType.GetField("gameUnitBase");
                
                foreach (var poolAmount in prefabsToPreloadList)
                {
                    var existingPrefab = gameUnitBaseField?.GetValue(poolAmount) as GameUnitBase;
                    if (existingPrefab == prefab)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    // Hàm kiểm tra BotDefinition có trong BotSpawnManager hay không
    private bool CheckIfBotDefinitionInManager(BotDefinition botDef)
    {
        if (botDef == null || botSpawnManager == null) return false;
        
        var botSpawnManagerType = botSpawnManager.GetType();
        var botDefinitionsField = botSpawnManagerType.GetField("botDefinitions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (botDefinitionsField != null)
        {
            var botDefinitionsList = botDefinitionsField.GetValue(botSpawnManager) as List<BotDefinition>;
            if (botDefinitionsList != null && botDefinitionsList.Contains(botDef))
            {
                return true;
            }
        }
        
        return false;
    }
    
    // Hàm xóa prefab khỏi PoolControl và BotDefinition khỏi BotSpawnManager
    private void RemovePrefabFromPool(EnumPool enumPool, int index, GameUnitBase prefab)
    {
        if (prefab == null) return;
        
        // Xóa khỏi PoolControl (nếu có)
        if (poolControl != null)
        {
            // Lấy list prefabsToPreload qua reflection
            var poolControlType = poolControl.GetType();
            var prefabsToPreloadField = poolControlType.GetField("prefabsToPreload",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (prefabsToPreloadField != null)
            {
                var prefabsToPreloadList = prefabsToPreloadField.GetValue(poolControl) as System.Collections.IList;
                var poolAmountType = System.Type.GetType("PoolAmount");
                
                if (poolAmountType != null && prefabsToPreloadList != null)
                {
                    var gameUnitBaseField = poolAmountType.GetField("gameUnitBase");
                    
                    // Tìm và xóa prefab
                    for (int i = prefabsToPreloadList.Count - 1; i >= 0; i--)
                    {
                        var poolAmount = prefabsToPreloadList[i];
                        var existingPrefab = gameUnitBaseField?.GetValue(poolAmount) as GameUnitBase;
                        if (existingPrefab == prefab)
                        {
                            prefabsToPreloadList.RemoveAt(i);
                            Debug.Log($"❌ Đã xóa '{prefab.name}' khỏi PoolControl");
                            EditorUtility.SetDirty(poolControl);
                        }
                    }
                }
            }
        }
        
        // Xóa BotDefinition khỏi BotSpawnManager (nếu có)
        if (botSpawnManager != null && index < enumPool.matchedBotDefs.Count)
        {
            var botDef = enumPool.matchedBotDefs[index];
            if (botDef != null)
            {
                var botSpawnManagerType = botSpawnManager.GetType();
                var botDefinitionsField = botSpawnManagerType.GetField("botDefinitions",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                if (botDefinitionsField != null)
                {
                    var botDefinitionsList = botDefinitionsField.GetValue(botSpawnManager) as List<BotDefinition>;
                    
                    if (botDefinitionsList != null && botDefinitionsList.Contains(botDef))
                    {
                        botDefinitionsList.Remove(botDef);
                        Debug.Log($"❌ Đã xóa BotDefinition '{botDef.name}' khỏi BotSpawnManager");
                        EditorUtility.SetDirty(botSpawnManager);
                    }
                }
            }
        }
    }
    
    // Hàm thêm một prefab vào PoolControl và BotDefinition vào BotSpawnManager
    private void AddSinglePrefabToPool(EnumPool enumPool, int index)
    {
        if (poolControl == null)
        {
            Debug.LogWarning("Vui lòng assign PoolControl trước!");
            return;
        }
        
        var prefab = enumPool.prefabUnits[index];
        if (prefab == null) return;
        
        // Kiểm tra xem đã có trong pool chưa
        if (CheckIfPrefabInPool(prefab))
        {
            Debug.LogWarning($"Prefab '{prefab.name}' đã có trong PoolControl!");
            return;
        }
        
        // Thêm vào PoolControl
        var poolControlType = poolControl.GetType();
        var prefabsToPreloadField = poolControlType.GetField("prefabsToPreload",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (prefabsToPreloadField != null)
        {
            var prefabsToPreloadList = prefabsToPreloadField.GetValue(poolControl) as System.Collections.IList;
            var poolAmountType = System.Type.GetType("PoolAmount");
            
            if (poolAmountType != null && prefabsToPreloadList != null)
            {
                var poolAmount = System.Activator.CreateInstance(poolAmountType);
                
                // Set các giá trị
                var gameUnitBaseField = poolAmountType.GetField("gameUnitBase");
                var parentField = poolAmountType.GetField("parent");
                var amountField = poolAmountType.GetField("amount");
                
                // Lấy transform và amount từ dictionary
                var transforms = poolTransforms[enumPool];
                var amounts = poolAmounts[enumPool];
                
                if (gameUnitBaseField != null) gameUnitBaseField.SetValue(poolAmount, prefab);
                if (parentField != null && index < transforms.Count) 
                    parentField.SetValue(poolAmount, transforms[index]);
                if (amountField != null && index < amounts.Count) 
                    amountField.SetValue(poolAmount, amounts[index]);
                else if (amountField != null)
                    amountField.SetValue(poolAmount, 1);
                    
                // Thêm vào list
                prefabsToPreloadList.Add(poolAmount);
                Debug.Log($"✅ Đã thêm '{prefab.name}' vào PoolControl");
                EditorUtility.SetDirty(poolControl);
            }
        }
        
        // Thêm BotDefinition vào BotSpawnManager (nếu có)
        if (botSpawnManager != null && index < enumPool.matchedBotDefs.Count)
        {
            var botDef = enumPool.matchedBotDefs[index];
            if (botDef != null)
            {
                var botSpawnManagerType = botSpawnManager.GetType();
                var botDefinitionsField = botSpawnManagerType.GetField("botDefinitions",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                if (botDefinitionsField != null)
                {
                    var botDefinitionsList = botDefinitionsField.GetValue(botSpawnManager) as List<BotDefinition>;
                    
                    if (botDefinitionsList != null && !botDefinitionsList.Contains(botDef))
                    {
                        botDefinitionsList.Add(botDef);
                        Debug.Log($"✅ Đã thêm BotDefinition '{botDef.name}' vào BotSpawnManager");
                        EditorUtility.SetDirty(botSpawnManager);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Prefab '{prefab.name}' không có BotDefinition match, không thêm vào BotSpawnManager");
            }
        }
    }
}

[System.Serializable]
public class EnumPool
{
    public string typePool;
    public string folderPath; // Đường dẫn tới folder chứa prefabs
    public string botDefinitionPath; // Đường dẫn tới folder chứa BotDefinition ScriptableObjects
    public EnumBase enumName;
    public List<GameUnitBase> prefabUnits = new List<GameUnitBase>();
    public List<BotDefinition> matchedBotDefs = new List<BotDefinition>(); // Lưu BotDefinition đã match với prefab
    
    [System.Obsolete("Use matchedBotDefs instead")]
    public List<int> savedAmounts = new List<int>(); // Deprecated - giữ lại để tương thích
}

public enum EnumBase
{
    BotType,
    EffectType,
    Weapon,
    ProjecttilePlayer,
    Missile_Player,
    Gift,
}
#endif
