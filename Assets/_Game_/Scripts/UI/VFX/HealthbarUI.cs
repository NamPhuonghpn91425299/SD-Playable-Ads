using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarUI : VFXBase
{
    [SerializeField] private Slider _healthbarSlider;
    [SerializeField] private Text HealthText;
    [SerializeField] private Color _fullHealthColor = Color.green;
    [SerializeField] private Color _lowHealthColor = Color.red;
    [SerializeField] private Color _mediumHealthColor = Color.yellow;
    public override void Play<T>(T parameter)
    {
        if (parameter is ValueTuple<int, int> tuple)
        {
            UpdateHealthbar(tuple.Item1, tuple.Item2);
            HealthText.text = tuple.Item2.ToString() + "/" + tuple.Item1.ToString();
        }
    }

    private void UpdateHealthbar(int maxHealth, int currentHealth)
    {
        if (maxHealth <= 0)
        {
            _healthbarSlider.value = 0f; // Avoid division by zero
            return;
        }

        float healthPercentage = (float)currentHealth / maxHealth;
        _healthbarSlider.value = healthPercentage;

        // Update health bar color based on health percentage
        if (healthPercentage == 1f)
        {
            _healthbarSlider.fillRect.GetComponent<Image>().color = _fullHealthColor;
        }
        else if (healthPercentage > 0.5f)
        {
            _healthbarSlider.fillRect.GetComponent<Image>().color = _mediumHealthColor;
        }
        else
        {
            _healthbarSlider.fillRect.GetComponent<Image>().color = _lowHealthColor;
        }
    }
}
