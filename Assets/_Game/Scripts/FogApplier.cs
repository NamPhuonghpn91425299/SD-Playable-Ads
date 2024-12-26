using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteAlways]
public class FogApplier : MonoBehaviour
{
    [SerializeField] private FogSetting _setting;
#if UNITY_EDITOR
    private static bool _hasShowWarning;
#endif
    private void Start()
    {
#if UNITY_EDITOR
        _hasShowWarning = true;
#endif
        if (_setting != null)
            _setting.Apply();
        else
            Debug.LogError("Không có FogSetting. Nếu không dùng tính năng này thì nên xoá bỏ component", gameObject);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (_setting == null || _hasShowWarning) return;

        if (RenderSettings.fog != _setting.Enable || RenderSettings.fogColor != _setting.Color ||
            RenderSettings.fogMode != _setting.Mode ||
            Math.Abs(RenderSettings.fogDensity - _setting.Density) > float.Epsilon ||
            Math.Abs(RenderSettings.fogStartDistance - _setting.Start) > float.Epsilon ||
            Math.Abs(RenderSettings.fogEndDistance - _setting.End) > float.Epsilon)
        {
            Selection.activeObject = _setting;
            _hasShowWarning = EditorUtility.DisplayDialog("FogApplier đang được áp dụng",
                "Chỉnh fog trong rendering window sẽ ra kết quả không mong muốn", "Đã hiểu");
            _setting.Apply();
        }
    }
#endif
}