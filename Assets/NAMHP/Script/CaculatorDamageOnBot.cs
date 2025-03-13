using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static BotNetwork;
public class CaculatorDamageOnBot : MonoBehaviour
{
    [SerializeField] private Text _Dmgtxt;
    [SerializeField] private Text _DmgtxtShadow;
    [SerializeField] private int minDamage;
    [SerializeField] private int maxDamage;
    [SerializeField] private int damageRan;
    void Start()
    {
        //BotNetwork.OnReceiverDamage += ReceiverDamage;
    }

    private void OnEnable()
    {
        damageRan = UnityEngine.Random.Range(minDamage, maxDamage);
        BotNetwork.OnReceiverDamage += ReceiverDamage;
    }

    private void OnDisable()
    {
        BotNetwork.OnReceiverDamage -= ReceiverDamage;
    }
    private void ReceiverDamage(int damage)
    {
        _Dmgtxt.text = (damageRan + damage).ToString();
        _DmgtxtShadow.text = (damageRan + damage).ToString();
    }


    // Update is called once per frame
    void Update()
    {

    }
}

