using System;
using System.Collections;
using System.Collections.Concurrent;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using UnityEngine.Serialization;
using static GameConstants;


public class Rocket : GameUnit<ProjectileEnemy>
{
    [SerializeField] private float         speed    = 10f;
    [SerializeField] private float         m_strength = 0.025f;
    private                  float         speedDefault;
    [SerializeField] private float         rotationSpeed     = 3f;
    [SerializeField] private float         distanceToDespawn = 10f;
    [SerializeField] private GameObject    explosionEffect;
    [SerializeField] private GameObject    model;
    private                  int           damage;
    private                  bool          canDespawn;
    private                  Transform     PlayerTransform;
    [SerializeField] private AudioSource[] audioSource;
    private void Awake()
    {
        speedDefault = speed;
        PlayerTransform = PlayerInstant.Instance.transform;
    }

    public void Init(int damage, float speedNhanThem)
    {
        canDespawn = true;
        speed *= speedNhanThem;
        this.damage = damage;
    }
    public void Init(int damage)
    {
        canDespawn = true;
        this.damage = damage;
    }

    void OnEnable()
    {
        // Add any initialization logic here
        explosionEffect.SetActive(false);
        model.SetActive(true);
        if (GameController.Instance.CurrentGameState != GameState.InGame)
            foreach (AudioSource VARIABLE in audioSource)
                VARIABLE.enabled = false;
        else
            foreach (AudioSource VARIABLE in audioSource)
                VARIABLE.enabled = true;
    }


    // Update is called once per frame
    void Update()
    {
        // Check if the rocket has exceeded its despawn distance
        if (Vector3.Distance(TF.position, PlayerTransform.position) < distanceToDespawn)
            OnDespawn();
        // Follow the target position
        FollowTarget(PlayerTransform.position);
    }

    void FollowTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - TF.position).normalized;

        // Move rocket
        TF.position =Vector3.MoveTowards(TF.position,PlayerTransform.position,  speed * Time.deltaTime);

        // Smooth rotation với độ trễ
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            TF.rotation = Quaternion.Slerp(TF.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void OnDespawn()
    {
        if(!canDespawn)
            return;
        canDespawn = false;
        speed = speedDefault;
        if (GameController.Instance.CurrentGameState == GameState.InGame)
        {
            EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: damage, state: "OnlyDamage"));
            EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData{duration = .3f,strength = m_strength,vibrato = 15,randomness = 45}));
        }
            
        
        // EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: damage, state: "OnlyDamage"));
        SimplePool<GameConstants.ProjectileEnemy>.Spawn<ExplosionPanzerwerfer>(ProjectileEnemy.Explsion, this.transform.position, Quaternion.identity);
        SimplePool<ProjectileEnemy>.Despawn(this);

    }
}
