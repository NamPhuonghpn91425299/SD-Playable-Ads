using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtilities;
using static GameConstants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtilities;
using static GameConstants;
using DG.Tweening;
using System;

/// <summary>
/// Core Manager cho Mech Ronin - chỉ quản lý state transitions và shared data
/// </summary>
public class MechRoninManager : MonoBehaviour
{
    public CharacterNetwork botContext;
    [Header("State Components")]
    [SerializeField] private MechRoninStateBase m_currentState;
    [SerializeField] private MechRonin_Move_State m_moveState;
    [SerializeField] private MechRonin_Landing_State m_landingState;
    [SerializeField] private MechRonin_Attack_State m_attackState;
    [SerializeField] private MechRonin_Special_State m_specialState;

    [Header("Components")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private Transform m_transform;

    [Header("Visual References")]
    [SerializeField] private GameObject[] m_smokeTrails;
    [SerializeField] private GameObject[] m_jetObjects;
    [SerializeField] private GameObject m_weaponMech;
    [SerializeField] private GameObject m_shadowMech;
    [SerializeField] private GameObject m_explosionMech;
    [SerializeField] private GameObject m_smokeGround;

    [Header("Path References")]
    [SerializeField] private List<Transform> m_waypoints = new List<Transform>();
    [SerializeField] private List<Transform> m_attackPoints = new List<Transform>();


    // Public properties để states truy cập
    public Animator Animator => m_animator;
    public Transform Transform => m_transform;
    public GameObject WeaponMech => m_weaponMech;
    public GameObject ShadowMech => m_shadowMech;
    public List<Transform> Waypoints => m_waypoints;
    public List<Transform> AttackPoints => m_attackPoints;
    public MechRonin_Special_State SpecialState => m_specialState;
    public MechRonin_Attack_State AttackState => m_attackState;


    private void Awake()
    {
        if (m_transform == null)
            m_transform = transform;

        //InitializeStates();
    }

    private void OnEnable()
    {
        botContext.ACBotDead += OnBotDead;
        m_weaponMech.SetActive(false);
        //ChangeState(m_moveState);
    }
    private void OnDisable()
    {
        botContext.ACBotDead -= OnBotDead;
    }

    private void OnBotDead(bool isDead)
    {
        if (isDead)
        {
            ChangeState(null);
        }
    }

    private void Update()
    {
        m_currentState?.UpdateState();
        // 🔥 TEST: Bấm phím T để vào Special State ngay
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[TEST] Force enter Special State");
            //ChangeState(m_specialState);
        }
    }

    /// <summary>
    /// Khởi tạo các state components
    /// </summary>
    // private void InitializeStates()
    // {
    //     if (m_moveState == null)
    //         m_moveState = GetComponent<MechRonin_Move_State>();
    //     if (m_landingState == null)
    //         m_landingState = GetComponent<MechRonin_Landing_State>();
    //     if (m_attackState == null)
    //         m_attackState = GetComponent<MechRonin_Attack_State>();
    //     if (m_specialState == null)
    //         m_specialState = GetComponent<MechRonin_Special_State>();
    //
    //     // Inject manager vào các states
    //     m_moveState?.Initialize(this);
    //     m_landingState?.Initialize(this);
    //     m_attackState?.Initialize(this);
    //     m_specialState?.Initialize(this);
    // }

    /// <summary>
    /// Chuyển state
    /// </summary>
    public void ChangeState(MechRoninStateBase newState)
    {
        if (newState == null) return;

        m_currentState?.ExitState();
        m_currentState = newState;
        m_currentState.EnterState();
    }

    /// <summary>
    /// Kích hoạt/tắt smoke trails và jet effects
    /// </summary>
    public void SetTrailEffects(bool active)
    {
        if (m_smokeTrails != null)
        {
            foreach (var smoke in m_smokeTrails)
                if (smoke) smoke.SetActive(active);
        }

        if (m_jetObjects != null)
        {
            foreach (var jet in m_jetObjects)
                if (jet) jet.SetActive(active);
        }
    }

    /// <summary>
    /// Set animation trigger
    /// </summary>
    public void SetAnimation(int animHash)
    {
        if (m_animator != null)
            m_animator.SetTrigger(animHash);
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        // Waypoints
        if (m_waypoints != null && m_waypoints.Count > 0)
        {
            for (int i = 0; i < m_waypoints.Count; i++)
            {
                if (m_waypoints[i] != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(m_waypoints[i].position, 0.5f);

                    if (i < m_waypoints.Count - 1 && m_waypoints[i + 1] != null)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(m_waypoints[i].position, m_waypoints[i + 1].position);
                    }
                }
            }
        }

        // Attack points
        if (m_attackPoints != null && m_attackPoints.Count > 0)
        {
            for (int i = 0; i < m_attackPoints.Count; i++)
            {
                if (m_attackPoints[i] != null)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(m_attackPoints[i].position, 0.7f);

                    if (i < m_attackPoints.Count - 1 && m_attackPoints[i + 1] != null)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(m_attackPoints[i].position, m_attackPoints[i + 1].position);
                    }
                }
            }
        }
    }
}

/// <summary>
/// Base class cho tất cả Mech Ronin states
/// </summary>
public abstract class MechRoninStateBase : MonoBehaviour
{
    protected MechRoninManager manager;

    public virtual void Initialize(MechRoninManager manager)
    {
        this.manager = manager;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}
