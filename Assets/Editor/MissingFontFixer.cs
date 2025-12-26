using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace PlayableAds.Editor
{
    public class MissingFontFixer : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<TextComponentInfo> missingFontTexts = new List<TextComponentInfo>();
        private Font replacementFont;
        private bool searchInPrefabs = true;
        private bool searchInScenes = true;
        
        private class TextComponentInfo
        {
            public Text textComponent;
            public string path;
            public GameObject gameObject;
            public bool selected;
            
            public TextComponentInfo(Text text, string path, GameObject go)
            {
                this.textComponent = text;
                this.path = path;
                this.gameObject = go;
                this.selected = true;
            }
        }
        
        [MenuItem("Tools/Missing Font Fixer")]
        public static void ShowWindow()
        {
            var window = GetWindow<MissingFontFixer>("Missing Font Fixer");
            window.minSize = new Vector2(500, 400);
        }
        
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Title
            EditorGUILayout.LabelField("Missing Font Finder & Fixer", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Search options
            EditorGUILayout.LabelField("Search Options", EditorStyles.boldLabel);
            searchInPrefabs = EditorGUILayout.Toggle("Search in Prefabs", searchInPrefabs);
            searchInScenes = EditorGUILayout.Toggle("Search in Scenes", searchInScenes);
            EditorGUILayout.Space();
            
            // Search button
            if (GUILayout.Button("Find Missing Fonts", GUILayout.Height(30)))
            {
                FindMissingFonts();
            }
            
            EditorGUILayout.Space();
            
            // Results section
            if (missingFontTexts.Count > 0)
            {
                EditorGUILayout.LabelField($"Found {missingFontTexts.Count} Text components with missing fonts:", EditorStyles.boldLabel);
                
                // Replacement font field
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Replacement Font:", GUILayout.Width(120));
                replacementFont = (Font)EditorGUILayout.ObjectField(replacementFont, typeof(Font), false);
                EditorGUILayout.EndHorizontal();
                
                // Select/Deselect all buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All", GUILayout.Width(100)))
                {
                    foreach (var item in missingFontTexts)
                        item.selected = true;
                }
                if (GUILayout.Button("Deselect All", GUILayout.Width(100)))
                {
                    foreach (var item in missingFontTexts)
                        item.selected = false;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // List of missing font components
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                for (int i = 0; i < missingFontTexts.Count; i++)
                {
                    var info = missingFontTexts[i];
                    
                    EditorGUILayout.BeginHorizontal("Box");
                    
                    // Checkbox
                    info.selected = EditorGUILayout.Toggle(info.selected, GUILayout.Width(20));
                    
                    // Path
                    EditorGUILayout.LabelField(info.path, GUILayout.MinWidth(200));
                    
                    // Select button
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = info.gameObject;
                        EditorGUIUtility.PingObject(info.gameObject);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.Space();
                
                // Fix button
                GUI.enabled = replacementFont != null && missingFontTexts.Any(x => x.selected);
                if (GUILayout.Button("Fix Selected Missing Fonts", GUILayout.Height(30)))
                {
                    FixSelectedMissingFonts();
                }
                GUI.enabled = true;
            }
            else if (missingFontTexts.Count == 0 && GUILayout.Button("Clear Results"))
            {
                missingFontTexts.Clear();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void FindMissingFonts()
        {
            missingFontTexts.Clear();
            
            if (searchInPrefabs)
            {
                FindMissingFontsInPrefabs();
            }
            
            if (searchInScenes)
            {
                FindMissingFontsInScenes();
            }
            
            if (missingFontTexts.Count == 0)
            {
                EditorUtility.DisplayDialog("Missing Font Finder", 
                    "No Text components with missing fonts found!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Missing Font Finder", 
                    $"Found {missingFontTexts.Count} Text components with missing fonts.", "OK");
            }
        }
        
        private void FindMissingFontsInPrefabs()
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    Text[] texts = prefab.GetComponentsInChildren<Text>(true);
                    
                    foreach (Text text in texts)
                    {
                        if (text.font == null)
                        {
                            string fullPath = $"{path}/{GetGameObjectPath(text.transform, prefab.transform)}";
                            missingFontTexts.Add(new TextComponentInfo(text, fullPath, prefab));
                        }
                    }
                }
            }
        }
        
        private void FindMissingFontsInScenes()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            
            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                
                // Load scene additively to check
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, 
                    UnityEditor.SceneManagement.OpenSceneMode.Additive);
                
                GameObject[] rootObjects = scene.GetRootGameObjects();
                
                foreach (GameObject root in rootObjects)
                {
                    Text[] texts = root.GetComponentsInChildren<Text>(true);
                    
                    foreach (Text text in texts)
                    {
                        if (text.font == null)
                        {
                            string fullPath = $"{scenePath}/{GetGameObjectPath(text.transform, null)}";
                            missingFontTexts.Add(new TextComponentInfo(text, fullPath, text.gameObject));
                        }
                    }
                }
                
                // Close the scene if it wasn't originally open
                if (!UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().Equals(scene))
                {
                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
                }
            }
        }
        
        private string GetGameObjectPath(Transform transform, Transform root)
        {
            string path = transform.name;
            Transform current = transform.parent;
            
            while (current != null && current != root)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            
            return path;
        }
        
        private void FixSelectedMissingFonts()
        {
            if (replacementFont == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a replacement font first!", "OK");
                return;
            }
            
            int fixedCount = 0;
            
            foreach (var info in missingFontTexts)
            {
                if (info.selected && info.textComponent != null)
                {
                    Undo.RecordObject(info.textComponent, "Fix Missing Font");
                    info.textComponent.font = replacementFont;
                    EditorUtility.SetDirty(info.textComponent);
                    fixedCount++;
                }
            }
            
            if (fixedCount > 0)
            {
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Success", 
                    $"Fixed {fixedCount} Text components with the selected font.", "OK");
                
                // Re-scan to update the list
                FindMissingFonts();
            }
        }
    }
}
