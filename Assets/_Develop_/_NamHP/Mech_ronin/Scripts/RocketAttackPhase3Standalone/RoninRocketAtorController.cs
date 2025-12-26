using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RoninRocketAtorController : MonoBehaviour
{

    [SerializeField]
    private Animator   m_rocketAtor;
    [SerializeField]
    private GameObject _cannonFlashFx;
    [SerializeField]
    private RocketAttackPhase3Standalone m_rocketAttackPhase3Standalone;
    private void OnEnable()
    {
        if (m_rocketAtor == null)
        {
            m_rocketAtor = this.GetComponent<Animator>();
        }
        _cannonFlashFx.SetActive(false);
    }

    /// <summary>
    /// gọi trong anim rocket phase 2
    /// </summary>
    public void OnAttackNormal()
    {
        //RocketAttackNormal.Publish(5);
    }

    /// <summary>
    /// gọi trong anim rocket phase 3
    /// </summary>
    public void OnAttackPhase3(int posId)
    {
        m_rocketAttackPhase3Standalone.StartAttack(posId);
        _cannonFlashFx.SetActive(true);
    }

    [ContextMenu("Test Rocket")]
    private void TestRocket()
    {
        m_rocketAtor.SetTrigger("RocketPhase3");
    }
}
