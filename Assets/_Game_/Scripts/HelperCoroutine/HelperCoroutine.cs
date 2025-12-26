using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class để tối ưu coroutines và giảm GC allocations.
/// Cung cấp cache cho WaitForSeconds và các utility methods.
/// </summary>
public static class HelperCoroutine
{
    #region Cached Wait Instructions
    
    // Pre-cached common wait times để tránh allocations
    private static readonly WaitForEndOfFrame _waitEndOfFrame = new WaitForEndOfFrame();
    private static readonly WaitForFixedUpdate _waitFixedUpdate = new WaitForFixedUpdate();
    
    // Cache cho các giá trị WaitForSeconds phổ biến
    private static readonly WaitForSeconds _wait01 = new WaitForSeconds(0.1f);
    private static readonly WaitForSeconds _wait02 = new WaitForSeconds(0.2f);
    private static readonly WaitForSeconds _wait025 = new WaitForSeconds(0.25f);
    private static readonly WaitForSeconds _wait03 = new WaitForSeconds(0.3f);
    private static readonly WaitForSeconds _wait05 = new WaitForSeconds(0.5f);
    private static readonly WaitForSeconds _wait1 = new WaitForSeconds(1f);
    private static readonly WaitForSeconds _wait2 = new WaitForSeconds(2f);
    private static readonly WaitForSeconds _wait3 = new WaitForSeconds(3f);
    private static readonly WaitForSeconds _wait5 = new WaitForSeconds(5f);
    
    // Dictionary cho custom wait times
    private static Dictionary<float, WaitForSeconds> _customWaitCache = new Dictionary<float, WaitForSeconds>();
    private const int MAX_CACHE_SIZE = 100; // Giới hạn cache size để tránh memory bloat
    
    #endregion
    
    #region Public Properties
    
    public static WaitForEndOfFrame WaitEndOfFrame => _waitEndOfFrame;
    public static WaitForFixedUpdate WaitFixedUpdate => _waitFixedUpdate;
    
    #endregion
    
    #region Wait Methods
    
    /// <summary>
    /// Lấy cached WaitForSeconds. Tự động cache các giá trị mới.
    /// </summary>
    public static WaitForSeconds GetWait(float seconds)
    {
        // Fast path cho các giá trị phổ biến
        switch (seconds)
        {
            case 0.1f: return _wait01;
            case 0.2f: return _wait02;
            case 0.25f: return _wait025;
            case 0.3f: return _wait03;
            case 0.5f: return _wait05;
            case 1f: return _wait1;
            case 2f: return _wait2;
            case 3f: return _wait3;
            case 5f: return _wait5;
        }
        
        // Check custom cache
        if (!_customWaitCache.TryGetValue(seconds, out var wait))
        {
            // Clear cache nếu quá lớn (tránh memory leak)
            if (_customWaitCache.Count >= MAX_CACHE_SIZE)
            {
                Debug.LogWarning($"[HelperCoroutine] Cache cleared at {MAX_CACHE_SIZE} entries");
                _customWaitCache.Clear();
            }
            
            wait = new WaitForSeconds(seconds);
            _customWaitCache[seconds] = wait;
        }
        
        return wait;
    }
    
    /// <summary>
    /// Alias ngắn gọn cho GetWait
    /// </summary>
    public static WaitForSeconds Wait(float seconds) => GetWait(seconds);
    
    #endregion
    
    #region Delay Methods
    
    /// <summary>
    /// Delay một action sau số giây nhất định
    /// </summary>
    public static Coroutine DelaySeconds(this MonoBehaviour mono, float seconds, Action callback)
    {
        if (mono == null || !mono.gameObject.activeInHierarchy)
        {
            Debug.LogError("[HelperCoroutine] MonoBehaviour is null or inactive!");
            return null;
        }
        
        if (callback == null)
        {
            Debug.LogError("[HelperCoroutine] Callback is null!");
            return null;
        }
        
        return mono.StartCoroutine(DelaySecondsCoroutine(seconds, callback));
    }
    
    /// <summary>
    /// Delay một action sau số frame nhất định
    /// </summary>
    public static Coroutine DelayFrames(this MonoBehaviour mono, int frames, Action callback)
    {
        if (mono == null || !mono.gameObject.activeInHierarchy)
        {
            Debug.LogError("[HelperCoroutine] MonoBehaviour is null or inactive!");
            return null;
        }
        
        if (callback == null)
        {
            Debug.LogError("[HelperCoroutine] Callback is null!");
            return null;
        }
        
        if (frames <= 0)
        {
            callback.Invoke();
            return null;
        }
        
        return mono.StartCoroutine(DelayFramesCoroutine(frames, callback));
    }
    
