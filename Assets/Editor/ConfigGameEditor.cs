#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(ConfigGame))]
public class ConfigGameEditor : Editor
{
    private ReorderableList fightRoundList;
    private SerializedProperty fightRoundsProperty;

    private void OnEnable()
    {
        fightRoundsProperty = serializedObject.FindProperty("fightRound");

        fightRoundList = new ReorderableList(serializedObject, fightRoundsProperty,
            draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);

        fightRoundList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Fight Rounds Configuration");
        };

        fightRoundList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = fightRoundList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2; // Thêm chút padding

            // Unity sẽ tự động sử dụng FightRoundDrawer để vẽ element này
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element, true)),
                element, true);
        };

        fightRoundList.elementHeightCallback = (int index) =>
        {
            SerializedProperty element = fightRoundList.serializedProperty.GetArrayElementAtIndex(index);
            // Cộng thêm một chút padding giữa các element
            return EditorGUI.GetPropertyHeight(element, true) + EditorGUIUtility.standardVerticalSpacing * 2;
        };

        // Xử lý khi thêm một phần tử mới
        fightRoundList.onAddCallback = (ReorderableList list) =>
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index; // Chọn phần tử vừa thêm
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            // Khởi tạo giá trị mặc định cho round mới nếu cần
            SerializedProperty roundNameProp = element.FindPropertyRelative("roundName");
            if (roundNameProp != null)
            {
                roundNameProp.stringValue = $"Round {index + 1}";
            }
            // Xóa các list con để bắt đầu sạch
            SerializedProperty botConfigsProp = element.FindPropertyRelative("botConfigs");
            if (botConfigsProp != null) botConfigsProp.arraySize = 0;
            SerializedProperty rewardConfigsProp = element.FindPropertyRelative("rewardConfig");
            if (rewardConfigsProp != null) rewardConfigsProp.arraySize = 0;

        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox("Configure game rounds here. Each round can have multiple bot and reward configurations.", MessageType.Info);
        EditorGUILayout.Space();

        fightRoundList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif