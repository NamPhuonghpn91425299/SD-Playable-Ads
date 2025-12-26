using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// =================
// DAMAGE BUTTON (For Testing)
// =================
public class DamageButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private Button damageButton;
    
    private PlayerController playerController;
    
    void Start()
    {
        // Find player controller
        playerController = FindObjectOfType<PlayerController>();
        
        // Setup button
        if (damageButton == null)
        {
            damageButton = GetComponent<Button>();
        }
        
        if (damageButton != null)
        {
            damageButton.onClick.AddListener(OnDamageButtonClick);
        }
        
        // Update button text
        if (damageButton != null)
        {
            Text buttonText = damageButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = $"Damage {damageAmount}";
            }
        }
    }
    
    public void OnDamageButtonClick()
    {
        
    }
}