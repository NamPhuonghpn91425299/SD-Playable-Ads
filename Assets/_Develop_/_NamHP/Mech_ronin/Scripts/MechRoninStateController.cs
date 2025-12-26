using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;
public class MechRoninStateController : StateControllerBase
{
    [Header("State Components")]
    [SerializeField] private MechRoninStateBase m_currentState;
    [SerializeField] private MechRonin_Move_State m_moveState;
    [SerializeField] private MechRonin_Landing_State m_landingState;
    [SerializeField] private MechRonin_Attack_State m_attackState;
    [SerializeField] private MechRonin_Special_State m_specialState;
    [SerializeField] private MechRonin_Dead_State m_deadState;
    [SerializeField] private float m_lowHealthThreshold = 0.5f; // Ngưỡng máu thấp để kích hoạt Special State
    [SerializeField] private float m_ArmorThreshold = 10f; // Ngưỡng giáp thấp để kích hoạt Special State
    private bool m_isChangeState = false;
    private void OnEnable()
    {
        m_isChangeState = false;
        botContext.botNetwork.ACOnHealChange += OnCulateDame;
    }


    private void OnDisable()
    {
        botContext.botNetwork.ACOnHealChange -= OnCulateDame;

    }

    private void OnCulateDame(int healAmount)
    {

        if (healAmount <= botContext.botNetwork.MaxHealth * m_lowHealthThreshold)
        {
            //Debug.Log(", Current Health: " + healAmount + ", Threshold: " + (botContext.botNetwork.MaxHealth * m_lowHealthThreshold));
            if (m_currentState != m_specialState && m_currentState != m_deadState && !m_isChangeState)
            {
                m_attackState.m_isLowHealth = true;
                m_isChangeState = true;
                int finalArmor = (int)(botContext.botNetwork.Armor * m_ArmorThreshold);
                botContext.botNetwork.SetArmor(finalArmor);
                Debug.Log("Switch to Special State due to low health. New Armor: "
                + finalArmor + ", Current Damage: " + botContext.botNetwork.Armor + ", Threshold: " + (m_ArmorThreshold));
                //ChangeState(EnemyState.Special);
            }
        }
        // Xử lý logic khi bot được hồi máu nếu cần
    }



#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        m_moveState = GetComponent<MechRonin_Move_State>();
        m_landingState = GetComponent<MechRonin_Landing_State>();
        m_attackState = GetComponent<MechRonin_Attack_State>();
        m_specialState = GetComponent<MechRonin_Special_State>();
        m_deadState = GetComponent<MechRonin_Dead_State>();
    }
#endif

    void Awake()
    {

        m_moveState.Initialize(EnemyState.Move, botContext);
        m_landingState.Initialize(EnemyState.Falling, botContext);
        m_attackState.Initialize(EnemyState.Attack, botContext);
        m_specialState.Initialize(EnemyState.Special, botContext);
        m_deadState.Initialize(EnemyState.Dead, botContext);

        stateController.Add(EnemyState.Move, m_moveState);
        stateController.Add(EnemyState.Falling, m_landingState);
        stateController.Add(EnemyState.Attack, m_attackState);
        stateController.Add(EnemyState.Special, m_specialState);
        stateController.Add(EnemyState.Dead, m_deadState);
    }

    public override void DeadExplosion()
    {
        base.DeadExplosion();
        if (!canDead)
            return;
        canDead = false;
        ChangeState(EnemyState.Dead);
    }

    protected override void OnDead(bool isDead)
    {
        botContext.botNetwork.ACOnTakeDamage -= OnTakeDame;
        botContext.botNetwork.ACBotDead -= OnDead;
        if (!canDead)
            return;
        canDead = false;
        if (transform.parent != null)
        {
            transform.parent = null;
            ChangeState(EnemyState.Dead);
        }
        else
            ChangeState(EnemyState.Dead);
    }

    [ContextMenu("Test Dead")]
    private void TestDead()
    {
        ChangeState(EnemyState.Dead);
    }

}
public static class AnimationParameters
{
    public static readonly int Run = Animator.StringToHash("Run");
    public static readonly int Rolling = Animator.StringToHash("Rolling");
    public static readonly int Fly = Animator.StringToHash("Fly");
    public static readonly int FlyUpReady = Animator.StringToHash("FlyUpReady");
    public static readonly int FlyUp = Animator.StringToHash("FlyUp");
    public static readonly int Landing = Animator.StringToHash("Landing");
    public static readonly int AttackGun = Animator.StringToHash("AttackGun");
    public static readonly int EndRun = Animator.StringToHash("EndRun");
    public static readonly int SwordSwitch = Animator.StringToHash("SwordSwitch");
    public static readonly int DashAttack = Animator.StringToHash("DashAttack");
    public static readonly int Ultimate = Animator.StringToHash("Ultimate");
    public static readonly int Dead = Animator.StringToHash("Dead");
    public static readonly int Dash = Animator.StringToHash("Dash");
    public static readonly int DashAttack2 = Animator.StringToHash("DashAttack2");
    public static readonly int LandingIdle = Animator.StringToHash("LandingIdle");
    public static readonly int AttackPhase3 = Animator.StringToHash("AttackPhase3");
}