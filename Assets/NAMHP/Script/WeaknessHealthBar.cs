using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaknessHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;
    [SerializeField] private GameObject healthBarContainer;
    private float maxHealth;
    private float currentHealth;
    
    public void Initialize(float maxHP)
    {
        maxHealth = maxHP;
        currentHealth = maxHP;
        UpdateHealthBar();
        healthBarContainer.SetActive(false); // Ẩn thanh máu lúc đầu
    }
    
    public void UpdateHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateHealthBar();
        ShowHealthBar();
    }
    
    private void UpdateHealthBar()
    {
        healthFillImage.fillAmount = currentHealth / maxHealth;
    }
    
    private void ShowHealthBar()
    {
        healthBarContainer.SetActive(true);
        CancelInvoke(nameof(HideHealthBar));
        Invoke(nameof(HideHealthBar), 3f); // Ẩn thanh máu sau 3 giây
    }
    
    private void HideHealthBar()
    {
        healthBarContainer.SetActive(false);
    }
}