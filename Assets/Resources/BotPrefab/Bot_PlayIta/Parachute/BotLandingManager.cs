using UnityEngine;

public class BotLandingManager : MonoBehaviour
{
    public static BotLandingManager Instance { get; private set; }
    public int TotalLandCount { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void IncrementLandCount()
    {
        TotalLandCount++;
        EventManager.Invoke(EventName.OnCountBotLanding, TotalLandCount);
    }
}
