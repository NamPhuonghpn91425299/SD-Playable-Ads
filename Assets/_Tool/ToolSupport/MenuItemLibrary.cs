#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


public static class MenuItemLibrary
{
    #region FIND REFERENCES

    [MenuItem("CONTEXT/Component/Find references")]
    private static void FindComponentReferences(MenuCommand menuCommand)
    {
        var target = menuCommand.context as Component;
        if (target != null)
        {
            FindReferencesTo(target);
        }
    }

    [MenuItem("CONTEXT/Component/Find references by scanning fields")]
    private static void FindComponentRefByFields(MenuCommand menuCommand)
    {
        var target = menuCommand.context as Component;
        if (target != null)
        {
            FindReferenceByFields(target);
        }
    }

    [MenuItem("GameObject/Find references")]
    private static void FindGameObjectRef(MenuCommand data)
    {
        var selected = Selection.activeObject;
        if (selected)
            FindReferencesTo(selected);
    }

    [MenuItem("GameObject/Find references by scanning fields")]
    private static void FindGameObjectRefByFields(MenuCommand data)
    {
        var selected = Selection.activeObject;
        if (selected)
            FindReferenceByFields(selected);
    }

    [MenuItem("GameObject/Find references (include find all components references)")]
    private static void FindGameObjectAndComponentsRef(MenuCommand data)
    {
        var selected = Selection.activeObject;
        if (selected)
            FindReferencesTo(selected);
        var go = selected as GameObject;
        if (go == null) return;
        var components = go.GetComponents<Component>();
        foreach (var component in components)
        {
            FindReferencesTo(component);
        }
    }

    [MenuItem("GameObject/Find references by scanning fields (include all components)")]
    private static void FindGameObjectAndComponentsRefByFields(MenuCommand data)
    {
        var selected = Selection.activeObject;
        if (selected)
            FindReferencesTo(selected);
        var go = selected as GameObject;
        if (go == null) return;
        var components = go.GetComponents<Component>();
        foreach (var component in components)
        {
            FindReferenceByFields(component);
        }
    }

    [MenuItem("Assets/Find references")]
    private static void FindReferencesToAsset(MenuCommand data)
    {
        var selected = Selection.activeObject;
        if (selected)
            FindReferencesTo(selected);
    }
    public static string GetTypeName(this Object obj)
    {
        return obj.GetType().ToString().Split('.').Last();
    }

    public static string Colorize(this string text, Color color)
    {
        Color32 color32 = color;
        string hex = "#" + color32.r.ToString("X2") + color32.g.ToString("X2") + color32.b.ToString("X2");
        return $"<color={hex}>" + text + "</color>";
    }

    private static void FindReferencesTo(Object target)
    {
        var referencedBy = new List<Object>();
        if (PrefabStageUtility.GetCurrentPrefabStage() != null) // In Prefab Mode
        {
            FindReferencesInPrefab(target);
            return;
        }

        var allObjectsInScene = Object.FindObjectsOfType<GameObject>(true);
        foreach (var go in allObjectsInScene)
        {
            if (PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab &&
                PrefabUtility.GetCorrespondingObjectFromSource(go) == target)
            {
                Debug.Log(
                    $"<color=cyan>{target.GetTypeName()} ({target.name})</color> is referenced by <color=yellow>{go.GetTypeName()}</color> in <color=orange>{go.name}</color>",
                    go);
                referencedBy.Add(go);
            }
            else
            {
                var components = go.GetComponents<Component>();
                FindMatch(target, components, referencedBy, go);
            }
        }

        Selection.objects = referencedBy.ToArray();

        if (referencedBy.Any())
            Selection.objects = referencedBy.ToArray();
        else
            Debug.Log($"<color=cyan>{target.GetTypeName()} ({target.name})</color> has no references in scene");
    }

    private static void FindReferenceByFields(Object target)
    {
        var targetType = target.GetType();

        var referencedBy = new List<Object>();
        var allObjectsInScene = Object.FindObjectsOfType<GameObject>(true);
        foreach (var go in allObjectsInScene)
        {
            var components = go.GetComponents<Component>();
            foreach (var component in components)
            {
                var fields = component.GetType().GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    Search(target, referencedBy, targetType, field, component);
                }
            }
        }

        Selection.objects = referencedBy.ToArray();

