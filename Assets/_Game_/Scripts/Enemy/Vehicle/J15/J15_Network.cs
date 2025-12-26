using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class J15_Network : VehicleNetwork
{
    [Header("Other This J15")] [SerializeField]
    private JetFighter _j15JetFighter;

    public int DamageRocket;
    public List<AudioSource> audioSourceEngine;

    public override void OnInit()
    {
        base.OnInit();
        _j15JetFighter.OnInit();
        foreach (AudioSource VARIABLE in audioSourceEngine)
        {
            VARIABLE.enabled = true;
        }

        audioSourceEngine[1].Play();
    }

    private void FixedUpdate()
    {
        if (audioSourceEngine.Count <= 0)
            return;
        if (GameController.Instance.CurrentGameState != GameConstants.GameState.InGame)
        {
            foreach (AudioSource VARIABLE in audioSourceEngine)
                VARIABLE.enabled = false;
            audioSourceEngine.Clear();
        }
    }

    public override void BotDead()
    {
        base.BotDead();
        _j15JetFighter.OnDead();
    }

    // public override void OnDespawn(float _delay)
    // {
    //     _j15JetFighter.OnDespawn(_delay-1f);
    //     base.OnDespawn(_delay);
    // }
}