    /// <summary>
    /// Delay tới end of frame
    /// </summary>
    public static Coroutine DelayToEndOfFrame(this MonoBehaviour mono, Action callback)
    {
        if (mono == null || !mono.gameObject.activeInHierarchy)
        {
            Debug.LogError("[HelperCoroutine] MonoBehaviour is null or inactive!");
            return null;
        }
        
        if (callback == null)
        {
            Debug.LogError("[HelperCoroutine] Callback is null!");
            return null;
        }
        
        return mono.StartCoroutine(DelayToEndOfFrameCoroutine(callback));
    }
    
    #endregion
    
    #region Repeat Methods
    
    /// <summary>
    /// Lặp lại một action mỗi interval giây
    /// </summary>
    public static Coroutine RepeatSeconds(this MonoBehaviour mono, float interval, Action callback, float duration = -1)
    {
        if (mono == null || !mono.gameObject.activeInHierarchy)
        {
            Debug.LogError("[HelperCoroutine] MonoBehaviour is null or inactive!");
            return null;
        }
        
        if (callback == null)
        {
            Debug.LogError("[HelperCoroutine] Callback is null!");
            return null;
        }
        
        return mono.StartCoroutine(RepeatSecondsCoroutine(interval, callback, duration));
    }
    
    /// <summary>
    /// Lặp lại một action với condition
    /// </summary>
    public static Coroutine RepeatUntil(this MonoBehaviour mono, float interval, Action callback, Func<bool> stopCondition)
    {
        if (mono == null || !mono.gameObject.activeInHierarchy)
        {
            Debug.LogError("[HelperCoroutine] MonoBehaviour is null or inactive!");
            return null;
        }
        
        if (callback == null || stopCondition == null)
        {
            Debug.LogError("[HelperCoroutine] Callback or condition is null!");
            return null;
        }
        
        return mono.StartCoroutine(RepeatUntilCoroutine(interval, callback, stopCondition));
    }
    
    #endregion
    
    #region Private Coroutines
    
    private static IEnumerator DelaySecondsCoroutine(float seconds, Action callback)
    {
        yield return GetWait(seconds);
        callback?.Invoke();
    }
    
    private static IEnumerator DelayFramesCoroutine(int frames, Action callback)
    {
        for (int i = 0; i < frames; i++)
            yield return null;
        callback?.Invoke();
    }
    
    private static IEnumerator DelayToEndOfFrameCoroutine(Action callback)
    {
        yield return _waitEndOfFrame;
        callback?.Invoke();
    }
    
    private static IEnumerator RepeatSecondsCoroutine(float interval, Action callback, float duration)
    {
        float elapsed = 0f;
        var wait = GetWait(interval);
        
        while (duration < 0 || elapsed < duration)
        {
            callback?.Invoke();
            yield return wait;
            elapsed += interval;
        }
    }
    
    private static IEnumerator RepeatUntilCoroutine(float interval, Action callback, Func<bool> stopCondition)
    {
        var wait = GetWait(interval);
        
        while (!stopCondition())
        {
            callback?.Invoke();
            yield return wait;
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Clear custom cache (gọi khi cần free memory)
    /// </summary>
    public static void ClearCache()
    {
        _customWaitCache.Clear();
        Debug.Log("[HelperCoroutine] Cache cleared manually");
    }
    
    /// <summary>
    /// Lấy thông tin về cache hiện tại
    /// </summary>
    public static string GetCacheInfo()
    {
        return $"[HelperCoroutine] Custom cache size: {_customWaitCache.Count}/{MAX_CACHE_SIZE}";
    }
    
    #endregion
}

/// <summary>
/// Extension class để dễ dàng stop coroutines
/// </summary>
public static class CoroutineExtensions
{
    /// <summary>
    /// Stop coroutine an toàn (check null)
    /// </summary>
    public static void StopCoroutineSafe(this MonoBehaviour mono, Coroutine coroutine)
    {
        if (mono != null && coroutine != null)
            mono.StopCoroutine(coroutine);
    }
    
    /// <summary>
    /// Stop all coroutines an toàn
    /// </summary>
    public static void StopAllCoroutinesSafe(this MonoBehaviour mono)
    {
        if (mono != null)
            mono.StopAllCoroutines();
    }
}
