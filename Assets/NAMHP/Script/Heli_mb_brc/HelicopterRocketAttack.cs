using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random; // Cần cho FirstOrDefault

// Giả sử bạn có ObjectPool và các lớp dữ liệu cần thiết khác

public class HelicopterRocketAttack : MonoBehaviour
{
    [Header("TARGET & PREFAB")]
    public Transform target;
    public GameObject rocketPrefab; // Prefab phải có component VolleyRocketMovement

    [Header("LAUNCHERS")]
    public Transform leftLauncher;
    public ParticleSystem[] launcherEffect; // Hiệu ứng phóng bên trái
    public Transform rightLauncher;
    //public ParticleSystem[] rightLauncherEffect; // Hiệu ứng phóng bên phải
    private List<Transform> launchers = new List<Transform>(); // Để dễ quản lý

    [Header("VOLLEY ATTACK SETTINGS")]
    public int rocketsPerVolley = 6; // Tổng số rocket trong 1 loạt bắn (chia đều 2 bên)
    public float timeBetweenRocketsInVolley = 0.2f; // Thời gian giữa các quả rocket trong loạt
    public float timeBetweenVolleys = 3.0f; // Thời gian nghỉ giữa các loạt bắn

    [Header("ROCKET TRAJECTORY")]
    public float initialStraightDistance = 15f; // Khoảng cách rocket bay thẳng ban đầu
    public float trajectorySpreadRadius = 5f; // Bán kính tối đa lệch điểm đến so với mục tiêu

    [Header("ROCKET ATTRIBUTES")]
    public float rocketSpeed = 50f;
    public float rocketRotationSpeed = 180f; // Tốc độ xoay khi bám đuổi/điều chỉnh hướng
    public float autoExplodeTime = 8f;
    public float explosionRadius = 5f;
    public int damage = 100;
    // public ExplosionAttribute explosionAttrib; // Hoặc dùng cấu trúc như trong code gốc

    [Header("POOLING")]
    public int initialPoolSize = 20;
    private List<VolleyRocketMovement> rocketPool = new List<VolleyRocketMovement>();
    private Coroutine attackCoroutine = null;
    private bool isAttacking = false;

    void Awake()
    {
        // Thêm các launcher vào danh sách nếu chúng được gán
        if (leftLauncher != null) launchers.Add(leftLauncher);
        if (rightLauncher != null) launchers.Add(rightLauncher);

        PrepareRocketPool();
    }

    private void Start()
    {
        target = LocalPlayer.Instance.GetTranExplosion();
    }

    void OnEnable()
    {
        // Lấy mục tiêu nếu chưa có (ví dụ từ một nguồn toàn cục)
        // if (target == null && TUtilities.TargetTrans != null)
        // {
        //     target = TUtilities.TargetTrans;
        // }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) // Ví dụ: nhấn phím Space để bắt đầu tấn công
        {
            StartAttack();
        }
        else if (Input.GetKeyDown(KeyCode.G)) // Nhấn phím G để dừng tấn công
        {
            StopAttack();
        }
        //UpdateTarget(target);
    }

    void OnDisable()
    {
        StopAttack(); // Dừng tấn công khi bị disable
    }

    void PrepareRocketPool()
    {
        if (rocketPrefab == null)
        {
            Debug.LogError("Rocket Prefab is not assigned!", this);
            return;
        }
        if (rocketPrefab.GetComponent<VolleyRocketMovement>() == null)
        {
             Debug.LogError("Rocket Prefab needs a VolleyRocketMovement component!", this);
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject rocketObj = Instantiate(rocketPrefab);
            VolleyRocketMovement rocketMovement = rocketObj.GetComponent<VolleyRocketMovement>();
            // Cấu hình pool nếu cần (ví dụ: tham chiếu ngược lại pool manager)
            rocketObj.SetActive(false);
            rocketPool.Add(rocketMovement);
        }
    }

    public void StartAttack(Transform newTarget = null)
    {
        if (newTarget != null)
        {
            target = newTarget;
        }

        if (target == null)
        {
            Debug.LogWarning("Cannot start attack: Target is null.", this);
            return;
        }

        if (launchers.Count == 0)
        {
             Debug.LogWarning("Cannot start attack: No launchers assigned.", this);
            return;
        }

        if (!isAttacking)
        {
            isAttacking = true;
            attackCoroutine = StartCoroutine(AttackCycleCoroutine());
            Debug.Log("Helicopter Attack Started", this);
        }
    }

