using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class BotUIManager : MonoBehaviour
{
    [Tooltip("Danh sách các thanh máu (Image) dùng làm UI cho bot")]
    public List<Image> botHealthBars;
    [SerializeField] private Dictionary<BotNetwork, Image> botToUIMap = new Dictionary<BotNetwork, Image>();

    [SerializeField] private Gradient HPState;
    public static BotUIManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public void AssignBotToUI(BotNetwork bot)
    {
        // Tìm thanh máu chưa được gán
        foreach (var healthBar in botHealthBars)
        {
            if (!botToUIMap.ContainsValue(healthBar))
            {
                botToUIMap.Add(bot, healthBar);
                //Debug.Log($"Gán {bot.name} vào thanh máu {healthBar.name}");
                // Lắng nghe sự kiện thay đổi máu của bot
                bot.OnHealthChanged += (fillAmount) =>
                {
                    //Debug.Log($"Cập nhật thanh máu {healthBar.name} với giá trị: {fillAmount}");
                    healthBar.fillAmount = fillAmount;
                    healthBar.color = HPState.Evaluate(healthBar.fillAmount);
                };

                return;
            }
        }

        Debug.LogWarning("Không còn thanh máu trống để gán cho bot!");
    }

    public void RemoveBotUI(BotNetwork bot)
    {
        if (botToUIMap.TryGetValue(bot, out var healthBar))
        {
            botToUIMap.Remove(bot);
            healthBar.fillAmount = 0; // Reset UI nếu cần
        }
    }
}

