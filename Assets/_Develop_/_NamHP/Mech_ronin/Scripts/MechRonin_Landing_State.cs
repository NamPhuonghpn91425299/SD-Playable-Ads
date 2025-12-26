using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static GameConstants;
using static MechRoninStateController;
/// <summary>
/// State xử lý chuỗi animation landing
/// </summary>
public class MechRonin_Landing_State : StateBase
{
    [SerializeField] private GameObject m_weaponMech;
    [SerializeField] private GameObject m_shadowMech;
    [Header("Animation Timing")]
    [SerializeField] private float m_flyUpReadyDuration = 2f;
    [SerializeField] private float m_flyUpDuration = 2f;
    [SerializeField] private float m_landingIdleDuration = 1.5f;
    [SerializeField] private float m_landingDuration = 8f;

    private WaitForSeconds m_waitFlyUpReady;
    private WaitForSeconds m_waitFlyUp;
    private WaitForSeconds m_waitLandingIdle;
    private WaitForSeconds m_waitLanding;
    private WaitForSeconds m_waitHalfSecond;

    private void Awake()
    {
        m_waitFlyUpReady = new WaitForSeconds(m_flyUpReadyDuration);
        m_waitFlyUp = new WaitForSeconds(m_flyUpDuration);
        m_waitLandingIdle = new WaitForSeconds(m_landingIdleDuration);
        m_waitLanding = new WaitForSeconds(m_landingDuration);
        m_waitHalfSecond = new WaitForSeconds(0.3f);
    }

    public override void EnterState()
    {
        StartCoroutine(LandingSequenceCoroutine());
    }

    public override void UpdateState()
    {
        // Logic chạy trong coroutine
    }

    public override void ExitState()
    {
        StopAllCoroutines();
        botContext.botNetwork.SetIsImmortal(false);
    }

    private IEnumerator LandingSequenceCoroutine()
    {

        Vector3 currentRotation = botContext.botNetwork.TF.eulerAngles;
        botContext.botNetwork.TF.DORotate(
            new Vector3(0f, currentRotation.y, currentRotation.z), 0.3f);

        // FlyUpReady
        botContext.SetAnimation(AnimationParameters.FlyUpReady);
        yield return m_waitFlyUpReady;

        // FlyUp
        botContext.SetAnimation(AnimationParameters.FlyUp);
        yield return m_waitFlyUp;

        // LandingIdle
        botContext.SetAnimation(AnimationParameters.LandingIdle);

        m_weaponMech.SetActive(true);
        yield return m_waitHalfSecond;

        // Move down
        var attackPoints = botContext.botIdentity.AssignedPath.attackPoints;
        if (attackPoints != null && attackPoints.Count > 0)
        {
            botContext.botNetwork.TF.DOMoveY(
                attackPoints[0].position.y, m_landingIdleDuration)
                .SetEase(Ease.InCubic);
        }
        yield return m_waitLandingIdle;

        // Landing
        botContext.SetAnimation(AnimationParameters.Landing);
        m_shadowMech.SetActive(true);
        yield return m_waitLanding;

        Debug.Log("Landing complete! Switching to Attack mode...");

        // Chuyển sang Attack State
        botContext.stateController.ChangeState(EnemyState.Attack);
    }
}
