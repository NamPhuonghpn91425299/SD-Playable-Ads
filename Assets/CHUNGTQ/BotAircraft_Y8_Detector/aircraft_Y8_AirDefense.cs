using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class aircraft_Y8_AirDefense : MonoBehaviour
{
    public BotNetwork botNetwork;
    [SerializeField] private bool isDead;
    public List<FanDetector> fanDetectors;
    public Dictionary<string, FanDetector> hpBot = new Dictionary<string, FanDetector>();
    private int countDeadFan;
    private bool isMoveDone;

    private bool WeaknessDestroyed => fanDetectors.All(e => e.IsDead);
    private FanDetector CanDestroyedWeakness
    {
        get
        {
            var data = fanDetectors.Where(e => !e.IsDead);
            
            return data.OrderBy(e => e.RemainHealth).FirstOrDefault();
        }
    }
    
    private void Awake()
    {
        hpBot = fanDetectors.ToHashSet().ToDictionary(e => e.name, e => e);
        foreach (var fanDetector in fanDetectors)
        {
            fanDetector.Initialize(botNetwork.BotConfigSO.WeaknessHealth);
        }
        
    }
    void OnEnable()
    {
        countDeadFan = (int)(botNetwork.BotConfigSO.health / botNetwork.BotConfigSO.WeaknessHealth);
        isDead = false;
        botNetwork.OnWeaknessTakeDamage += OnWeaknessTakeDamage;
        botNetwork.OnHealthChanged += OnHeathChange;


    }
    
    private void OnHeathChange(float obj)
    {
        var persentFan = botNetwork.BotConfigSO.WeaknessHealth / botNetwork.BotConfigSO.health;
        var persentDestroy = obj / persentFan;
        if (persentDestroy <= countDeadFan - 1)
        {
            countDeadFan--;
            CanDestroyedWeakness?.TryHandleDamage(9999);
        }
    
    }
    
    private void OnWeaknessTakeDamage(string weaknessName, int damage)
    {
        if (hpBot.TryGetValue(weaknessName, out FanDetector fan))
        {
            
            if (!fan.IsDead && !fan.TryHandleDamage(damage))
            {
                countDeadFan--;
            }

            if (WeaknessDestroyed)
            {
                botNetwork.TakeDamage(new DamageInfo() { damage = 99999 });
            }

        }

    }
}
