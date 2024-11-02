
using UnityEngine;
using System.Collections;

public class ShootingController : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private WeaponDataSO currentWeaponStats;
    public Transform firePoint;

    private bool isShooting = false;
    private bool canShoot = false;
    private float holdTime = 0f;
    private Coroutine shootingCoroutine;
    private Coroutine delayCoroutine;
    public Weapon weapon;

    private void Start()
    {
        weapon = GetComponentInChildren<Weapon>();

        if (firePoint == null)
            firePoint = transform;

        // Kiểm tra weapon stats khi khởi động
        if (currentWeaponStats == null)
        {
            Debug.LogWarning("No weapon stats assigned! Please assign a WeaponStats SO in the inspector.");
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            StartShooting();
            
        }

        if (isShooting && canShoot)
        {
            holdTime += Time.deltaTime;
            holdTime = Mathf.Min(holdTime, currentWeaponStats.timeToMaxFireRate);
            // Giới hạn holdTime không vượt quá currentWeaponStats.timeToMaxFireRate
        }

        if (Input.GetButtonUp("Fire1"))
        {
            StopShooting();
        }
    }

    public void ChangeWeapon(WeaponDataSO newWeaponStats)
    {
        if (newWeaponStats == null)
        {
            Debug.LogError("Attempting to change to null weapon stats!");
            return;
        }

        StopShooting();
        currentWeaponStats = newWeaponStats;
        Debug.Log($"Changed weapon to: {currentWeaponStats.Name}");
    }

    private void StartShooting()
    {
        if (currentWeaponStats == null)
        {
            Debug.LogError("No weapon stats assigned!");
            return;
        }

        isShooting = true;
        canShoot = false;
        holdTime = 0f;

        if (delayCoroutine != null)
            StopCoroutine(delayCoroutine);
        if (shootingCoroutine != null)
            StopCoroutine(shootingCoroutine);

        delayCoroutine = StartCoroutine(InitialDelayCoroutine());
    }

    private IEnumerator InitialDelayCoroutine()
    {
        weapon.WaitPlayShoot();
        Debug.Log($"Charging {currentWeaponStats.Name} with {currentWeaponStats.initialDelay}s delay...");

        yield return new WaitForSeconds(currentWeaponStats.initialDelay);

        canShoot = true;
        Debug.Log($"Started shooting with {currentWeaponStats.Name}!");

        shootingCoroutine = StartCoroutine(ShootingCoroutine());
        weapon.PlayShoot();
    }

    private void StopShooting()
    {
        isShooting = false;
        canShoot = false;
        holdTime = 0f;
        weapon.StopPlayShoot();

        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    private IEnumerator ShootingCoroutine()
    {
        while (isShooting && canShoot)
        {
            float progress = holdTime / currentWeaponStats.timeToMaxFireRate;
            float currentDelay = Mathf.Lerp(
                currentWeaponStats.startFireRate,
                currentWeaponStats.maxFireRate,
                progress
            );
            FireBullet();
            yield return new WaitForSeconds(currentDelay);
        }
    }

    private void OnGUI()
    {
        if (isShooting && currentWeaponStats != null)
        {
            if (!canShoot)
            {
                GUI.Label(new Rect(10, 10, 300, 20),
                    $"Charging {currentWeaponStats.Name}...");
            }
            else
            {
                float progress = holdTime / currentWeaponStats.timeToMaxFireRate;
                float currentFireRate = 1f / Mathf.Lerp(
                    currentWeaponStats.startFireRate,
                    currentWeaponStats.maxFireRate,
                    progress
                );

                GUI.Label(new Rect(10, 10, 300, 20),
                    $"{currentWeaponStats.Name} - Thời gian giữ: {holdTime:F2}s");
                GUI.Label(new Rect(10, 30, 300, 20),
                    $"Tốc độ bắn: {currentFireRate:F1} viên/giây");
            }
        }
    }

    private void FireBullet()
    {
        
        WeaponController.instance.SpawnBullet();
    }
}