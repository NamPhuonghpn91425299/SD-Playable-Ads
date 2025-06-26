#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class FindMissingScripts : EditorWindow
{
    private List<GameObject> objectsWithMissingScripts = new List<GameObject>();
    private List<GameObject> objectsWithNullTextures = new List<GameObject>();
    private Vector2 scrollPos;
    private Vector2 nullTextureScrollPos;
    private bool showNullTextures = false;

    [MenuItem("Tools/Find Missing Scripts In Scene")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FindMissingScripts));
    }

    private void OnEnable()
    {
        EditorApplication.update += UpdateMissingScripts;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateMissingScripts;
    }

    private void OnGUI()
    {
        GUILayout.Label("Find Missing Scripts", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Find Missing Scripts in Scene"))
        {
            FindInCurrentScene();
        }

        GUILayout.Space(1);
        GUILayout.Label("Objects with Missing Scripts:", EditorStyles.boldLabel);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));

        if (objectsWithMissingScripts.Count > 0)
        {
            foreach (GameObject go in objectsWithMissingScripts)
            {
                if (GUILayout.Button(go.name, GUILayout.ExpandWidth(true)))
                {
                    Selection.activeGameObject = go;
                    EditorGUIUtility.PingObject(go);
                }
            }
        }
        else
        {
            GUILayout.Label("No objects with missing scripts found.");
        }

        EditorGUILayout.EndScrollView();
        GUILayout.Space(1);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Select All Objects with Missing Scripts"))
        {
            SelectObjectsWithMissingScripts();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.red;

        if (GUILayout.Button("Remove Missing Scripts from Selected Objects"))
        {
            RemoveMissingScriptsFromSelectedObjects();
            FindInCurrentScene();
        }

        GUI.backgroundColor = originalColor;

        // NULL TEXTURE SECTION
        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        
        GUILayout.Label("Find Images with Null Textures", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Find Images with Null Textures in Scene"))
        {
            FindImagesWithNullTextures();
        }

        GUILayout.Space(1);
        showNullTextures = EditorGUILayout.Foldout(showNullTextures, $"Objects with Null Textures ({objectsWithNullTextures.Count})");
        
        if (showNullTextures)
        {
            nullTextureScrollPos = EditorGUILayout.BeginScrollView(nullTextureScrollPos, GUILayout.Height(150));

            if (objectsWithNullTextures.Count > 0)
            {
                foreach (GameObject go in objectsWithNullTextures)
                {
                    if (GUILayout.Button(go.name, GUILayout.ExpandWidth(true)))
                    {
                        Selection.activeGameObject = go;
                        EditorGUIUtility.PingObject(go);
                    }
                }
            }
            else
            {
                GUILayout.Label("No objects with null textures found.");
            }

            EditorGUILayout.EndScrollView();
            GUILayout.Space(1);

            if (GUILayout.Button("Select All Objects with Null Textures"))
            {
                SelectObjectsWithNullTextures();
            }
        }
    }

    private void UpdateMissingScripts()
    {
        for (int i = objectsWithMissingScripts.Count - 1; i >= 0; i--)
        {
            if (objectsWithMissingScripts[i] == null)
            {
                objectsWithMissingScripts.RemoveAt(i);
            }
        }

        for (int i = objectsWithNullTextures.Count - 1; i >= 0; i--)
        {
            if (objectsWithNullTextures[i] == null)
            {
                objectsWithNullTextures.RemoveAt(i);
            }
        }
    }

    private void FindInCurrentScene()
    {
        objectsWithMissingScripts.Clear();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && go.hideFlags == HideFlags.None)
            {
                UnityEngine.Component[] components = go.GetComponents<UnityEngine.Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        if (!objectsWithMissingScripts.Contains(go))
                        {
                            objectsWithMissingScripts.Add(go);
                        }
                        break;
                    }
                }
            }
        }
    }

    private void SelectObjectsWithMissingScripts()
    {
        if (objectsWithMissingScripts.Count > 0)
        {
            Selection.objects = objectsWithMissingScripts.ToArray();
        }
    }

    private void RemoveMissingScriptsFromSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            return;
        }

        foreach (GameObject go in selectedObjects)
        {
            var components = go.GetComponents<UnityEngine.Component>();
            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (components[i] == null)
                {
                    UnityEditor.Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                }
            }
        }
    }

    private void FindImagesWithNullTextures()
    {
        objectsWithNullTextures.Clear();
        Image[] allImages = Resources.FindObjectsOfTypeAll<Image>();

        foreach (Image image in allImages)
        {
            GameObject go = image.gameObject;
            
            // Skip objects in prefabs or that aren't in scene
            if (EditorUtility.IsPersistent(go.transform.root.gameObject) || go.hideFlags != HideFlags.None)
                continue;
                
            if (image.sprite == null)
            {
                if (!objectsWithNullTextures.Contains(go))
                {
                    objectsWithNullTextures.Add(go);
                }
            }
        }
        
        Debug.Log($"Found {objectsWithNullTextures.Count} Images with null textures");
    }

    private void SelectObjectsWithNullTextures()
    {
        if (objectsWithNullTextures.Count > 0)
        {
            Selection.objects = objectsWithNullTextures.ToArray();
        }
    }
}

#endif