
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using UnityEngine.UI;


public class HealthUIDisplay : MonoBehaviour, IObserver<PlayerHealthChangedEvent>, IObserver<PlayerDeadEvent>
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private Text percentText;
    
    [Header("Visual Effects")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color warnColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private Image sliderFill;
    
    [Header("Animation")]
    [SerializeField] private float animationSpeed = 2f;
    [SerializeField] private bool useSmoothing = true;
    

    
    private float targetValue = 1f;
    private Coroutine animationCoroutine;

    
    private void InitializeUI()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }
        
        if (sliderFill == null && healthSlider != null)
        {
            sliderFill = healthSlider.fillRect.GetComponent<Image>();
        }
        
        UpdateUI(100, 100);
    }

    public void OnNotify(PlayerHealthChangedEvent data)
    {
        Debug.Log($"Health changed: {data.CurrentHealth}/{data.MaxHealth}");
        UpdateUI(data.CurrentHealth ?? 0, data.MaxHealth ?? 0);
        
    }
    
    private void UpdateUI(int currentHealth, int maxHealth)
    {
        // Calculate percentage
        float healthPercentage = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        
        // Update text
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
        
        if (percentText != null)
        {
            percentText.text = $"{Mathf.RoundToInt(healthPercentage * 100)}%";
        }
        
        // Update slider with animation
        if (healthSlider != null)
        {
            targetValue = healthPercentage;
            
            if (useSmoothing)
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(AnimateSlider());
            }
            else
            {
                healthSlider.value = targetValue;
                UpdateSliderColor(targetValue);
            }
        }
    }
    
    private System.Collections.IEnumerator AnimateSlider()
    {
        float startValue = healthSlider.value;
        float elapsed = 0f;
        
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * animationSpeed;
            float currentValue = Mathf.Lerp(startValue, targetValue, elapsed);
            
            healthSlider.value = currentValue;
            UpdateSliderColor(currentValue);
            
            yield return null;
        }
        
        healthSlider.value = targetValue;
        UpdateSliderColor(targetValue);
    }
    
    private void UpdateSliderColor(float healthPercentage)
    {
        if (sliderFill == null) return;
        
        Color targetColor;
        if (healthPercentage > 0.6f)
        {
            targetColor = healthyColor;
        }
        else if (healthPercentage > 0.3f)
        {
            targetColor = warnColor;
        }
        else
        {
            targetColor = dangerColor;
        }
        
        sliderFill.color = targetColor;
    }

    public void OnNotify(PlayerDeadEvent data)
    {
        // Optionally handle player death UI here
        Debug.Log("Player is dead.");
    }
}