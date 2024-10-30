using UnityEngine;
using System.Collections;

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    [Tooltip("Thời gian chờ trước khi bắt đầu bắn (giây)")]
    public float initialDelay = 1f;
    [Tooltip("Thời gian giữa các phát bắn lúc bắt đầu (giây)")]
    public float startFireRate = 0.2f;
    [Tooltip("Thời gian giữa các phát bắn lúc tối đa (giây)")]
    public float maxFireRate = 0.1f;
    [Tooltip("Thời gian để đạt tốc độ bắn tối đa (giây)")]
    public float timeToMaxFireRate = 1f;

    [Header("Bullet Settings")]

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
    }

    private void Update()
    {
        // Bắt đầu bắn
        if (Input.GetButtonDown("Fire1"))
        {
            StartShooting();
        }

        // Đang bắn và đã qua delay
        if (isShooting && canShoot)
        {
            // Tăng thời gian giữ
            holdTime += Time.deltaTime;
            holdTime = Mathf.Min(holdTime, timeToMaxFireRate);
        }

        // Dừng bắn
        if (Input.GetButtonUp("Fire1"))
        {
            StopShooting();
        }
    }

    private void StartShooting()
    {
        isShooting = true;
        canShoot = false;
        holdTime = 0f;

        // Hủy các coroutine đang chạy (nếu có)
        if (delayCoroutine != null)
            StopCoroutine(delayCoroutine);
        if (shootingCoroutine != null)
            StopCoroutine(shootingCoroutine);

        // Bắt đầu đếm thời gian delay
        delayCoroutine = StartCoroutine(InitialDelayCoroutine());
    }

    private IEnumerator InitialDelayCoroutine()
    {
        weapon.WaitPlayShoot();
        // Hiệu ứng charging hoặc animation có thể thêm ở đây
        Debug.Log("Đang charging...");

        yield return new WaitForSeconds(initialDelay);

        canShoot = true;
        Debug.Log("Bắt đầu bắn!");

        // Bắt đầu bắn sau khi delay
        shootingCoroutine = StartCoroutine(ShootingCoroutine());
        weapon.PlayShoot();
    }

    private void StopShooting()
    {
        isShooting = false;
        canShoot = false;
        holdTime = 0f;
        weapon.StopPlayShoot();
        // Hủy tất cả coroutine
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
            // Tính toán thời gian chờ giữa các phát bắn
            float progress = holdTime / timeToMaxFireRate;
            float currentDelay = Mathf.Lerp(startFireRate, maxFireRate, progress);

            FireBullet();

            // Chờ đến phát bắn tiếp theo
            yield return new WaitForSeconds(currentDelay);
        }
    }

    // Hiển thị debug
    private void OnGUI()
    {
        if (isShooting)
        {
            if (!canShoot)
            {
                GUI.Label(new Rect(10, 10, 200, 20), "Đang charging...");
            }
            else
            {
                float progress = holdTime / timeToMaxFireRate;
                float currentFireRate = 1f / Mathf.Lerp(startFireRate, maxFireRate, progress);

                GUI.Label(new Rect(10, 10, 200, 20), $"Thời gian giữ: {holdTime:F2}s");
                GUI.Label(new Rect(10, 30, 200, 20), $"Tốc độ bắn: {currentFireRate:F1} viên/giây");
            }
        }
    }
    private void FireBullet()
    {
        WeaponController.instance.SpawnBullet();
    }

}