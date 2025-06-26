#if UNITY_EDITOR
// File: Editor/RewardConfigDrawer.cs
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RewardConfig))]
public class RewardConfigDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        try
        {
            SerializedProperty rewardPrefabProp = property.FindPropertyRelative("rewardPrefab");
            SerializedProperty rewardTypeProp = property.FindPropertyRelative("rewardType");
            string rewardSpecificName = "";

            if (rewardPrefabProp != null && rewardPrefabProp.objectReferenceValue != null)
            {
                rewardSpecificName = $": {rewardPrefabProp.objectReferenceValue.name}";
            }
            else if (rewardTypeProp != null && rewardTypeProp.propertyType == SerializedPropertyType.Enum)
            {
                try
                {
                    if (rewardTypeProp.enumValueIndex >= 0 && rewardTypeProp.enumValueIndex < rewardTypeProp.enumDisplayNames.Length)
                        rewardSpecificName = $": {rewardTypeProp.enumDisplayNames[rewardTypeProp.enumValueIndex]}";
                    else
                        rewardSpecificName = ": (Invalid Enum Index)";
                }
                catch (System.Exception) { rewardSpecificName = ": (Error Reading Enum)"; }
            }

            string foldoutLabel = $"{label.text}{rewardSpecificName}";

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, foldoutLabel, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect currentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);

                // Vẽ từng trường một cách tường minh
                DrawPropertyChild(ref currentRect, property.FindPropertyRelative("rewardPrefab"));
                DrawPropertyChild(ref currentRect, property.FindPropertyRelative("rewardType"));
                DrawPropertyChild(ref currentRect, property.FindPropertyRelative("isNotUse"));
                DrawPropertyChild(ref currentRect, property.FindPropertyRelative("rewardQuantity"));
                DrawPropertyChild(ref currentRect, property.FindPropertyRelative("RewardDelaySpawn"));
                DrawPropertyChild(ref currentRect, property.FindPropertyRelative("WaitToSpawn"));

                // Sử dụng tên biến chính xác từ class RewardConfig của bạn
                SerializedProperty isSpawnOnBotKillProp = property.FindPropertyRelative("IsSpawmOnBotKill"); // ĐÚNG TÊN
                DrawPropertyChild(ref currentRect, isSpawnOnBotKillProp);

                // Kiểm tra giá trị của isSpawnOnBotKillProp SAU KHI nó được vẽ
                if (isSpawnOnBotKillProp != null && isSpawnOnBotKillProp.boolValue)
                {
                    DrawPropertyChild(ref currentRect, property.FindPropertyRelative("BotkillSpawn")); // ĐÚNG TÊN
                }

                EditorGUI.indentLevel--;
            }
        }
        finally
        {
            EditorGUI.EndProperty();
        }
    }

    private void DrawPropertyChild(ref Rect currentRect, SerializedProperty childProperty)
    {
        if (childProperty != null)
        {
            EditorGUI.PropertyField(currentRect, childProperty, true);
            currentRect.y += EditorGUI.GetPropertyHeight(childProperty, true) + EditorGUIUtility.standardVerticalSpacing;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // For foldout
        if (property.isExpanded)
        {
            height += CalculateChildPropertiesHeight(property.FindPropertyRelative("rewardPrefab"));
            height += CalculateChildPropertiesHeight(property.FindPropertyRelative("rewardType"));
            height += CalculateChildPropertiesHeight(property.FindPropertyRelative("isNotUse"));
            height += CalculateChildPropertiesHeight(property.FindPropertyRelative("rewardQuantity"));
            height += CalculateChildPropertiesHeight(property.FindPropertyRelative("RewardDelaySpawn"));
            height += CalculateChildPropertiesHeight(property.FindPropertyRelative("WaitToSpawn"));

            // Sử dụng tên biến chính xác từ class RewardConfig của bạn
            SerializedProperty isSpawnOnBotKillProp = property.FindPropertyRelative("IsSpawmOnBotKill"); // ĐÚNG TÊN
            if (isSpawnOnBotKillProp != null) // Luôn cộng chiều cao của checkbox
            {
                height += CalculateChildPropertiesHeight(isSpawnOnBotKillProp);
                // Chỉ cộng chiều cao của BotkillSpawn nếu checkbox được tick
                if (isSpawnOnBotKillProp.boolValue)
                {
                    height += CalculateChildPropertiesHeight(property.FindPropertyRelative("BotkillSpawn")); // ĐÚNG TÊN
                }
            }
        }
        return height;
    }

    private float CalculateChildPropertiesHeight(SerializedProperty childProperty)
    {
        if (childProperty != null)
        {
            return EditorGUI.GetPropertyHeight(childProperty, true) + EditorGUIUtility.standardVerticalSpacing;
        }
        return 0;
    }
}
#endif // UNITY_EDITOR