    public void StopAttack()
    {
        if (isAttacking)
        {
            isAttacking = false;
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            // Có thể dừng các hiệu ứng khác nếu có
            Debug.Log("Helicopter Attack Stopped", this);
        }
    }

    IEnumerator AttackCycleCoroutine()
    {
        while (isAttacking)
        {
            if (target == null) // Kiểm tra mục tiêu còn tồn tại không
            {
                Debug.LogWarning("Target lost, stopping attack.", this);
                StopAttack();
                yield break; // Thoát coroutine
            }

            // Thực hiện một loạt bắn
            yield return StartCoroutine(FireVolleyCoroutine());

            // Nghỉ giữa các loạt bắn
            if (isAttacking) // Kiểm tra lại phòng trường hợp StopAttack được gọi trong lúc FireVolleyCoroutine
            {
                yield return new WaitForSeconds(timeBetweenVolleys);
            }
        }
    }

    IEnumerator FireVolleyCoroutine()
    {
        //Debug.Log($"Firing volley of {rocketsPerVolley} rockets.", this);
        for (int i = 0; i < rocketsPerVolley; i++)
        {
            if (!isAttacking || target == null) yield break; // Dừng nếu trạng thái thay đổi

            // Chọn bệ phóng xen kẽ
            int launcherIndex = i % launchers.Count;
            Transform launchPoint = launchers[launcherIndex];

            // Tính toán điểm đến lệch đi
            Vector3 targetOffset = Random.insideUnitSphere * trajectorySpreadRadius;
            targetOffset.y = Mathf.Abs(targetOffset.y) * 0.2f; // Giảm lệch theo chiều dọc nếu muốn
            Vector3 calculatedTargetPos = target.position + targetOffset;

            // Spawn tên lửa
            SpawnRocket(launchPoint, calculatedTargetPos);

            // Chờ trước khi bắn quả tiếp theo trong loạt
            yield return new WaitForSeconds(timeBetweenRocketsInVolley);
        }
        Debug.Log("Volley finished.", this);
    }

    void SpawnRocket(Transform launchPoint, Vector3 calculatedTargetPos)
    {
        // Tìm một tên lửa không hoạt động trong pool
        VolleyRocketMovement rocket = rocketPool.FirstOrDefault(r => !r.gameObject.activeSelf);

        if (rocket == null)
        {
            // Nếu hết pool, tạo mới (hoặc có thể cảnh báo/bỏ qua)
            Debug.LogWarning("Rocket pool depleted, instantiating new rocket.", this);
            GameObject rocketObj = Instantiate(rocketPrefab);
            rocket = rocketObj.GetComponent<VolleyRocketMovement>();
            // Không thêm vào pool ở đây để tránh pool tăng vô hạn, cần quản lý pool tốt hơn
            // Hoặc là tạo thêm vào pool nếu logic cho phép
            // rocketPool.Add(rocket); // Cẩn thận khi làm việc này
             if (rocket == null) return; // Lỗi prefab
        }

        // Cấu hình và kích hoạt tên lửa
        rocket.transform.position = launchPoint.position;
        rocket.transform.rotation = launchPoint.rotation; // Hướng ban đầu theo bệ phóng
        rocket.gameObject.SetActive(true);

        // Thiết lập thông số cho tên lửa
        rocket.Setup(
            target,
            calculatedTargetPos, // Điểm đến đã tính toán (lệch đi)
            rocketSpeed,
            rocketRotationSpeed,
            initialStraightDistance,
            autoExplodeTime,
            damage,
            explosionRadius
            // , explosionAttrib // Nếu dùng cấu trúc ExplosionAttribute
        );

         // Kích hoạt hiệu ứng phóng (nếu có)
         GetEffects();
         // ParticleSystem launchEffect = launchPoint.GetComponentInChildren<ParticleSystem>();
         // if(launchEffect) launchEffect.Play();
    }

    // (Optional) Cung cấp phương thức để thay đổi mục tiêu đang bay
    private void GetEffects()
    {
        foreach (ParticleSystem launcher in launcherEffect)
        {
            launcher.Play();
        }
    }
    public void UpdateTarget(Transform newTarget)
    {
         target = newTarget;
         // Các tên lửa đang bay có thể cần cập nhật mục tiêu nếu chúng đang bám đuổi
         foreach(var rocket in rocketPool)
         {
             if(rocket.gameObject.activeSelf)
             {
                 rocket.UpdateTargetTransform(newTarget);
             }
         }
    }
}