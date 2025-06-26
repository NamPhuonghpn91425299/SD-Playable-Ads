using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FighterStateMachine;
using static HelperCoroutine;
public class FighterAttackState : BaseState<FighterState>
{
    [SerializeField] BotNetwork botNetwork;
    [SerializeField] Transform[] _muzzle;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _audioClip;
    private float reloadTime;
    private bool isAttackDone;
    private float lastAttackTime;
    [SerializeField] private bool isAttack; 
    private Coroutine _Attack;
    [SerializeField] BotConfigSO fighterConfig;
    public Transform player; // Tham chiếu đến đối tượng người chơi
    public float randomDelay; // Độ rộng tùy chỉnh
    public GameObject bulletPrefab;
    [SerializeField] private ParticleSystem[] _muzzleEffect;

    public override void EnterState()
    {
        randomDelay = Random.Range(0.5f, 1.5f);
        player = LocalPlayer.Instance.GetTranformPlayer();
        SetActiveAll(false);
        reloadTime = fighterConfig.timeReload;
        isAttackDone = false;
        _Attack = StartCoroutine(FighterAttack());
        //_audioSource.pitch = Random.Range(0.6f, 0.7f);
        //_audioSource.pitch = 0.6f;
    }
    private IEnumerator FighterAttack()
    {
        yield return WaitSeconds(randomDelay);
        int numAttacks = Mathf.FloorToInt(fighterConfig.timeAttack * fighterConfig.fireRate);

        for (int i = 0; i < numAttacks; i++)
        {
            _audioSource.PlayOneShot(_audioClip);
            SetActiveAll(true);
            for (int j = 0; j < _muzzle.Length; j++)
            {
                Transform muzzle = _muzzle[j];
                // Lấy từ ObjectPool và thiết lập viên đạn
                if (bulletPrefab != null)
                {
                var bulletprb = ObjectPool.Instance.PopFromPool(bulletPrefab, instantiateIfNone: true);
                BulletFromBot bullet = bulletprb.GetComponent<BulletFromBot>();
                Vector3 directionToTarget = (player.position - muzzle.position).normalized;

                bullet.transform.SetPositionAndRotation(muzzle.position, Quaternion.LookRotation(directionToTarget));
                bullet.Initialize(botNetwork.IsImmortal ? 99999f : fighterConfig.damage, directionToTarget);
                }
                EventManager.Invoke(EventName.OnTakeDamagePlayer, fighterConfig.damage);
                // Kích hoạt hiệu ứng particle tương ứng
                if (j < _muzzleEffect.Length && _muzzleEffect[j] != null)
                {
                    ParticleSystem muzzleEffect = _muzzleEffect[j];
                    muzzleEffect.transform.position = muzzle.position; // Đảm bảo particle ở đúng vị trí
                    muzzleEffect.transform.rotation = muzzle.rotation; // Đảm bảo particle quay đúng hướng
                    muzzleEffect.Play();
                }
            }

            yield return WaitSeconds(1f / fighterConfig.fireRate);
        }

        SetActiveAll(false);
        //_audioSource.Stop();
        isAttackDone = true;
        lastAttackTime = Time.time;
    }

    public void SetActiveAll(bool isActive)
    {
        foreach (Transform obj in _muzzle)
        {
            if (obj != null)
            {
                obj.gameObject.SetActive(isActive);
            }
        }
    }


    public void SetActive()
    {
        foreach (ParticleSystem obj in _muzzleEffect)
        {

            obj.Play();
        }
    }

    public override void UpdateState()
    {
        if (!isAttackDone)
        {
            AimGuns();
        }
    }

    void AimGuns()
    {
        // Tính toán hướng tới người chơi
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.Normalize();
        var up = Vector3.Cross(directionToPlayer, player.right);
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
            fighterConfig.targetRotation * Time.deltaTime);

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

