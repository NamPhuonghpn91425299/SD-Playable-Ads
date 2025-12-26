// #if UNITY_EDITOR
// // File: Editor/BotConfigDrawer.cs
// using UnityEngine;
// using UnityEditor;
//
// [CustomPropertyDrawer(typeof(BotConfig))]
// public class BotConfigDrawer : PropertyDrawer
// {
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginProperty(position, label, property);
//         try
//         {
//             SerializedProperty botPrefabProp = property.FindPropertyRelative("botPrefab");
//             SerializedProperty botTypeProp = property.FindPropertyRelative("botType");
//             string botSpecificName = "";
//
//             if (botPrefabProp != null && botPrefabProp.objectReferenceValue != null)
//             {
//                 botSpecificName = $": {botPrefabProp.objectReferenceValue.name}";
//             }
//             else if (botTypeProp != null && botTypeProp.propertyType == SerializedPropertyType.Enum)
//             {
//                 try
//                 {
//                     if (botTypeProp.enumValueIndex >= 0 && botTypeProp.enumValueIndex < botTypeProp.enumDisplayNames.Length)
//                         botSpecificName = $": {botTypeProp.enumDisplayNames[botTypeProp.enumValueIndex]}";
//                     else
//                         botSpecificName = ": (Invalid Enum Index)";
//                 }
//                 catch (System.Exception) { botSpecificName = ": (Error Reading Enum)"; }
//             }
//
//             string foldoutLabel = $"{label.text}{botSpecificName}";
//
//             Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
//             property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, foldoutLabel, true);
//
//             if (property.isExpanded)
//             {
//                 EditorGUI.indentLevel++;
//                 Rect currentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
//
//                 // Vẽ từng trường một cách tường minh
//                 DrawPropertyChild(ref currentRect, property.FindPropertyRelative("botPrefab"));
//                 DrawPropertyChild(ref currentRect, property.FindPropertyRelative("botConfigSO"));
//                 DrawPropertyChild(ref currentRect, property.FindPropertyRelative("botType"));
//                 DrawPropertyChild(ref currentRect, property.FindPropertyRelative("isNotUse"));
//                 DrawPropertyChild(ref currentRect, property.FindPropertyRelative("isNotCount"));
//                 DrawPropertyChild(ref currentRect, property.FindPropertyRelative("botQuantity"));
//                 DrawPropertyChild(ref currentRect, property.FindPropertyRelative("botDelaySpawn"));
//                 DrawPropertyChild(ref currentRect, property.FindPropertyRelative("WaitToSpawn"));
//
//                 // Sử dụng tên biến chính xác từ class BotConfig của bạn
//                 SerializedProperty isSpawnOnBotKillProp = property.FindPropertyRelative("IsSpawmOnBotKill"); // ĐÚNG TÊN
//                 DrawPropertyChild(ref currentRect, isSpawnOnBotKillProp);
//
//                 // Kiểm tra giá trị của isSpawnOnBotKillProp SAU KHI nó được vẽ
//                 if (isSpawnOnBotKillProp != null && isSpawnOnBotKillProp.boolValue)
//                 {
//                     DrawPropertyChild(ref currentRect, property.FindPropertyRelative("BotkillSpawn")); // ĐÚNG TÊN
//                 }
//
//                 EditorGUI.indentLevel--;
//             }
//         }
//         finally
//         {
//             EditorGUI.EndProperty();
//         }
//     }
//
//     private void DrawPropertyChild(ref Rect currentRect, SerializedProperty childProperty)
//     {
//         if (childProperty != null)
//         {
//             EditorGUI.PropertyField(currentRect, childProperty, true); // true để hiển thị label mặc định của property
//             currentRect.y += EditorGUI.GetPropertyHeight(childProperty, true) + EditorGUIUtility.standardVerticalSpacing;
//         }
//         // Nếu childProperty là null, chúng ta không làm gì cả (không tăng currentRect.y)
//         // Hoặc có thể vẽ một label báo lỗi nếu muốn
//         // else if (childProperty == null) { /* Debug.LogError("Property not found during DrawPropertyChild"); */ }
//     }
//
//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
//         float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // For foldout
//         if (property.isExpanded)
//         {
//             height += CalculateChildPropertiesHeight(property.FindPropertyRelative("botPrefab"));
//             height += CalculateChildPropertiesHeight(property.FindPropertyRelative("botConfigSO"));
//             height += CalculateChildPropertiesHeight(property.FindPropertyRelative("botType"));
//             height += CalculateChildPropertiesHeight(property.FindPropertyRelative("isNotUse"));
//             height += CalculateChildPropertiesHeight(property.FindPropertyRelative("isNotCount"));
//             height += CalculateChildPropertiesHeight(property.FindPropertyRelative("botQuantity"));
//             height += CalculateChildPropertiesHeight(property.FindPropertyRelative("botDelaySpawn"));
//             height += CalculateChildPropertiesHeight(property.FindPropertyRelative("WaitToSpawn"));
//
//             // Sử dụng tên biến chính xác từ class BotConfig của bạn
//             SerializedProperty isSpawnOnBotKillProp = property.FindPropertyRelative("IsSpawmOnBotKill"); // ĐÚNG TÊN
//             if (isSpawnOnBotKillProp != null) // Luôn cộng chiều cao của checkbox
//             {
//                 height += CalculateChildPropertiesHeight(isSpawnOnBotKillProp);
//                 // Chỉ cộng chiều cao của BotkillSpawn nếu checkbox được tick
//                 if (isSpawnOnBotKillProp.boolValue)
//                 {
//                     height += CalculateChildPropertiesHeight(property.FindPropertyRelative("BotkillSpawn")); // ĐÚNG TÊN
//                 }
//             }
//         }
//         return height;
//     }
//
//     private float CalculateChildPropertiesHeight(SerializedProperty childProperty)
//     {
//         if (childProperty != null)
//         {
//             return EditorGUI.GetPropertyHeight(childProperty, true) + EditorGUIUtility.standardVerticalSpacing;
//         }
//         return 0; // Nếu property không tìm thấy, không cộng thêm chiều cao
//     }
// }
// #endif // UNITY_EDITOR
