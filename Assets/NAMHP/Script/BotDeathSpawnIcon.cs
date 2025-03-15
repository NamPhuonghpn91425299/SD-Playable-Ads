using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotDeathSpawnIcon : MonoBehaviour
{
    public GameObject iconDeathPrefab;
    public BotNetwork botNetwork;
    public Transform botTransform; // Transform của bot để lấy vị trí
    // [SerializeField] private Text _Dmgtxt;
    // [SerializeField] private Text _DmgtxtShadow;
    [SerializeField] private int minDamage;
    [SerializeField] private int maxDamage;
    public int damageRan;
    public int damageCritical;
    public int lastDamage;

    private void OnEnable()
    {
        botNetwork.OnBotDead += OnBotDead;
        botNetwork.OnLastTakeDamage += OnLastDamage;
        damageRan = UnityEngine.Random.Range(minDamage, maxDamage);
    }
    
    private void OnDisable()
    {
        botNetwork.OnBotDead -= OnBotDead;
        botNetwork.OnLastTakeDamage -= OnLastDamage;
    }
    
    void OnLastDamage(int damage)
    {
        lastDamage = (damageRan + damage);
    }

    private void OnBotDead()
    {
        Vector3 spawnPosition = botTransform.position;
        GameObject icon = ObjectPool.Instance.PopFromPool(iconDeathPrefab, instantiateIfNone: true);
        icon.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);

        var iconEffect = icon.GetComponent<IconEffect>();
        icon.SetActive(true);
        iconEffect.StartEffect(lastDamage, damageCritical);


    }
    
}