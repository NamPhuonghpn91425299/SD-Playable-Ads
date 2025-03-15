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
    public int damageRan;
    public int damageTotal;
    public int damageCritical;
    [SerializeField] private Color[] color;

    private void Awake()
    {
        damageRan = UnityEngine.Random.Range(minDamage, maxDamage);
    }

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
        damageTotal = (damageRan + damage);
        if (damageTotal > damageCritical)
        {
            _Dmgtxt.color = color[0];
            _Dmgtxt.text = damageTotal.ToString();
            _DmgtxtShadow.text = damageTotal.ToString();
        }
        else
        {
            _Dmgtxt.color = color[1];
            _Dmgtxt.text = damageTotal.ToString();
            _DmgtxtShadow.text = damageTotal.ToString();
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}

