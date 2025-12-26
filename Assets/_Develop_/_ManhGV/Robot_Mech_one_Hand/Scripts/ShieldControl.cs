using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ShieldControl : MonoBehaviour,ITakeDamage
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool Immortal;

    public float valueShield; //0.05,.4
    
    [Header("References")]
    public MOH_Shield shield;
    public MeshRenderer shieldRenderer;
    public SphereCollider shieldCollider;
    private float timeFixRocket;
    
    public virtual void OnInit(float _timeFixRocket,Vector3 _position)
    {
        transform.position = _position + Vector3.up * 3.5f;
        gameObject.SetActive(true);
        timeFixRocket = _timeFixRocket;
        currentHealth = maxHealth;
        Immortal = false;
        valueShield = 0.05f;
        shieldRenderer.material.SetFloat("_NormalPush", valueShield);
        shieldCollider.enabled = true;
    }

    private void OnDisable()
    {
        valueShield = 0.05f;
        StopAllCoroutines();
    }

    private void Update()
    {
        if(Immortal)
            return;
        timeFixRocket -= Time.deltaTime;
        if (timeFixRocket <= 0)
        {
            Immortal = true;
            gameObject.SetActive(false);
            shield.DoneShield();
        }
    }

    public virtual void OnTakeDamage(DamageInfo damageInfo)
    {
        if(Immortal)
            return;
        currentHealth -= damageInfo.damage;
        valueShield = Mathf.Lerp(0.05f, .4f, 1f - (float)currentHealth / maxHealth);
        shieldRenderer.material.SetFloat("_NormalPush", valueShield);
        if (currentHealth <= 0)
        {
            Immortal = true;
            shieldCollider.enabled = false;
            currentHealth = 0;
            shield.ShieldExplosion();
            StartCoroutine(IEOnDeath());
        }
    }

    private IEnumerator IEOnDeath()
    {
        float elapsed = 0f;
        float duration = .2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            valueShield = Mathf.Lerp(0.4f, 8f, elapsed / duration);
            shieldRenderer.material.SetFloat("_NormalPush", valueShield);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    public Transform GetTransformThis()
    {
        throw new System.NotImplementedException();
    }

    public Transform GetTransformCenter()
    {
        throw new System.NotImplementedException();
    }
}