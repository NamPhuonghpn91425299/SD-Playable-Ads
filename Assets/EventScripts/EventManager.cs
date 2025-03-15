using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    private static readonly Dictionary<EventName, List<Delegate>> Channels = new Dictionary<EventName, List<Delegate>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ManualReloadDomain()
    {
        Channels.Clear();
    }

    public static void AddListener<M>(EventName key, UnityAction<M> response)
    {
        if (Channels.TryGetValue(key, out var list))
        {
            list.Add(response);
        }
        else
        {
            Channels.Add(key, new List<Delegate> { response });
        }
    }

    public static void RemoveListener<M>(EventName key, UnityAction<M> response)
    {
        if (Channels.TryGetValue(key, out var list))
        {
            if (!list.Remove(response))
            {
                Debug.Log($"Fail to remove event listener {response.Method} in {response.Target}");
            }
        }
        else
        {
            Debug.Log($"Fail to find channels with key {key}");
        }
    }

    public static void Invoke<M>(EventName key, M data)
    {
        if (Channels.TryGetValue(key, out var list))
        {
            for (var idx = 0; idx < list.Count; idx++)
            {
                var action = list[idx];
                try
                {
                    if (action is UnityAction<M> unityAction)
                    {
                        unityAction.Invoke(data);
                    }
                }
                catch (Exception e)
                {
                    var content = $"Error when invoke method {action.Method} from {action.Target} \n" + e;
                    if (action.Target is MonoBehaviour monoBehaviour)
                        Debug.LogError(content, monoBehaviour.gameObject);
                    else
                        Debug.LogError(content);
                }
            }
        }
        else
        {
            Debug.LogWarning($"Can't find listener on channel {key}");
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Horus Tool/Log EventManager")]
    public static void Log()
    {
        foreach (var channel in Channels)
        {
            Debug.Log($"<color=yellow>Channel {channel.Key}:</color>");
            Log(channel.Key);
        }
    }

    public static void Log(EventName key)
    {
        if (Channels.TryGetValue(key, out var list))
        {
            foreach (var item in list)
                Debug.Log($"<color=green>{item.Method}</color> in <color=cyan>{item.Target}</color>",
                          (UnityEngine.Object)item.Target);
        }
        else
        {
            Debug.Log($"Can't find channel <color=yellow>{key}</color>");
        }
    }
#endif

}

public enum EventName
{
    OnShowLunaEndGame,
    OnCheckTurnPlay, // sự kiện show EndCard
    UpdateBulletCount,// đếm số lượng đạn và in ra ở UI
    OnCountBotLanding,//đếm số lượng bot landing
    OnBotKillCount,//đếm số lượng bot Chết
    OnCheckBotTakeDamage,//CheckBot có dính Damage hay không 
    OnCheckShakeCam,
    OnClearReward,
    OnUpgradeFireRate,
    OnChangeFireRate,
    OnChangeMachineGun,
    OnTakeDamagePlayer,
    OnReloading,
    OnPlayerDead,
    OnTimeOut,
    OnTakeDameWeakPoint,
    OnStaticBotDead,
    OnChangeWeapon,
    OnGameWon,
    OnGameLost,

    // Thêm các sự kiện khác ở đây
}
