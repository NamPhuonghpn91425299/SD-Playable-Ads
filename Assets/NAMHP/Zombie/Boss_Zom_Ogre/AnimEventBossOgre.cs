using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventBossOgre : MonoBehaviour
{
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip[] soundOnStep;
    [SerializeField] public GameObject[] indicatorsAttack2;
    [SerializeField] public GameObject indicatorsAttack1;
    [SerializeField] public GameObject bloodPrefab; // Hiệu ứng máu khi bị tấn công
    [SerializeField] public Animator animator;
    private bool canShakeOnStep = false;
    
    [Header("Attack Origin & Target")]
    public Transform attackPoint;       // Điểm bắt đầu của chuỗi nổ (ví dụ: nòng súng, tay nhân vật)
    public Transform spawnBulletPoint;       // Điểm bắt đầu của chuỗi nổ (ví dụ: nòng súng, tay nhân vật)
    public Transform currentTarget;     // Mục tiêu (Transform của đối tượng địch hoặc một điểm trên mặt đất)

    [Header("Explosion Line Settings")]
    public GameObject explosionPrefab;   // KÉO PREFAB HIỆU ỨNG NỔ CỦA BẠN VÀO ĐÂY TRONG INSPECTOR
    public GameObject explosionPrefab1;
    public GameObject explosionPrefab2;
    public int numberOfExplosions = 3;
    public float timeBetweenSpawns = 0.2f;
    public bool groundExplosions = true; // Đặt hiệu ứng nổ trên mặt đất (Y=0)
    
    [Header("Attack 1 Setting")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 3f;
    public float curveStrength = 20f ; // Độ cong của đường đạn
    
    public readonly int PARAMETER_Attack1_AnimScale = Animator.StringToHash("Attack1_AnimScale");
    public readonly int PARAMETER_Attack2_AnimScale = Animator.StringToHash("Attack2_AnimScale");
    
    public Action OnPlaySound1;
    public Action OnPlaySound2;
    bool isPlaySound = false;

    private void OnEnable()
    {
        OnPlaySound1 += OnPlaySound1Handler;
        OnPlaySound2 += OnPlaySound2Handler;
    }

    private void OnDisable()
    {
        OnPlaySound1 -= OnPlaySound1Handler;
        OnPlaySound2 -= OnPlaySound2Handler;
    }

    private void OnPlaySound2Handler()
    {
        audioSource.PlayOneShot(soundOnStep[0]);
    }

    private void OnPlaySound1Handler()
    {
        audioSource.PlayOneShot(soundOnStep[1]);
    }

    public void SetCanShakeStep(bool value)
    {
        canShakeOnStep = value;
    }

    public void ShakeOnStep()
    {
        if (canShakeOnStep)
        {
            Vibration.Instance.StartShaking();
            //Debug.Log("Shake on step triggered");
        }
    }

    public void PlaySound1()
    {
        OnPlaySound1?.Invoke();
        // if (audioSource != null)
        // {
        //     audioSource.PlayOneShot(soundOnStep[0]);
        // }
        
    }

    public void PlaySound2()
    {
        OnPlaySound2?.Invoke();
        // if (audioSource != null)
        // {
        //     audioSource.PlayOneShot(soundOnStep[1]);
        // }
        
    }
    public void ShowAttack1Indicator()
    {
        indicatorsAttack1.SetActive(true);
        animator.SetFloat(PARAMETER_Attack1_AnimScale, 0.6f);
        Debug.Log("Show Attack 1 Indicator");

    }
    public void HideAttack1Indicator()
    {
        indicatorsAttack1.SetActive(false);
        animator.SetFloat(PARAMETER_Attack1_AnimScale, 1f);
        Debug.Log("Hide Attack 1 Indicator");
    }
    public void SetOnBulletFake()
    {
        bloodPrefab.SetActive(true);
    }
    public void SetOffBulletFake()
    {
        bloodPrefab.SetActive(false);
    }

    public void SpawnBloodBullet()
    {
        currentTarget = LocalPlayer.Instance.GetTransformExplosion();
        GameObject bullet = ObjectPool.Instance.PopFromPool(bulletPrefab, instantiateIfNone: true);
        if (bullet != null)
        {
            bullet.transform.SetPositionAndRotation(spawnBulletPoint.position, spawnBulletPoint.rotation);
            bullet.SetActive(true); // Kích hoạt hiệu ứng máu
            BulletBezier bulletScript = bullet.GetComponent<BulletBezier>();
            bulletScript.Init(spawnBulletPoint.position, currentTarget.position, bulletSpeed, curveStrength);
        }
        else
        {
            Debug.LogError("Failed to spawn blood bullet: bloodPrefab is null or not set in the pool.");
        }
    }
    public void ShowAttack2Indicator()
    {
        foreach (GameObject indicator in indicatorsAttack2)
        {
            if (indicator != null)
            {
                indicator.SetActive(true);
            }
        }
        
        animator.SetFloat(PARAMETER_Attack2_AnimScale, 0.3f);
        Debug.Log("Show Attack 2 Indicator");
    }
    public void HideAttack2Indicator()
    {
        foreach (GameObject indicator in indicatorsAttack2)
        {
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
        }
        animator.SetFloat(PARAMETER_Attack2_AnimScale, 1f);
        Debug.Log("Hide Attack 2 Indicator");
    }
    
    public void ShowExplosion()
    {
        currentTarget = LocalPlayer.Instance.GetTranformPlayer();
        SpawnExplosionLine(
            attackPoint.position,
            currentTarget.position,
            numberOfExplosions,
            explosionPrefab,
            explosionPrefab1,
            explosionPrefab2,
            timeBetweenSpawns,
            groundExplosions
        );
        Debug.Log("Start Explosion Logic");
    }
    
    /// <summary>
    /// Spawns a sequence of explosion effects from a start point to an end point.
    /// </summary>
    /// <param name="startPosition">The world position where the explosions start.</param>
    /// <param name="targetPosition">The world position where the explosions end.</param>
    /// <param name="numberOfExplosions">Total number of explosions in the sequence.</param>
    /// <param name="explosionEffectPrefab">The prefab for the explosion effect.</param>
    /// <param name="timeBetweenSpawns">Delay in seconds between each explosion spawn.</param>
    /// <param name="applyGroundingY">If true, sets the Y position of each explosion to 0 (ground level).</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    public Coroutine SpawnExplosionLine(
        Vector3 startPosition,
        Vector3 targetPosition,
        int numberOfExplosions,
        GameObject explosionEffectPrefab,
        GameObject explosionEffectPrefab1,
        GameObject explosionEffectPrefab2,
        float timeBetweenSpawns,
        bool applyGroundingY = true
    )
    {
        // Kiểm tra đầu vào cơ bản
        if (explosionEffectPrefab == null)
        {
            Debug.LogError("SpawnExplosionLine: explosionEffectPrefab is null!");
            return null;
        }
        if (numberOfExplosions <= 0)
        {
            Debug.LogWarning("SpawnExplosionLine: numberOfExplosions is 0 or less. No explosions will spawn.");
            return null;
        }

        return StartCoroutine(DoSpawnExplosionLine(
            startPosition,
            targetPosition,
            numberOfExplosions,
            explosionEffectPrefab,
            explosionEffectPrefab1,
            explosionEffectPrefab2,
            timeBetweenSpawns,
            applyGroundingY
        ));
    }

    public IEnumerator DoSpawnExplosionLine(
        Vector3 startPosition,
        Vector3 targetPosition,
        int numberOfExplosions,
        GameObject explosionEffectPrefab,
        GameObject explosionEffectPrefab1,
        GameObject explosionEffectPrefab2,
        float timeBetweenSpawns,
        bool applyGroundingY)
    {
        // Trường hợp chỉ có 1 vụ nổ
        if (numberOfExplosions == 1)
        {
            Vector3 spawnPos = targetPosition; // Vụ nổ duy nhất sẽ ở điểm cuối
            if (applyGroundingY)
            {
                spawnPos.y = 0;
            }

            GameObject fxInstance = ObjectPool.Instance.PopFromPool(explosionEffectPrefab, instantiateIfNone: true);
            GameObject fxInstance1 = ObjectPool.Instance.PopFromPool(explosionEffectPrefab1, instantiateIfNone: true);
            // GameObject fxInstance = Instantiate(explosionEffectPrefab); // Tạm thời nếu không có pool
            if (fxInstance != null)
            {
                fxInstance.transform.position = spawnPos;
                fxInstance.SetActive(true); // Đảm bảo hiệu ứng được kích hoạt
                fxInstance1.transform.position = spawnPos;
                fxInstance1.SetActive(true); // Đảm bảo hiệu ứng được kích hoạt
                // fxInstance.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f); // Ví dụ scale
            }
            // Debug.Log($"Spawned single explosion at {spawnPos}");
            yield break; // Kết thúc
        }

        // Vòng lặp cho nhiều vụ nổ
        for (int i = 0; i < numberOfExplosions; i++)
        {
            // (float)i / (numberOfExplosions - 1) sẽ đi từ 0 đến 1
            float t = (float)i / (numberOfExplosions - 1);
            Vector3 spawnPosition = Vector3.Lerp(startPosition, targetPosition, t);

            if (applyGroundingY)
            {
                spawnPosition.y = 0;
            }
            // Kiểm tra xem có phải vụ nổ cuối cùng không
            bool isLastExplosion = (i == numberOfExplosions - 1);
            GameObject prefabToUse = isLastExplosion && explosionEffectPrefab2 != null ? explosionEffectPrefab2 : explosionEffectPrefab;

            // Spawn hiệu ứng từ Object Pool (hoặc Instantiate)
            GameObject fxInstance = ObjectPool.Instance.PopFromPool(prefabToUse, instantiateIfNone: true);
            fxInstance.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            
            GameObject fxInstance1 = ObjectPool.Instance.PopFromPool(explosionEffectPrefab1, instantiateIfNone: true);
            fxInstance1.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);

            // Chờ giữa các vụ nổ, trừ vụ nổ cuối cùng (vì không còn vụ nào sau nó)
            if (i < numberOfExplosions - 1) // Chỉ đợi nếu chưa phải là vụ nổ cuối
            {
                if (timeBetweenSpawns > 0)
                {
                    yield return new WaitForSeconds(timeBetweenSpawns);
                }
                else
                {
                    yield return null; // Chờ 1 frame nếu timeBetweenSpawns là 0
                }
            }
        }

    }
}
public enum ZombieAudioType
{
    Idle,
    Walk,
    Run,
    Attack1,
    Attack2,
    Die,
    Hit,
    Stun,
    FootStep1,
    FootStep2,
    Breathe,
    Scream,
    DieBodyFall,
    WeaknessExplode,
    Pain1,
    Pain2,
    Rock1,
    Rock2,
    SlashAttack,
    SpinAttack,
    Roar,
    FireBall,
    FireBallExplode,
    SmashAttack,
    Stab,
    RipupRock,
    Landing,
    HeadExplode,
    BodyExplode,
    StartUp,
    Flying,
    ExplosionBomb,
}