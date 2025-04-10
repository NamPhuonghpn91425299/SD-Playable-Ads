using System;
using UnityEngine;
using UnityEngine.UI;
using static NUtiliti;

public class BotIndicatorController : MonoBehaviour
{
    public BotNetwork botNetwork; // Tham chiếu đến BotNetwork
    public Image botIndicator; // Transform của chỉ báo bot
    public bool isShow = true; // Biến kiểm tra xem chỉ báo có được hiển thị hay không
    private Transform mainCameraTranform; // Transform của camera chính
    private bool isRegistered = false; // Biến kiểm tra xem bot đã được đăng ký hay chưa
    private bool isOnBot = false; // Biến kiểm tra trạng thái chỉ báo trên bot

    private void OnEnable()
    {
        botNetwork.OnBotDead += () =>
        {
            // Khi bot chết, ẩn chỉ báo và hủy đăng ký với BotIndicatorManager
            if (botIndicator != null)
            {
                botIndicator.enabled = false; // Ẩn chỉ báo
            }
            if (BotIndicatorManager.instance != null && isRegistered)
            {
                BotIndicatorManager.instance.UnregisterBot(gameObject);
                isRegistered = false; // Đánh dấu bot đã bị hủy đăng ký
            }
        };
        // Lấy transform của camera chính khi object được kích hoạt
        mainCameraTranform = Camera.main.transform;

        // Đăng ký bot với BotIndicatorManager khi object được kích hoạt
        if (BotIndicatorManager.instance != null && !isRegistered)
        {
            BotIndicatorManager.instance.RegisterBot(gameObject);
            isRegistered = true; // Đánh dấu bot đã được đăng ký
        }
    }

    private void OnDisable()
    {
        botNetwork.OnBotDead += () =>
        {
            // Khi bot chết, ẩn chỉ báo và hủy đăng ký với BotIndicatorManager
            if (botIndicator != null)
            {
                botIndicator.enabled = false; // Ẩn chỉ báo
            }
        };
        // Hủy đăng ký bot với BotIndicatorManager khi object bị vô hiệu hóa
        if (BotIndicatorManager.instance != null && isRegistered)
        {
            BotIndicatorManager.instance.UnregisterBot(gameObject);
            isRegistered = false; // Đánh dấu bot đã bị hủy đăng ký
        }
    }

    private void Update()
    {
        // Chỉ thực hiện kiểm tra nếu BotIndicatorManager không null và chỉ báo được hiển thị
        if (BotIndicatorManager.instance != null && isShow)
        {
            CheckIndicatorsOnBot(); // Kiểm tra trạng thái chỉ báo trên bot
            AlignCamera(botIndicator.transform, mainCameraTranform); // Căn chỉnh camera với chỉ báo
        }
    }

    private void CheckIndicatorsOnBot()
    {
        // Nếu bot nằm trong màn hình và chỉ báo chưa được hiển thị
        if (!BotIndicatorManager.instance.isOffScreen && !isOnBot)
        {
            botIndicator.enabled = true;
            isOnBot = true; // Đánh dấu trạng thái chỉ báo trên bot
        }
        // Nếu bot nằm ngoài màn hình và chỉ báo đang được hiển thị
        else if (BotIndicatorManager.instance.isOffScreen && isOnBot)
        {
            botIndicator.enabled = false; // Ẩn chỉ báo
            isOnBot = false; // Đánh dấu trạng thái chỉ báo không còn trên bot
        }
    }
}