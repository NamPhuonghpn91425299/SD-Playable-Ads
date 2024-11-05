using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bot : MonoBehaviour,IDamageHit
{
    [SerializeField] private IState currentState;
    [SerializeField] private Transform target;
    [SerializeField] private float currentHealth;
    [SerializeField] private BotConfigSO config;
    [SerializeField] public Animator animator;
    [SerializeField] private GameObject acttackEffect;
    [SerializeField] public bool isAttacking;
    [SerializeField] private float acttackTimer;
    
    [SerializeField] private float maxActtackTime = 3f;
    public float MaxHealth => config.maxHealth;
    public float MoveSpeed => config.moveSpeed;
    public float RotationSpeed => config.rotationSpeed;
    public float AttackRange => config.attackRange;
    public float AttackDamage => config.attackDamage;
    public float AttackSpeed => config.attackSpeed;
    [SerializeField]private HousePoint housePoint;
    [SerializeField]private int currentPointIndex;

    [Header("Health Bar")]
    [SerializeField] private MeshRenderer healthBar;    // Reference đến mesh renderer của thanh máu
    [SerializeField] private Material healthBarMaterial; // Material cho thanh máu
 
    public void Initialize(BotConfigSO botConfigSo, HousePoint housePoint)
    {
        config = botConfigSo;
        this.housePoint = housePoint;
        currentHealth = config.maxHealth;
        ChangeState(new MoveState(this));
    }
    // Start is called before the first frame update

    private void OnEnable()
    {
        currentHealth = config.maxHealth;
        target = WeaponController.instance.transform.root;
        currentPointIndex = 0;
    }

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
            currentState?.Exit();
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

    public bool isLastPoint()
    {
        var lastPoint = ++currentPointIndex < housePoint.HousePoints.Count;        
        return lastPoint;
    }

    public Vector3 GetPoint()
    {
        Debug.DrawLine(transform.position, target.position, Color.magenta, 5) ;
        return housePoint.HousePoints[currentPointIndex].position;
    }
    
    public void MoveToNextPoint()
    {
        // Lấy điểm tiếp theo trong danh sách housePoints
        if (currentPointIndex < housePoint.HousePoints.Count)
        {
            Transform targetPoint = housePoint.HousePoints[currentPointIndex];
            MoveToTarget(targetPoint.position);
            currentPointIndex++;
        }
        else
        {

        }
    }
    public void MoveToTarget(Vector3 targetPosition)
    {
        if (target != null)
        {
            var dir = (targetPosition - transform.position).normalized;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), RotationSpeed * Time.deltaTime);
            //transform.LookAt(targetPosition);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, MoveSpeed * Time.deltaTime);
        }
    }

    public void LockAtTager()
    {
        // Tính toán hướng để quay mặt về phía target
        Vector3 directionToTarget = (target.position - acttackEffect.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, RotationSpeed * Time.deltaTime);
        Debug.DrawLine(acttackEffect.transform.position, target.position, Color.red);
    }
    public void ActtackToTarget()
    {
        if (isAttacking)
        {
            acttackEffect.SetActive(true);
        }
        else
        {
            acttackEffect.SetActive(false);
        }
    }

    public void OnHit(int damage)
    {
        if (currentHealth == 0)
        {
            return;
        }
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        SetHealthBar(currentHealth);
        if (currentHealth <= 0)
            ChangeState(new DeathState(this));
        {
        }
    }
    public void OnSpawn()
    {
        // Reset máu về max
        currentHealth = config.maxHealth;
        // Update lại thanh máu
        SetHealthBar(currentHealth);
    }
    private void SetHealthBar(float currentHealth)
    {
        float healthBarValue = (currentHealth / config.maxHealth);
        if (healthBar != null && healthBar.material != null)
        {
            healthBar.material.SetFloat("_Fill", healthBarValue);
        }
    }
}
