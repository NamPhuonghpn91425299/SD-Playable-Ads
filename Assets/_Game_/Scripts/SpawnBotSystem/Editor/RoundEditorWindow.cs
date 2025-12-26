using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Collections.Generic;

public class RoundEditorWindow : EditorWindow
{
    // --- Dữ liệu ---
    private RoundSO targetRoundSO;
    private SerializedObject serializedTarget;
    private ReorderableList reorderableList;
    
    // --- Trạng thái giao diện ---
    private int selectedStepIndex = -1;
    private Vector2 masterScrollPos;
    private Vector2 detailScrollPos;

    /// <summary>
    /// Một class trung gian để bọc dữ liệu của RoundSO trước khi serialize ra JSON,
    /// vì JsonUtility không thể serialize một ScriptableObject trực tiếp.
    /// </summary>
    [System.Serializable]
    private class RoundDataWrapper
    {
        public string RoundName;
        public float DelayAfterComplete;
        public List<BotWave> SpawnSteps;
    }

    [MenuItem("Tools/SpawnSystem/Round Editor Pro")]
    public static void ShowWindow()
    {
        GetWindow<RoundEditorWindow>("Round Editor Pro");
    }

    #region Initialization & Lifecycle

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
        OnSelectionChanged();
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if (Selection.activeObject is RoundSO roundSO)
        {
            targetRoundSO = roundSO;
            serializedTarget = new SerializedObject(targetRoundSO);
            SetupReorderableList();
            selectedStepIndex = -1;
        }
        else
        {
            targetRoundSO = null;
            serializedTarget = null;
            reorderableList = null;
        }
        Repaint();
    }

    #endregion

    #region GUI Drawing

    private void OnGUI()
    {
        if (targetRoundSO == null || serializedTarget == null)
        {
            DrawWelcomeScreen();
            return;
        }

        serializedTarget.Update();

        DrawHeader();
        
        EditorGUILayout.BeginHorizontal();
        DrawMasterPanel();
        DrawDetailPanel();
        EditorGUILayout.EndHorizontal();

        if (serializedTarget.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(targetRoundSO);
        }
        
        EditorGUILayout.Space(20);
        DrawCreateNewButton();
    }

    private void DrawWelcomeScreen()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Round Editor Pro", EditorStyles.boldLabel, GUILayout.Height(40));
        EditorGUILayout.HelpBox("Select a RoundSO asset to begin editing, or create a new one.", MessageType.Info);
        if (GUILayout.Button("Create New Round SO Asset", GUILayout.Height(30)))
        {
            CreateNewRoundSO();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Editing: " + targetRoundSO.name, EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedTarget.FindProperty("RoundName"));
        EditorGUILayout.PropertyField(serializedTarget.FindProperty("DelayAfterComplete"));
        EditorGUILayout.Space();
        DrawImportExportButtons();
        EditorGUILayout.Space();
    }

    private void DrawImportExportButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Export to JSON"))
        {
            ExportToJson(targetRoundSO);
        }
        if(GUILayout.Button("Import from JSON (Creates New)"))
        {
            ImportFromJson();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawMasterPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(280), GUILayout.ExpandHeight(true));
        EditorGUILayout.LabelField("Spawn Steps", EditorStyles.boldLabel);
        masterScrollPos = EditorGUILayout.BeginScrollView(masterScrollPos, "box");
        if (reorderableList != null)
        {
            reorderableList.DoLayoutList();
        }
        EditorGUILayout.EndScrollView();
        DrawSummary();
        EditorGUILayout.EndVertical();
    }

    private void DrawDetailPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if(serializedTarget == null || serializedTarget.targetObject == null) return;
        SerializedProperty stepsProperty = serializedTarget.FindProperty("SpawnSteps");
        if (selectedStepIndex >= 0 && selectedStepIndex < stepsProperty.arraySize)
        {
            SerializedProperty selectedStepProp = stepsProperty.GetArrayElementAtIndex(selectedStepIndex);
            BotType botType = (BotType)selectedStepProp.FindPropertyRelative("BotToSpawn").enumValueIndex;
            EditorGUILayout.LabelField($"Details for: {botType} (Step {selectedStepIndex + 1})", EditorStyles.boldLabel);
            detailScrollPos = EditorGUILayout.BeginScrollView(detailScrollPos, "box");
            DrawStepDetails(selectedStepProp);
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a step from the list on the left to see its details.", MessageType.Info);
        }
        EditorGUILayout.EndVertical();
    }
    
    private void DrawStepDetails(SerializedProperty stepProperty)
    {
        SerializedProperty botToSpawnProp = stepProperty.FindPropertyRelative("BotToSpawn");
        //SerializedProperty botMoveTypeProp = stepProperty.FindPropertyRelative("botMoveType");
        SerializedProperty quantityProp = stepProperty.FindPropertyRelative("Quantity");
        SerializedProperty delayProp = stepProperty.FindPropertyRelative("DelayBetweenSpawns");
        SerializedProperty conditionsProp = stepProperty.FindPropertyRelative("Conditions");
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Preview:", GUILayout.Width(60));
        BotType botType = (BotType)botToSpawnProp.enumValueIndex;
        if (Application.isPlaying && BotSpawnManager.Instance != null)
        {
            var definition = BotSpawnManager.Instance.GetDefinitionForType(botType);
            if (definition != null && GUILayout.Button("Ping Prefab", GUILayout.Width(80)))
            {
                EditorGUIUtility.PingObject(definition.Prefab);
            }
        }
        else { EditorGUILayout.LabelField("(Available in Play Mode)"); }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(botToSpawnProp);
        //EditorGUILayout.PropertyField(botMoveTypeProp);
        EditorGUILayout.PropertyField(quantityProp);
        EditorGUILayout.PropertyField(delayProp);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(conditionsProp, true);
    }
    
    private void DrawSummary()
    {
        GUILayout.BeginVertical("box");
        int totalBots = targetRoundSO.TotalBotCount;
        EditorGUILayout.LabelField("Total Scripted Bots:", totalBots.ToString(), EditorStyles.boldLabel);
        GUILayout.EndVertical();
    }

    #endregion

    #region ReorderableList Setup

    private void SetupReorderableList()
    {
        SerializedProperty stepsProperty = serializedTarget.FindProperty("SpawnSteps");
        reorderableList = new ReorderableList(serializedTarget, stepsProperty, true, true, true, true);
        reorderableList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Click to select, drag to reorder"); };
        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            BotType botType = (BotType)element.FindPropertyRelative("BotToSpawn").enumValueIndex;
            int quantity = element.FindPropertyRelative("Quantity").intValue;
            string label = $"Step {index + 1}: {botType}";
            string details = $"{quantity} unit(s)";
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.6f, EditorGUIUtility.singleLineHeight), label, EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight), details);
        };
        reorderableList.onSelectCallback = (ReorderableList list) => { selectedStepIndex = list.index; Repaint(); };
        reorderableList.onAddCallback = (ReorderableList list) => {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            selectedStepIndex = index;
            var newStep = list.serializedProperty.GetArrayElementAtIndex(index);
            newStep.FindPropertyRelative("Quantity").intValue = 1;
        };
        reorderableList.onRemoveCallback = (ReorderableList list) => {
             if (EditorUtility.DisplayDialog("Confirm Deletion", "Are you sure you want to remove this step?", "Yes", "No"))
             {
                 ReorderableList.defaultBehaviours.DoRemoveButton(list);
                 if (selectedStepIndex >= list.serializedProperty.arraySize) { selectedStepIndex = list.serializedProperty.arraySize - 1; }
                 Repaint();
             }
        };
    }

    #endregion

    #region Utility Functions
    
    private void DrawCreateNewButton()
    {
        if (GUILayout.Button("Create New RoundSO Asset")) { CreateNewRoundSO(); }
    }

    private void CreateNewRoundSO()
    {
        RoundSO round = CreateInstance<RoundSO>();
        string directory = "Assets/Rounds";
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        string path = AssetDatabase.GenerateUniqueAssetPath($"{directory}/Round_SO_.asset");
        AssetDatabase.CreateAsset(round, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = round;
    }

    private void ExportToJson(RoundSO round)
    {
        var dataWrapper = new RoundDataWrapper { RoundName = round.RoundName, DelayAfterComplete = round.DelayAfterComplete, SpawnSteps = round.SpawnSteps };
        string jsonData = JsonUtility.ToJson(dataWrapper, true);
        string path = EditorUtility.SaveFilePanel("Export Round to JSON", "Assets", $"{round.name}.json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, jsonData);
            EditorUtility.DisplayDialog("Success", $"Exported Round data to {path}", "OK");
        }
    }

    private void ImportFromJson()
    {
        string path = EditorUtility.OpenFilePanel("Import Round from JSON", "Assets", "json");
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            string jsonData = File.ReadAllText(path);
            RoundDataWrapper dataWrapper = JsonUtility.FromJson<RoundDataWrapper>(jsonData);
            if (dataWrapper == null) throw new System.Exception("JSON data could not be parsed.");
            
            RoundSO newRound = CreateInstance<RoundSO>();
            newRound.RoundName = dataWrapper.RoundName;
            newRound.DelayAfterComplete = dataWrapper.DelayAfterComplete;
            newRound.SpawnSteps = dataWrapper.SpawnSteps;
            
            string directory = "Assets/Rounds";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            string fileName = Path.GetFileNameWithoutExtension(path) + "_imported.asset";
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{fileName}");
            AssetDatabase.CreateAsset(newRound, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newRound;
            EditorUtility.DisplayDialog("Success", $"Successfully imported round data to:\n{assetPath}", "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Import Error", $"Failed to import JSON file.\nError: {ex.Message}", "OK");
        }
    }
    
    #endregion
}