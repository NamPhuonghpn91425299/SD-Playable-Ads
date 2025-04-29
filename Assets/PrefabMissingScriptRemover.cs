using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

public class PrefabMissingScriptRemover : EditorWindow
{
    private List<GameObject> prefabs = new List<GameObject>();
    private ReorderableList reorderableList;
    private int numberOfPrefabsToAdd = 1;
    private Vector2 scrollPosition;
    private Vector2 listScrollPosition;

    [MenuItem("Tools/Prefab Missing Script Remover")]
    public static void ShowWindow()
    {
        GetWindow<PrefabMissingScriptRemover>("Prefab Missing Script Remover");
    }

    private void OnEnable()
    {
        reorderableList = new ReorderableList(prefabs, typeof(GameObject), true, true, true, true);
        reorderableList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Prefabs");
        };
        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            prefabs[index] = (GameObject)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), prefabs[index], typeof(GameObject), false);
        };
        reorderableList.onAddCallback = (ReorderableList list) => {
            prefabs.Add(null);
        };
        reorderableList.onRemoveCallback = (ReorderableList list) => {
            prefabs.RemoveAt(list.index);
        };
    }

private void OnGUI()
{
    scrollPosition = GUILayout.BeginScrollView(scrollPosition);

    GUILayout.Label("Prefab Missing Script Remover", EditorStyles.boldLabel);

    listScrollPosition = GUILayout.BeginScrollView(listScrollPosition, GUILayout.Height(300));
    reorderableList.DoLayoutList();
    GUILayout.EndScrollView();

    HandleDragAndDrop();

    if (GUILayout.Button("Scan and Remove Missing Scripts"))
    {
        foreach (var prefab in prefabs)
        {
            if (prefab != null)
            {
                RemoveAllMissingScripts(prefab);
            }
        }
    }

    if (GUILayout.Button("Disable Colliders in Prefabs"))
    {
        foreach (var prefab in prefabs)
        {
            if (prefab != null)
            {
                DisableCollidersInPrefab(prefab);
            }
        }
    }

    if (GUILayout.Button("Clear List"))
    {
        
        prefabs.Clear();
    }

    GUILayout.EndScrollView();
}

private void DisableCollidersInPrefab(GameObject gameObject)
{
    string path = AssetDatabase.GetAssetPath(gameObject);
    if (string.IsNullOrEmpty(path))
    {
        // Handle GameObject instance from scene
        DisableCollidersInChildren(gameObject);
        Debug.Log("All colliders disabled in GameObject: " + gameObject.name);
    }
    else
    {
        // Handle prefab asset
        GameObject prefabInstance = (GameObject)PrefabUtility.LoadPrefabContents(path);

        if (prefabInstance != null)
        {
            DisableCollidersInChildren(prefabInstance);
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
            PrefabUtility.UnloadPrefabContents(prefabInstance);

            Debug.Log("All colliders disabled in prefab: " + gameObject.name);
        }
        else
        {
            Debug.LogError("Failed to load prefab: " + gameObject.name);
        }
    }
}

private void DisableCollidersInChildren(GameObject obj)
{
    foreach (Transform child in obj.transform)
    {
        DisableCollidersInChildren(child.gameObject);
    }

    Collider collider = obj.GetComponent<Collider>();
    if (collider != null)
    {
        collider.enabled = false;
    }
}

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Ném cả cụm Prefabs vào đây cho tiện !!!");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        GameObject prefab = draggedObject as GameObject;
                        if (prefab != null)
                        {
                            prefabs.Add(prefab);
                        }
                    }
                }
                Event.current.Use();
                break;
        }
    }

    private void RemoveAllMissingScripts(GameObject gameObject)
    {
        string path = AssetDatabase.GetAssetPath(gameObject);
        if (string.IsNullOrEmpty(path))
        {
            // Xử lý GameObject instance từ scene
            RemoveMissingScriptsInChildren(gameObject);
            Debug.Log("All missing scripts removed from GameObject: " + gameObject.name);
        }
        else
        {
            // Xử lý prefab asset
            GameObject prefabInstance = (GameObject)PrefabUtility.LoadPrefabContents(path);

            if (prefabInstance != null)
            {
                RemoveMissingScriptsInChildren(prefabInstance);
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                PrefabUtility.UnloadPrefabContents(prefabInstance);

                Debug.Log("All missing scripts removed from prefab: " + gameObject.name);
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + gameObject.name);
            }
        }
    }

    private void RemoveMissingScriptsInChildren(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            RemoveMissingScriptsInChildren(child.gameObject);
        }

        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
    }
}