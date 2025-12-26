using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AssetOrganizerTool : EditorWindow
{
    // ... (Phần class OrganizationRule không đổi)
    [System.Serializable]
    public class OrganizationRule
    {
        public string ruleName;
        public string fileExtensions;
        public string targetFolder;

        public OrganizationRule(string name, string extensions, string folder)
        {
            ruleName = name;
            fileExtensions = extensions;
            targetFolder = folder;
        }
    }

    private List<OrganizationRule> rules = new List<OrganizationRule>();
    private Vector2 scrollPosition;
    private string baseFolder = "_Game";

    [MenuItem("Tools/Asset Organizer Tool")]
    public static void ShowWindow()
    {
        GetWindow<AssetOrganizerTool>("Asset Organizer");
    }

    private void OnEnable()
    {
        if (rules.Count == 0)
        {
            LoadDefaultRules();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Asset Organizer Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Công cụ này sẽ giúp bạn sắp xếp các tài sản trong dự án vào các thư mục được định nghĩa trước dựa trên loại tệp.", MessageType.Info);
        
        EditorGUILayout.Space();

        // --- Cài đặt chung với nút Browse ---
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        baseFolder = EditorGUILayout.TextField(
            new GUIContent("Base Organizing Folder", "Tất cả các thư mục sẽ được tạo bên trong thư mục này. Để trống nếu muốn tạo ở cấp gốc 'Assets'."), 
            baseFolder
        );
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string absPath = EditorUtility.OpenFolderPanel("Select Base Folder", "Assets", "");
            if (!string.IsNullOrEmpty(absPath))
            {
                // Chuyển đường dẫn tuyệt đối thành đường dẫn tương đối với thư mục dự án
                baseFolder = GetRelativePath(absPath);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();

        // --- Giao diện quản lý quy tắc với nút Browse ---
        GUILayout.Label("Organization Rules", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < rules.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            
            rules[i].ruleName = EditorGUILayout.TextField("Rule Name", rules[i].ruleName);
            rules[i].fileExtensions = EditorGUILayout.TextField(
                new GUIContent("File Extensions", "Các đuôi file, phân tách bởi dấu phẩy. Ví dụ: .png,.jpg"), 
                rules[i].fileExtensions
            );
            
            // Trường Target Subfolder với nút Browse
            EditorGUILayout.BeginHorizontal();
            rules[i].targetFolder = EditorGUILayout.TextField("Target Subfolder", rules[i].targetFolder);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                // Mở bảng chọn thư mục, bắt đầu từ thư mục Assets
                string absPath = EditorUtility.OpenFolderPanel("Select Target Folder", "Assets", "");
                if (!string.IsNullOrEmpty(absPath))
                {
                    // Chuyển đổi đường dẫn tuyệt đối thành đường dẫn tương đối
                    // và loại bỏ phần baseFolder nếu có
                    string relativePath = GetRelativePath(absPath);
                    string baseFolderPath = Path.Combine("Assets", baseFolder).Replace("\\", "/");

                    if (!string.IsNullOrEmpty(baseFolder) && relativePath.StartsWith(baseFolderPath))
                    {
                        // Lấy phần đường dẫn con sau baseFolder
                        rules[i].targetFolder = relativePath.Substring(baseFolderPath.Length + 1);
                    }
                    else if (relativePath.StartsWith("Assets/"))
                    {
                         // Lấy phần đường dẫn con sau "Assets/"
                         rules[i].targetFolder = relativePath.Substring("Assets/".Length);
                    }
                    else
                    {
                        // Nếu thư mục được chọn nằm ngoài thư mục Assets thì không hợp lệ
                        // nhưng chúng ta vẫn có thể gán nó, mặc dù công cụ có thể không hoạt động đúng
                        rules[i].targetFolder = relativePath;
                    }

                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Remove Rule", GUILayout.Width(100)))
            {
                rules.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
        
        // ... (Phần các nút Add, Restore, Organize không đổi)
        if (GUILayout.Button("Add New Rule"))
        {
            rules.Add(new OrganizationRule("New Rule", ".new_extension", "NewFolder"));
        }

        if (GUILayout.Button("Restore Default Rules"))
        {
            if (EditorUtility.DisplayDialog("Restore Defaults?", "Bạn có chắc muốn xóa tất cả các quy tắc hiện tại và khôi phục bộ quy tắc mặc định không?", "Yes", "No"))
            {
                LoadDefaultRules();
            }
        }
        
        EditorGUILayout.Space(20);

        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("Organize Selected Assets", GUILayout.Height(30)))
        {
            OrganizeSelectedAssets();
        }
        
        GUI.backgroundColor = new Color(1f, 0.8f, 0.8f);
        if (GUILayout.Button("Organize ALL Project Assets", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Full Organization",
                "CẢNH BÁO: Hành động này sẽ duyệt và di chuyển TẤT CẢ các tài sản trong dự án của bạn dựa trên các quy tắc đã định nghĩa.\n\n" +
                "Bạn có chắc chắn muốn tiếp tục không? (Nên backup dự án trước)", "Yes, I am sure", "Cancel"))
            {
                OrganizeAllAssets();
            }
        }
        GUI.backgroundColor = Color.white;
    }

    /// <summary>
    /// Chuyển đổi đường dẫn tuyệt đối thành đường dẫn tương đối so với thư mục dự án.
    /// </summary>
    private string GetRelativePath(string absolutePath)
    {
        // Lấy đường dẫn thư mục dự án, ví dụ: "C:/Users/You/MyUnityProject/"
        // Chúng ta cần loại bỏ phần "/Assets" để có đường dẫn gốc của dự án
        string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
        
        if (absolutePath.StartsWith(projectPath))
        {
            // Trả về đường dẫn tương đối, ví dụ: "Assets/MyFolder/MySubfolder"
            return absolutePath.Substring(projectPath.Length).Replace("\\", "/");
        }
        else
        {
            // Nếu đường dẫn nằm ngoài thư mục dự án, trả về chính nó (trường hợp hiếm)
            Debug.LogWarning("Selected folder is outside of the project directory. This may not work as expected.");
            return absolutePath;
        }
    }

    // ... (Các hàm LoadDefaultRules, OrganizeSelectedAssets, OrganizeAllAssets, Organize không đổi)
    private void LoadDefaultRules()
    {
        rules.Clear();
        rules.Add(new OrganizationRule("Prefabs", ".prefab", "Prefabs"));
        rules.Add(new OrganizationRule("Scenes", ".unity", "Scenes"));
        rules.Add(new OrganizationRule("Scripts", ".cs", "Scripts"));
        rules.Add(new OrganizationRule("Textures", ".png,.jpg,.jpeg,.tga,.tiff,.psd,.bmp", "Textures"));
        rules.Add(new OrganizationRule("Materials", ".mat", "Materials"));
        rules.Add(new OrganizationRule("3D Models", ".fbx,.obj,.blend,.max,.dae", "Models"));
        rules.Add(new OrganizationRule("Audio Clips", ".wav,.mp3,.ogg,.aif", "Audios"));
        rules.Add(new OrganizationRule("Audio Mixers", ".mixer", "Audio/Mixers"));
        rules.Add(new OrganizationRule("Animations", ".anim", "Animations"));
        rules.Add(new OrganizationRule("Animations", ".controller", "Animators"));
        rules.Add(new OrganizationRule("Fonts", ".ttf,.otf", "Fonts"));
        rules.Add(new OrganizationRule("Shaders", ".shader,.shadergraph", "Shaders"));;
    }
    
    private void OrganizeSelectedAssets()
    {
        var selectedObjects = Selection.GetFiltered<Object>(SelectionMode.Assets);
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Assets Selected", "Vui lòng chọn các tài sản trong cửa sổ Project để sắp xếp.", "OK");
            return;
        }

        List<string> assetPaths = selectedObjects.Select(AssetDatabase.GetAssetPath).ToList();
        Organize(assetPaths);
    }
    
    private void OrganizeAllAssets()
    {
        List<string> allAssetPaths = AssetDatabase.GetAllAssetPaths().ToList();
        Organize(allAssetPaths);
    }

    private void Organize(List<string> assetPaths)
    {
        int movedCount = 0;
        var foldersToCreate = new HashSet<string>();
        var assetsToMove = new List<(string oldPath, string newPath)>();

        foreach (string path in assetPaths)
        {
            if (string.IsNullOrEmpty(path) || Directory.Exists(path) || path.StartsWith("Assets/Editor"))
            {
                continue;
            }

            string extension = Path.GetExtension(path).ToLower();

            foreach (var rule in rules)
            {
                var extensions = rule.fileExtensions.ToLower().Split(',').Select(e => e.Trim());
                if (extensions.Contains(extension))
                {
                    // Xây dựng đường dẫn thư mục đích
                    string finalTargetPath = string.IsNullOrEmpty(baseFolder) ? rule.targetFolder : Path.Combine(baseFolder, rule.targetFolder);
                    string fullTargetFolderPath = Path.Combine("Assets", finalTargetPath).Replace("\\", "/");

                    foldersToCreate.Add(fullTargetFolderPath);

                    string fileName = Path.GetFileName(path);
                    string newPath = Path.Combine(fullTargetFolderPath, fileName);

                    if (path != newPath)
                    {
                        assetsToMove.Add((path, newPath));
                    }
                    
                    break; 
                }
            }
        }

        foreach (var folderPath in foldersToCreate)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"Created directory: {folderPath}");
            }
        }
        
        AssetDatabase.Refresh();

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var moveAction in assetsToMove)
            {
                string result = AssetDatabase.MoveAsset(moveAction.oldPath, moveAction.newPath);
                if (string.IsNullOrEmpty(result))
                {
                    Debug.Log($"Moved: {moveAction.oldPath} -> {moveAction.newPath}");
                    movedCount++;
                }
                else
                {
                    Debug.LogError($"Failed to move {moveAction.oldPath} to {moveAction.newPath}: {result}");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }

        if(movedCount > 0)
        {
             EditorUtility.DisplayDialog("Organization Complete", $"Đã di chuyển thành công {movedCount} tài sản.", "OK");
        }
        else
        {
            Debug.Log("No assets were moved. They might already be in the correct folders or have no matching rules.");
        }
    }
}