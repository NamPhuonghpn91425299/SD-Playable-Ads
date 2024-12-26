using System;
using UnityEngine;

public enum TimeCategory
{
    General,
    Player,
    Game,
    Exactly
}

public static class PlayerTime
{
    public static float time { get; private set; }
    private static float _localTimeScale = 1;
    public static float LocalTimeScale => _localTimeScale;

    public static event Action<float, float> OnTimeScaleChange;

    public static float timeScale
    {
        get => Time.timeScale * _localTimeScale;
        set
        {
            var previousTimeScale = _localTimeScale;
            if (Time.timeScale != 0)
                _localTimeScale = value / Time.timeScale;
            OnTimeScaleChange?.Invoke(previousTimeScale, _localTimeScale);
           // SoundManager.Instance.SetSpeed(AudioGroup.Player, timeScale);
        }
    }

    private static float _deltaTime;
    public static float deltaTime => _deltaTime;

    public static void Init()
    {
        _localTimeScale = 1;
        time = 0;
    }

    public static void Update()
    {
        _deltaTime = Mathf.Min(Time.unscaledDeltaTime, Time.maximumDeltaTime) * timeScale;
        time += _deltaTime;
    }
}

public static class GameTime
{
    public static float time { get; private set; }
    private static float _localTimeScale = 1;

    public static float timeScale
    {
        get => Time.timeScale * _localTimeScale;
        set
        {
            if (Time.timeScale != 0)
                _localTimeScale = value / Time.timeScale;
        }
    }

    private static float _deltaTime;
    public static float deltaTime => _deltaTime;

    public static void Init()
    {
        _localTimeScale = 1;
        time = 0;
    }

    public static void Update()
    {
        _deltaTime = Mathf.Min(Time.unscaledDeltaTime, Time.maximumDeltaTime) * timeScale;
        time += _deltaTime;
    }
}