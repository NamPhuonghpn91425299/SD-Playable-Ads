using System;
using System.Collections;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using static GameConstants;
using UnityEngine;

public class XeToyotahilux_Attack : StateBase
{
    [SerializeField] private GameObject muzzle;
    [SerializeField] private Transform posGun;
    [SerializeField] private float timerAttack;
    [SerializeField] private float turnSpeed = 45f;
    private Vector3 direction;
    private Vector3 localPlayer;
    private Coroutine IEAttackCoroutine;

    public override void EnterState()
    {
        localPlayer = GameController.Instance.GetPosLocalPlayer();
        IEAttackCoroutine = StartCoroutine(IEAttack());
    }

    private IEnumerator IEAttack()
    {
        botContext.ChangeAnimAndType(HashEndStart);
        while (true)
        {
            Vector3 direction = localPlayer - posGun.position;
            direction.y = 0; // giữ nguyên chiều cao

            if (direction.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                posGun.rotation = Quaternion.RotateTowards(posGun.rotation, targetRotation, turnSpeed * Time.deltaTime);

                // Kiểm tra nếu đã quay gần đúng mục tiêu
                if (Quaternion.Angle(posGun.rotation, targetRotation) < 1f)
                {
                    muzzle.SetActive(true);
                    for (int i = 0; i < 5; i++)
                    {
                        if(GameController.Instance.CurrentGameState == GameState.InGame)
                            EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: botContext.botNetwork.Damage*2, state:"OnlyDamage"));
                        if(GameController.Instance.CurrentGameState == GameConstants.GameState.InGame)
                            botContext.audioPlayable.PlayAudio(GameConstants.AudioType.BotAttack);
                        yield return HelperCoroutine.GetWait(timerAttack/5);
                    }
                    muzzle.SetActive(false);
                    botContext.ChangeAnimAndType(HashStart);
                    botContext.stateController.ChangeState(EnemyState.Move);
                    yield break; // hoặc break nếu trong vòng lặp
                }
            }
            yield return null;
        }
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        if (IEAttackCoroutine != null)
        {
            StopCoroutine(IEAttackCoroutine);
            muzzle.gameObject.SetActive(false);
        }
    }
}