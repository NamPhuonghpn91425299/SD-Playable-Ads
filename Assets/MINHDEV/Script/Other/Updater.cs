using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lớp được dùng để sử dụng Coroutine từ những lớp không phải MonoBehaviour
/// </summary>
public class Updater : MonoBehaviour
{
    private static Updater _instance;

    public static Updater Ins
    {
        get
        {
#if DEVELOPER_MODE && UNITY_EDITOR
            if (_instance == null)
            {
                UtilsHr.SpawnGameSingleton();
            }
#endif
            return _instance;
        }
    }

    public event UnityAction Tick;
    /// <summary>
    /// Updater for bot which can be turned off by <see cref="IsBotActive"/> variable
    /// </summary>
    public event UnityAction BotUpdater;
    public bool IsBotActive;

    private void Awake()
    {
        _instance = this;
        IsBotActive = true;
    }

    private void Update()
    {
        Tick?.Invoke();
        if (IsBotActive)
            BotUpdater?.Invoke();
    }
}