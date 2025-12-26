using System.Collections;
using UnityEngine;
using GameUtilities;
using static GameConstants;

/// <summary>
/// State xử lý hành vi đặc biệt khi máu thấp.
/// Flow: Attack → Dash trái (45°) → Dash phải (-45°) → SwordSwitch → Quay lại Attack State.
/// </summary>
public class MechRonin_Special_State : StateBase
{
    /// <summary>
    /// Các phase trong Special State.
    /// </summary>
    private enum SpecialPhase
    {
        Attack,       // Attack một lần trước khi dash
        DashLeft,     // Dash sang trái
        DashRight,    // Dash sang phải
        SwordSwitch,  // Đổi kiếm
        ReturnToAttack // Trở về Attack State
    }
    [SerializeField] private Animator m_animator;
    public readonly int RocketPhase3 = Animator.StringToHash("RocketPhase3");

    [Header("Special State Settings")]
    [Tooltip("Ngưỡng máu thấp (tính theo %) để kích hoạt special state.")]
    [SerializeField] private float m_lowHealthThreshold = 0.3f;

    [Tooltip("Thời gian thực hiện dash (giây).")]
    [SerializeField] private float m_dashDuration = 0.8f;

    [Tooltip("Thời gian đổi kiếm (giây).")]
    [SerializeField] private float m_swordSwitchDuration = 3.3f;

    [Tooltip("Khoảng cách mỗi dash (m).")]
    [SerializeField] private float m_dashDistance = 15f;

    [Tooltip("Tốc độ xoay (deg/s).")]
    [SerializeField] private float m_rotationSpeed = 6f;

    [Header("Phase Delays")]
    [Tooltip("Delay giữa DashLeft và DashRight.")]
    [SerializeField] private float m_delayBetweenDashPhases = 2f;

    [Tooltip("Delay giữa Attack và Dash đầu tiên.")]
    [SerializeField] private float m_delayAttack = 2f;

    [Tooltip("Delay sau Dash trước khi vào SwordSwitch.")]
    [SerializeField] private float m_delayAfterDashBeforeSwordSwitch = 1f;

    [Tooltip("Delay sau SwordSwitch trước khi trở về Attack.")]
    [SerializeField] private float m_delayAfterSwordSwitchBeforeReturn = 3f;

    [Header("Usage Restrictions")]
    [Tooltip("Số lần tối đa được dùng trong một trận.")]
    [SerializeField] private int m_maxUsesPerBattle = 1;

    [Tooltip("Cooldown giữa 2 lần dùng Special (giây).")]
    [SerializeField] private float m_cooldownAfterSpecial = 5f;

    [Header("Debug Info (Runtime)")]
    [SerializeField] private SpecialPhase m_currentPhase;
    [SerializeField] private float m_phaseTimer;


    // Runtime states
    private bool m_isActive = false;
    private int m_specialUses = 0;
    private float m_lastSpecialTime = 0f;

    /// <inheritdoc />
    public override void EnterState()
    {
        m_isActive = true;
        m_currentPhase = SpecialPhase.Attack;
        m_phaseTimer = 0f;

        m_specialUses++;
        m_lastSpecialTime = Time.time;

        ExecuteCurrentPhase();
    }

    /// <inheritdoc />
    public override void UpdateState()
    {
        if (!m_isActive || botContext.botNetwork.IsDead) return;
        m_phaseTimer += Time.deltaTime;

        if (m_currentPhase == SpecialPhase.DashLeft || m_currentPhase == SpecialPhase.DashRight)
            TrackPlayerDuringDash();
    }

    /// <inheritdoc />
    public override void ExitState()
    {
        m_isActive = false;
        m_phaseTimer = 0f;
        StopAllCoroutines();
    }

    #region Phase Execution

    /// <summary>
    /// Thực thi phase hiện tại.
    /// </summary>
    private void ExecuteCurrentPhase()
    {
        switch (m_currentPhase)
        {
            case SpecialPhase.Attack: Attacking(); break;
            case SpecialPhase.DashLeft:
            case SpecialPhase.DashRight: StartDashPhase(); break;
            case SpecialPhase.SwordSwitch: StartSwordSwitchPhase(); break;
            case SpecialPhase.ReturnToAttack: StartReturnToAttackPhase(); break;
        }
    }

    /// <summary>
    /// Thực hiện Attack trước khi dash.
    /// </summary>
    private void Attacking()
    {
        botContext.SetAnimation(AnimationParameters.AttackPhase3);
        StartCoroutine(SingleAttack());
    }

    private IEnumerator SingleAttack()
    {
        yield return new WaitForSeconds(m_delayAttack);
        m_currentPhase = SpecialPhase.DashLeft;
        ExecuteCurrentPhase();
    }

