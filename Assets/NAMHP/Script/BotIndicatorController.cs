using System;
using UnityEngine;
using static NUtiliti;
public class BotIndicatorController : MonoBehaviour
{
    private bool isRegistered = false;
    private bool isOnBot = false;
    public Transform botIndicator;
    public bool isShow = true;
    private Transform mainCameraTranform;
    private void OnEnable()
    {
        mainCameraTranform = Camera.main.transform;
        // Register khi object được enable
        if (BotIndicatorManager.instance != null && !isRegistered)
        {
            BotIndicatorManager.instance.RegisterBot(gameObject);
            isRegistered = true;
        }
    }

    private void OnDisable()
    {
        // Unregister khi object bị disable
        if (BotIndicatorManager.instance != null && isRegistered)
        {
            BotIndicatorManager.instance.UnregisterBot(gameObject);
            isRegistered = false;
        }
    }

    private void Update()
    {
        // Chỉ kiểm tra nếu BotIndicatorManager không null

        if (BotIndicatorManager.instance != null && isShow)
        {
            CheckIndicatorsOnBot();
            AlignCamera(botIndicator.transform, mainCameraTranform);
        }
    }

    private void CheckIndicatorsOnBot()
    {
        if (!BotIndicatorManager.instance.isOffScreen && !isOnBot)
        {
            botIndicator.gameObject.SetActive(true);
            isOnBot = true;
        }
        else if (BotIndicatorManager.instance.isOffScreen && isOnBot)
        {
            botIndicator.gameObject.SetActive(false);
            isOnBot = false;
        }
    }
}