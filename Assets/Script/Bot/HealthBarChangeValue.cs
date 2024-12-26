using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarChangeValue : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private MeshRenderer healthBar;    // Reference đến mesh renderer của thanh máu
    [SerializeField] private Material healthBarMaterial; // Material cho thanh máu
    [SerializeField]private float currentHealth;
    [SerializeField]private BotConfigSO1 botConfig;

    void Start()
    {
        // Khởi tạo máu
        currentHealth = botConfig.maxHealth;
        // Set up thanh máu
        if (healthBar != null)
        {
            healthBar.material = new Material(healthBarMaterial);
            UpdateHealthBar();
        }
    }
    // Cập nhật thanh máu
    public void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            float healthPercent = currentHealth / botConfig.maxHealth;
            healthBar.material.SetFloat("_Fill", healthPercent);

            // Đổi màu theo lượng máu (tùy chọn)
            if (healthPercent > 0.5f)
            {
                healthBar.material.SetColor("_Color", Color.green);
            }
            else if (healthPercent > 0.2f)
            {
                healthBar.material.SetColor("_Color", Color.yellow);
            }
            else
            {
                healthBar.material.SetColor("_Color", Color.red);
            }
        }
    }


}
