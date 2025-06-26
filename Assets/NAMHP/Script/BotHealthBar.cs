using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NUtiliti;
using Image = UnityEngine.UI.Image;
public class BotHealthBar : MonoBehaviour
{
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private BotNetwork botNetwork;
    // Start is called before the first frame update
    void Start()
    {
        healthBar.SetActive(true);
        botNetwork.OnHealthChanged += SetHealth;
    }

    private void OnDisable()
    {
        botNetwork.OnHealthChanged -= SetHealth;
    }

    // Update is called once per frame
    void Update()
    {
        AlignCamera(healthBar.transform, botNetwork.mainCameraTranform);
    }

    public void SetHealth(float health)
    {
        if (health <= 0) healthBar.SetActive(false);
        healthBarFill.fillAmount = health;
    }
}
