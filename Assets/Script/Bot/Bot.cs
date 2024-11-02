using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour,IDamageHit
{
    [SerializeField] private IState currentState;
    [SerializeField] private Transform target;
    [SerializeField] private float currentHealth;
    [SerializeField] private BotConfigSO config;
    public float MaxHealth => config.maxHealth;
    public float MoveSpeed => config.moveSpeed;
    public float AttackRange => config.attackRange;
    public float AttackDamage => config.attackDamage;
    public float AttackSpeed => config.attackSpeed;

    public void Initialize(BotConfigSO botConfigSo)
    {
        config = botConfigSo;
        currentHealth = config.maxHealth;
        ChangeState(new IdleState(this));
    }
    // Start is called before the first frame update
    

    // Update is called once per frame
    void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
    }

    public void ChangeState(IState newState)
    {
        if (newState != null)
        {
            currentState.Exit();
        }
        currentState = newState;
        currentState.Enter();
    }

    public bool IsTargetInRange()
    {
        if (target == null)
        {
            return false;
        }
        return Vector3.Distance(transform.position, target.position) <= config.attackRange;
    }

    public void MoveToTarget()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, config.moveSpeed * Time.deltaTime);
        }
    }


    public void OnHit(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            ChangeState(new DeathState(this));
        }
    }


}