    /// <summary>
    /// Bắt đầu Dash sang trái hoặc phải.
    /// </summary>
    private void StartDashPhase()
    {
        Vector3 currentForward = botContext.botNetwork.TF.forward;
        Vector3 dashDirection = (m_currentPhase == SpecialPhase.DashLeft)
            ? Quaternion.Euler(0, -45, 0) * currentForward
            : Quaternion.Euler(0, 45, 0) * currentForward;

        botContext.SetAnimation(m_currentPhase == SpecialPhase.DashLeft
            ? AnimationParameters.DashAttack
            : AnimationParameters.DashAttack2);

        dashDirection.Normalize();
        Vector3 targetPosition = botContext.botNetwork.TF.position + dashDirection * m_dashDistance;

        StartCoroutine(DashToPosition(targetPosition, dashDirection));
    }

    /// <summary>
    /// Bắt đầu phase SwordSwitch.
    /// </summary>
    private void StartSwordSwitchPhase()
    {
        botContext.SetAnimation(AnimationParameters.SwordSwitch);
        StartCoroutine(WaitForSwordSwitch());
    }

    /// <summary>
    /// Kết thúc Special và quay về Attack State.
    /// </summary>
    private void StartReturnToAttackPhase()
    {
        var attackState = GetComponent<MechRonin_Attack_State>();
        attackState.OnReturnFromSpecial();
        botContext.stateController.ChangeState(EnemyState.Attack);

        m_isActive = false;
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Dash sang vị trí mới, sau đó chuyển phase tiếp theo.
    /// </summary>
    private IEnumerator DashToPosition(Vector3 targetPosition, Vector3 dashDirection)
    {
        Vector3 startPosition = botContext.botNetwork.TF.position;
        float elapsed = 0f;

        // Xoay ngay về hướng dash khi bắt đầu
        botContext.botNetwork.TF.rotation = Quaternion.LookRotation(dashDirection);

        while (elapsed < m_dashDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / m_dashDuration;

            botContext.botNetwork.TF.position = Vector3.Lerp(startPosition, targetPosition, progress);

            // Gần cuối dash thì bắt đầu xoay dần về player
            if (progress > 0.7f && PlayerInstant.Instance != null)
            {
                Vector3 playerDir = (PlayerInstant.Instance.ExplosionPos.position - botContext.botNetwork.TF.position).normalized;
                Quaternion targetRot = Quaternion.LookRotation(playerDir);
                botContext.botNetwork.TF.rotation = Quaternion.Slerp(
                        botContext.botNetwork.TF.rotation,
                    targetRot,
                    (progress - 0.7f) * (m_rotationSpeed * Time.deltaTime)
                );
            }
            yield return null;
        }

        // Phase tiếp theo
        if (m_currentPhase == SpecialPhase.DashLeft)
        {
            yield return new WaitForSeconds(m_delayBetweenDashPhases);
            m_currentPhase = SpecialPhase.DashRight;
        }
        else if (m_currentPhase == SpecialPhase.DashRight)
        {
            yield return new WaitForSeconds(m_delayAfterDashBeforeSwordSwitch);
            m_currentPhase = SpecialPhase.SwordSwitch;
        }

        ExecuteCurrentPhase();
    }

    /// <summary>
    /// Chờ SwordSwitch xong rồi quay về Attack.
    /// </summary>
    private IEnumerator WaitForSwordSwitch()
    {
        yield return new WaitForSeconds(m_swordSwitchDuration);
        m_animator.SetTrigger(RocketPhase3);
        yield return new WaitForSeconds(m_swordSwitchDuration * 0.2f);
        botContext.SetAnimation(AnimationParameters.Ultimate);
        yield return new WaitForSeconds(m_swordSwitchDuration);
        yield return new WaitForSeconds(m_delayAfterSwordSwitchBeforeReturn);

        m_currentPhase = SpecialPhase.ReturnToAttack;
        ExecuteCurrentPhase();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Xoay theo player trong khi dash.
    /// </summary>
    private void TrackPlayerDuringDash()
    {
        if (PlayerInstant.Instance == null) return;

        Vector3 playerDirection = (PlayerInstant.Instance.ExplosionPos.position - botContext.botNetwork.TF.position).normalized;
        botContext.botNetwork.TF.rotation = Quaternion.Slerp(
                botContext.botNetwork.TF.rotation,
            Quaternion.LookRotation(playerDirection),
            m_rotationSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Kiểm tra bot có đủ điều kiện vào Special Mode hay không.
    /// </summary>
    public bool ShouldEnterSpecialMode(float currentHealthPercent)
    {
        bool withinLimit = m_specialUses < m_maxUsesPerBattle;
        bool cooldownPassed = Time.time - m_lastSpecialTime >= m_cooldownAfterSpecial;
        bool healthLow = currentHealthPercent <= m_lowHealthThreshold;

        return healthLow && withinLimit && cooldownPassed && !m_isActive;
    }

    /// <summary>
    /// Trả về true nếu đang ở Special Mode.
    /// </summary>
    public bool IsInSpecialMode() => m_isActive;

    #endregion
}
