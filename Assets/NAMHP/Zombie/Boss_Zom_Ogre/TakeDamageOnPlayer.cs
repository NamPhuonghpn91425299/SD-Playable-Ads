using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamageOnPlayer : MonoBehaviour
{
    [SerializeField] private BotConfigSO botConfigSo;
    private void OnEnable()
    {
        EventManager.Invoke(EventName.OnTakeDamagePlayer, botConfigSo.damage);
        EffectUI.Instance.Play();
    }
}
