using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FighterStateMachine;
using static HelperCoroutine;
public class FighterAttackState : BaseState<FighterState>
{
    [SerializeField] BotNetwork botNetwork;
    [SerializeField] GameObject[] _muzzle;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _audioClip;
    private float reloadTime;
    private bool isAttackDone;
    private float lastAttackTime;
    [SerializeField] private bool isAttack; 
    private Coroutine _Attack;
    [SerializeField] BotConfigSO fighterConfig;
    public Transform player; // Tham chiếu đến đối tượng người chơi
    public Transform leftGun; // Tham chiếu đến súng bên trái
    public Transform rightGun; // Tham chiếu đến súng bên phải
    public float angleWidth = 30f; // Độ rộng tùy chỉnh
    public override void EnterState()
    {
        player = LocalPlayer.Instance.GetTranformPlayer();
        reloadTime = fighterConfig.timeReload;
        isAttackDone = false;
        _Attack = StartCoroutine(FighterAttack());
    }
    private IEnumerator FighterAttack()
    {
        //yield return WaitSeconds(1);
        int timeAttack = Mathf.FloorToInt(fighterConfig.timeAttack * fighterConfig.fireRate);
        for (int i = 0; i < timeAttack; i++)
        {
            SetActiveAll(true);
            //_muzzle.GetComponentInChildren<ParticleSystem>().Play();
            _audioSource.PlayOneShot(_audioClip);
            EventManager.Invoke(EventName.OnTakeDamagePlayer, fighterConfig.damage);
            yield return WaitSeconds(1f / fighterConfig.fireRate);
        }
        SetActiveAll(false);
        _audioSource.Stop();
        isAttackDone = true;
        lastAttackTime = Time.time;
    }    
    public override void UpdateState()
    {
        AimGuns();
    }
    public void SetActiveAll(bool isActive)
    {
        foreach (GameObject obj in _muzzle)
        {
            obj.SetActive(isActive); // Bật hoặc tắt GameObject dựa trên tham số isActive
        }
    }
    void AimGuns()
    {
        // Tính toán hướng tới người chơi
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        
        // Tính toán góc quay cho súng trái và súng phải
        Quaternion leftGunRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(0, -angleWidth / 2, 0);
        Quaternion rightGunRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(0, angleWidth / 2, 0);

        // Áp dụng góc quay cho súng
        leftGun.rotation = leftGunRotation;
        rightGun.rotation = rightGunRotation;
    }
    public override void ExitState()
    {
        isAttack = false;
        SetActiveAll(false);
        StopCoroutine(_Attack);
        _audioSource.Stop();
    }
    public override FighterState GetNextState()
    {
        if (botNetwork.IsDead)
        {
            return FighterState.Dead;
        }
        else
        {
            if (isAttackDone && Time.time >= lastAttackTime + reloadTime)
            {
                return FighterState.Idle;
            }
            return StateKey;

        }
       

    }

}

