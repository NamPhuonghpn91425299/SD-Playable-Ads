#if UNITY_EDITOR
// File: Editor/FightRoundDrawer.cs
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
// using System.Collections.Generic; // Không cần Dictionary nữa nếu bỏ cache

[CustomPropertyDrawer(typeof(FightRound))]
public class FightRoundDrawer : PropertyDrawer
{
    // BỎ HOÀN TOÀN VIỆC SỬ DỤNG DICTIONARY ĐỂ CACHE REORDERABLELIST
    // private static Dictionary<string, ReorderableList> s_BotConfigLists = new Dictionary<string, ReorderableList>();
    // private static Dictionary<string, ReorderableList> s_RewardConfigLists = new Dictionary<string, ReorderableList>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
            EditorGUI.BeginProperty(position, label, property);
        try
        {
            Rect currentRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            SerializedProperty roundNameProp = property.FindPropertyRelative("roundName");
            if (roundNameProp != null)
            {
                EditorGUI.PropertyField(currentRect, roundNameProp, new GUIContent("Round Name"));
            }
            else
            {
                EditorGUI.LabelField(currentRect, "Error: 'roundName' property not found!");
            }
            currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            property.isExpanded = EditorGUI.Foldout(currentRect, property.isExpanded, "Round Details", true);
            currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                SerializedProperty botConfigsProp = property.FindPropertyRelative("botConfigs");
                if (botConfigsProp != null && botConfigsProp.isArray)
                {
                    // Tạo mới ReorderableList mỗi lần
                    ReorderableList botList = CreateNewList(property.serializedObject, botConfigsProp, "Bot Configurations", "Bot");
                    float listHeight = botList.GetHeight();
                    botList.DoList(new Rect(currentRect.x, currentRect.y, currentRect.width, listHeight));
                    currentRect.y += listHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                else
                {
                    EditorGUI.LabelField(new Rect(currentRect.x, currentRect.y, currentRect.width, EditorGUIUtility.singleLineHeight), "Error: 'botConfigs' array not found!");
                    currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                SerializedProperty rewardConfigsProp = property.FindPropertyRelative("rewardConfig");
                if (rewardConfigsProp != null && rewardConfigsProp.isArray)
                {
                    // Tạo mới ReorderableList mỗi lần
                    ReorderableList rewardList = CreateNewList(property.serializedObject, rewardConfigsProp, "Reward Configurations", "Reward");
                    float listHeight = rewardList.GetHeight();
                    rewardList.DoList(new Rect(currentRect.x, currentRect.y, currentRect.width, listHeight));
                    currentRect.y += listHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                else
                {
                    EditorGUI.LabelField(new Rect(currentRect.x, currentRect.y, currentRect.width, EditorGUIUtility.singleLineHeight), "Error: 'rewardConfig' array not found!");
                    currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
        }
        finally
        {
            EditorGUI.EndProperty();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // roundName
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;      // Foldout

        if (property.isExpanded)
        {
            SerializedProperty botConfigsProp = property.FindPropertyRelative("botConfigs");
            if (botConfigsProp != null && botConfigsProp.isArray)
            {
                // Tạo list tạm để lấy chiều cao
                ReorderableList botList = CreateNewList(property.serializedObject, botConfigsProp, "Bot Configurations", "Bot");
                height += botList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            } else {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            SerializedProperty rewardConfigsProp = property.FindPropertyRelative("rewardConfig");
            if (rewardConfigsProp != null && rewardConfigsProp.isArray)
            {
                // Tạo list tạm để lấy chiều cao
                ReorderableList rewardList = CreateNewList(property.serializedObject, rewardConfigsProp, "Reward Configurations", "Reward");
                height += rewardList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            } else {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }
        return height;
    }

    // Hàm helper để tạo ReorderableList mới
    private ReorderableList CreateNewList(SerializedObject serializedObject, SerializedProperty listProperty, string headerText, string elementPrefix)
    {
        // Quan trọng: Phải tạo một bản copy của listProperty cho mỗi ReorderableList
        // để tránh lỗi khi callback truy cập vào property đã bị thay đổi hoặc disposed.
        // Tuy nhiên, ReorderableList thường làm việc tốt nhất khi được truyền trực tiếp SerializedProperty.
        // Vấn đề "disposed" thường liên quan đến việc property được giữ lại quá lâu trong cache.

        ReorderableList list = new ReorderableList(serializedObject, listProperty,
            draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);

        list.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, headerText);

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            // Rất quan trọng: Lấy lại element từ list.serializedProperty mỗi lần vẽ
            // vì listProperty ban đầu có thể không còn hợp lệ hoặc đã thay đổi.
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element)), element, new GUIContent($"{elementPrefix} #{index + 1}"), true);
        };

        list.elementHeightCallback = (int index) =>
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element) + 4;
        };
        return list;
    }
}
#endif