        if (referencedBy.Any())
            Selection.objects = referencedBy.ToArray();
        else
            Debug.Log($"<color=cyan>{target.GetTypeName()} ({target.name})</color> has no references in scene");
    }

    private static void FindReferencesInPrefab(Object target)
    {
        var allComponents = PrefabStageUtility.GetCurrentPrefabStage().FindComponentsOfType<Component>();
        var referencedBy = new List<Object>();

        FindMatch(target, allComponents, referencedBy);

        if (referencedBy.Any())
            Selection.objects = referencedBy.ToArray();
        else
            Debug.Log(
                $"<color=cyan>{target.GetTypeName()} ({target.name})</color> has no references in opening prefab");
    }

    private static void FindMatch(Object target, IEnumerable<Component> components, ICollection<Object> result,
        GameObject debugTarget = null)
    {
        foreach (var component in components)
        {
            if (component == null)
            {
                Debug.LogError("Detect a null component", debugTarget);
                continue;
            }

            var so = new SerializedObject(component);
            var sp = so.GetIterator();
            var parentName = "";
            var parentDisplayName = "";
            while (sp.NextVisible(true))
            {
                switch (sp.propertyType)
                {
                    case SerializedPropertyType.ObjectReference when sp.objectReferenceValue != target:
                        continue;
                    case SerializedPropertyType.ObjectReference:
                    {
                        if (sp.name == "data")
                            Debug.Log(
                                $"<color=cyan>{target.GetTypeName()} ({target.name})</color> is referenced by <color=yellow>{component.GetTypeName()}</color> in <color=orange>{component.name}</color>, element index <color=cyan>{sp.displayName.Split(' ')[1]}</color> in <color=yellow>{parentDisplayName}</color> (<color=orange>{parentName}</color>)",
                                component);
                        else
                            Debug.Log(
                                $"<color=cyan>{target.GetTypeName()} ({target.name})</color> is referenced by <color=yellow>{component.GetTypeName()}</color> in <color=orange>{component.name}</color>, property <color=yellow>{sp.displayName}</color> (<color=orange>{sp.name}</color>)",
                                component);
                        result.Add(component.gameObject);
                        break;
                    }
                    case SerializedPropertyType.Generic:
                        parentName = sp.name;
                        parentDisplayName = sp.displayName;
                        break;
                    case SerializedPropertyType.Integer:
                        break;
                    case SerializedPropertyType.Boolean:
                        break;
                    case SerializedPropertyType.Float:
                        break;
                    case SerializedPropertyType.String:
                        break;
                    case SerializedPropertyType.Color:
                        break;
                    case SerializedPropertyType.LayerMask:
                        break;
                    case SerializedPropertyType.Enum:
                        break;
                    case SerializedPropertyType.Vector2:
                        break;
                    case SerializedPropertyType.Vector3:
                        break;
                    case SerializedPropertyType.Vector4:
                        break;
                    case SerializedPropertyType.Rect:
                        break;
                    case SerializedPropertyType.ArraySize:
                        break;
                    case SerializedPropertyType.Character:
                        break;
                    case SerializedPropertyType.AnimationCurve:
                        break;
                    case SerializedPropertyType.Bounds:
                        break;
                    case SerializedPropertyType.Gradient:
                        break;
                    case SerializedPropertyType.Quaternion:
                        break;
                    case SerializedPropertyType.ExposedReference:
                        break;
                    case SerializedPropertyType.FixedBufferSize:
                        break;
                    case SerializedPropertyType.Vector2Int:
                        break;
                    case SerializedPropertyType.Vector3Int:
                        break;
                    case SerializedPropertyType.RectInt:
                        break;
                    case SerializedPropertyType.BoundsInt:
                        break;
                    case SerializedPropertyType.ManagedReference:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // OLD CODE: this code works like FindReferenceThroughFields function. It was a replace for finding reference by SerializedObject method.
            // var fields = component.GetType().GetFields(
            //     BindingFlags.NonPublic |
            //     BindingFlags.Public | BindingFlags.Instance);
            //
            // foreach (var field in fields)
            // {
            //     Search(target, result, targetType, field, component);
            // }

            // OLD TEST: this code doesn't work. It's used to compare attributes of an instance of a class inside component to target.
            // DeepSearch(target, result, targetType, component);
        }
    }

    private static void Search(Object target, ICollection<Object> result, Type targetType, FieldInfo field,
        Component component)
    {
        if (targetType.IsAssignableFrom(field.FieldType))
        {
            var value = field.GetValue(component);
            if ((Object)value != target) return;

            Debug.Log(
                $"<color=cyan>{target.GetTypeName()} ({target.name})</color> is referenced by <color=yellow>{component.GetTypeName()}</color> in <color=orange>{component.name}</color>, property <color=yellow>{field.Name}</color>",
                component);
            result.Add(component.gameObject);
        }
        else if (typeof(IList).IsAssignableFrom(field.FieldType))
        {
            var value = field.GetValue(component);
            if (value == null) return;
            for (var i = 0; i < ((IList)value).Count; i++)
            {
                var item = ((IList)value)[i];
                if (!targetType.IsInstanceOfType(item) || target != (Object)item)
                    continue;

                Debug.Log(
                    $"<color=cyan>{target.GetTypeName()} ({target.name})</color> is referenced by <color=yellow>{component.GetTypeName()}</color> in <color=orange>{component.name}</color>, element id <color=cyan>{i}</color> in <color=yellow>{field.Name}</color>",
                    component);
                result.Add(component.gameObject);
            }
        }

        // OLD TEST: this code doesn't work. It's used to compare attributes of an instance of a class inside component to target.
        // else
        // {
        //     var childFields = field.FieldType.GetFields();
        //     foreach (var chileField in childFields)
        //     {
        //         // if (!Search(target, targetType, chileField, field.GetValue(component))) continue;
        //         
        //         Debug.Log(
        //             $"<color=cyan>{target.GetTypeName()} ({target.name})</color> is referenced by <color=yellow>{component.GetTypeName()}</color> in <color=orange>{component.name}</color>, property <color=yellow>{field.Name}</color> (chileField",
        //             component);
        //         result.Add(component.gameObject);
        //     }
        // }
    }

    // OLD TEST: this code doesn't work. It's used to compare attributes of an instance of a class inside component to target.
    // private static void DeepSearch(Object target, ICollection<Object> result, Type targetType, Component component)
    // {
    //     var fields = component.GetType().GetFields(
    //         BindingFlags.NonPublic |
    //         BindingFlags.Public | BindingFlags.Instance);
    //     foreach (var field in fields)
    //     {
    //         var (isFound, path) = DeepSearch(target, targetType, field, component);
    //         if (!isFound) continue;
    //
    //         Debug.Log(
    //             $"<color=cyan>{target.GetTypeName()} ({target.name})</color> is referenced by <color=yellow>{component.name + "/" + component.GetTypeName() + path}</color>",
    //             component);
    //         result.Add(component.gameObject);
    //     }
    // }
    //
    // // OLD TEST: this code doesn't work. It's used to compare attributes of an instance of a class inside component to target.
    // private static (bool isFound, string path) DeepSearch(object target, Type targetType, FieldInfo rootInfo,
    //     object rootObject)
    // {
    //     var rootValue = rootInfo.GetValue(rootObject);
    //     if (rootValue == default) return (false, default);
    //     if (targetType.IsAssignableFrom(rootInfo.FieldType))
    //     {
    //         if (target == rootValue)
    //             return (true, $"/{rootInfo.Name}");
    //     }
    //     else if (typeof(IList).IsAssignableFrom(rootInfo.FieldType))
    //     {
    //         for (var i = 0; i < ((IList)rootValue).Count; i++)
    //         {
    //             var item = ((IList)rootValue)[i];
    //             if (!targetType.IsInstanceOfType(item) || target != item)
    //                 continue;
    //
    //             return (true, $"/{rootInfo.Name}[{i}]");
    //         }
    //
    //         return (false, default);
    //     }
    //     else
    //     {
    //         var fields = rootValue.GetType().GetFields(
    //             BindingFlags.NonPublic |
    //             BindingFlags.Public | BindingFlags.Instance);
    //
    //         var isFoundInChild = false;
    //         var allChildPath = "";
    //
    //         foreach (var field in fields)
    //         {
    //             var (isFound, childPath) = DeepSearch(target, targetType, field, rootValue);
    //             if (!isFound) continue;
    //             isFoundInChild = true;
    //             allChildPath += childPath;
    //         }
    //
    //         return isFoundInChild ? (true, $"{rootInfo.Name}/{allChildPath}") : (false, default);
    //     }
    //
    //     return (false, default);
    // }

    #endregion

    #region FIND MISSING REFERENCES

    [MenuItem("Horus Tool/Check/Missing References in Scene or Prefab")]
    private static void FindMissingReferences()
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        var components = prefabStage != null
            ? PrefabStageUtility.GetCurrentPrefabStage().FindComponentsOfType<Component>()
            : Object.FindObjectsOfType<Component>(true);

        foreach (var component in components)
        {
            FindMissingReferences(component);
        }
    }

    private static void FindMissingReferences(Component component)
    {
        var so = new SerializedObject(component);
        var sp = so.GetIterator();
        while (sp.NextVisible(true))
        {
            if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue == null &&
                sp.objectReferenceInstanceIDValue != 0)
            {
                Debug.LogError(
                    $"Property {sp.name.Colorize(Color.cyan)} in component <color=yellow>{component.GetTypeName()}</color> in <color=orange>{component.name}</color>  is missing reference.",
                    component.gameObject);
            }
        }
    }

    #endregion
}

#endif