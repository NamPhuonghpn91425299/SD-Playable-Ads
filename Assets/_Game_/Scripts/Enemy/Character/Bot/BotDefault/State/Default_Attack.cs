using System;
using System.Collections;
using static GameConstants;
using UnityEngine;
using Random = UnityEngine.Random;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine.Serialization;

public class Default_Attack : StateBase
{
    [Header("AKM Gun")]
    [SerializeField] private int countShoot;
    [SerializeField] private float timeOneShoot;
    [SerializeField] private GameObject muzzle;
    [SerializeField] private int timeDelayChangeStateMax = 3;
    [SerializeField] private int timeDelayChangeStateMin = 0;
    public int animType;
    private bool canSetAnim = true;
    public AudioSource audioSource;
    private void OnEnable()
    { 
        animType = Random.Range(0, 2);
        canSetAnim = true;
    }

    public override void EnterState()
    {

        botContext.botNetwork.RotateToPlayer();
        if (animType == 0)
        {
            StartCoroutine(IEAttack());
            botContext.ChangeAnimAndType(HashAttack, animType);
        }else// if (animType == 1)
        {
            if (canSetAnim)
            {
                botContext.ChangeAnimAndType(HashAttack, animType);
                canSetAnim = false;
            }
            StartCoroutine(IEAttack());
        }
        // else //animType == 2 nằm xuống
        // {
        //     botContext.ChangeAnimAndType(HashAttack, animType);
        // }
    }

    public override void UpdateState()
    {
    }

    private void FixedUpdate()
    {
        if(audioSource!=null&&audioSource.enabled&&GameController.Instance.CurrentGameState != GameState.InGame)
            audioSource.enabled = false;
    }

    public override void ExitState()
    {
        muzzle.gameObject.SetActive(false);
        StopAllCoroutines();
    }

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
        StartCoroutine(IEAttack());
    }
    
    public IEnumerator IEAttack()
    {
        var _timeDelayChangeState = Random.Range(timeDelayChangeStateMin, timeDelayChangeStateMax);
        yield return HelperCoroutine.GetWait(_timeDelayChangeState);
        Vector3 targetPos = GameController.Instance.GetPosLocalPlayer();
        if (animType==2)
        {
            yield return HelperCoroutine.GetWait(1f);
            while (true)
            {
                muzzle.SetActive(true);
                yield return HelperCoroutine.GetWait(timeOneShoot);
                muzzle.SetActive(false);
                if(GameController.Instance.CurrentGameState == GameState.InGame)
                    EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: botContext.botNetwork.Damage, state:"OnlyDamage"));
                yield return HelperCoroutine.GetWait(1.4f);
            }
        }
        else
        {
            yield return HelperCoroutine.GetWait(.5f);
            for (int i = 0; i < countShoot; i++)
            {
                muzzle.SetActive(true);
                yield return HelperCoroutine.GetWait(timeOneShoot);
                muzzle.SetActive(false);
                EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: botContext.botNetwork.Damage, state: "OnlyDamage"));
                if(i == countShoot-1)
                    break;
                yield return HelperCoroutine.GetWait(1.4f);
            }
            botContext.stateController.ChangeState(EnemyState.Reload);
        }
    }
}