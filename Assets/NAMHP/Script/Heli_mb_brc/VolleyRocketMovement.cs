using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class VolleyRocketMovement : MonoBehaviour,IPoolObject
{
    [SerializeField] private RocketType rocketType; // Kiểu tên lửa (nếu cần)
    [SerializeField] private GameObject model; // Mô hình tên lửa (nếu có)
    [SerializeField] private GameObject explosionPrefab; // Prefab hiệu ứng nổ (nếu có)
    [SerializeField] private float delayTime;
    private Transform currentTargetTransform; // Mục tiêu chính (Player)
    private Vector3 finalDestination;         // Điểm đến cụ thể (đã bị lệch)
    private float speed;
    private float rotationSpeed;
    private float initialStraightDist;
    private float autoExplodeTimer;
    private int rocketDamage;
    private float rocketExplosionRadius;
    // private ExplosionAttribute explosionData; // Nếu dùng

    private float distanceTraveled = 0f;
    private bool isExploding = false; // Cờ để kiểm tra trạng thái nổ
    private bool isFlyingStraight = true;
    private Coroutine explodeCoroutine;

    // Nên có Rigidbody để xử lý va chạm tốt hơn
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("VolleyRocketMovement works best with a Rigidbody. Adding one.", this);
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false; // Tắt trọng lực nếu không muốn tên lửa bị rơi
            rb.isKinematic = true; // Dùng kinematic nếu tự di chuyển bằng transform.Translate
            // Hoặc tắt kinematic và di chuyển bằng rb.velocity / rb.AddForce
        }
        // Đảm bảo có Collider để phát hiện va chạm
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning("VolleyRocketMovement needs a Collider for collisions. Adding SphereCollider.", this);
            gameObject.AddComponent<SphereCollider>().isTrigger = true; // Dùng trigger hoặc collision thường
        }
    }

    private void OnEnable()
    {
        explosionPrefab.SetActive(false); // Tắt hiệu ứng nổ khi kích hoạt
    }

    // Hàm được gọi bởi HelicopterRocketAttack để cấu hình tên lửa
    public void Setup(Transform target, Vector3 calculatedDestination, float spd, float rotSpd, float straightDist, float explodeTime, int dmg, float radius)
    {
        this.currentTargetTransform = target;
        this.finalDestination = calculatedDestination;
        this.speed = spd;
        this.rotationSpeed = rotSpd;
        this.initialStraightDist = straightDist;
        this.autoExplodeTimer = explodeTime;
        this.rocketDamage = dmg;
        this.rocketExplosionRadius = radius;

        // Reset trạng thái
        distanceTraveled = 0f;
        isFlyingStraight = true;
        rb.velocity = Vector3.zero; // Reset vận tốc cũ (nếu dùng Rigidbody không kinematic)
        rb.angularVelocity = Vector3.zero;
        isExploding = false;
        // Bắt đầu đếm giờ tự hủy
        if (explodeCoroutine != null) StopCoroutine(explodeCoroutine);
        explodeCoroutine = StartCoroutine(AutoExplodeTimer());
    }

    // (Optional) Cho phép cập nhật mục tiêu chính khi đang bay
    public void UpdateTargetTransform(Transform newTarget)
    {
        currentTargetTransform = newTarget;
        // Lưu ý: finalDestination (điểm lệch) không thay đổi để giữ quỹ đạo phân tán
    }


    // void Update()
    // {
    //     // THÊM KIỂM TRA NÀY: Nếu đang nổ hoặc không active, không làm gì cả
    //     if (isExploding || !gameObject.activeSelf) return;
    //
    //     float moveDistance = speed * Time.deltaTime;
    //
    //     if (isFlyingStraight)
    //     {
    //         // Bay thẳng theo hướng ban đầu
    //         transform.Translate(Vector3.forward * moveDistance, Space.Self);
    //         distanceTraveled += moveDistance;
    //
    //         if (distanceTraveled >= initialStraightDist)
    //         {
    //             isFlyingStraight = false;
    //             // Có thể bật chế độ homing/steering tại đây nếu cần
    //             Debug.Log("Rocket finished straight phase, starting homing/steering.", this);
    //         }
    //     }
    //     else // Đã hết giai đoạn bay thẳng
    //     {
    //         // Hướng về điểm đến cuối cùng (finalDestination)
    //         Vector3 directionToDestination;
    //         if(currentTargetTransform != null) {
    //             // Tùy chọn: Cập nhật finalDestination dựa trên vị trí mới của target + offset ban đầu
    //             // Vector3 offset = finalDestination - targetPositionAtLaunch; // Cần lưu targetPositionAtLaunch trong Setup()
    //             // finalDestination = currentTargetTransform.position + offset;
    //             // Hoặc đơn giản là hướng về điểm đã tính ban đầu:
    //             directionToDestination = (finalDestination - transform.position).normalized;
    //         } else {
    //             // Nếu mục tiêu biến mất, bay thẳng tiếp hoặc bay đến điểm cuối cùng đã biết
    //             directionToDestination = (finalDestination - transform.position).normalized;
    //             if (directionToDestination == Vector3.zero) directionToDestination = transform.forward; // Tránh lỗi chia cho 0
    //         }
    //
    //
    //         // Xoay về hướng đích
    //         if (directionToDestination != Vector3.zero)
    //         {
    //             Quaternion targetRotation = Quaternion.LookRotation(directionToDestination);
    //             transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    //         }
    //
    //         // Di chuyển về phía trước
    //         transform.Translate(Vector3.forward * moveDistance, Space.Self);
    //
    //         // Kiểm tra nếu đã đến rất gần đích (để tránh bay vòng vòng)
    //         if (Vector3.Distance(transform.position, finalDestination) < 1.0f)
    //         {
    //             Debug.Log("Rocket reached final destination.", this);
    //             Explode();
    //         }
    //     }
    // }
    void Update()
    {
        if (isExploding || !gameObject.activeSelf) return;

        if (isFlyingStraight)
            HandleStraightFlight();
        else
            HandleHomingFlight();
    }

    private void HandleStraightFlight()
    {
        float moveDistance = speed * Time.deltaTime;
        transform.Translate(Vector3.forward * moveDistance, Space.Self);
        distanceTraveled += moveDistance;

        if (distanceTraveled >= initialStraightDist)
        {
            isFlyingStraight = false;
            //Debug.Log("Rocket finished straight phase, starting homing/steering.", this);
        }
    }

    private void HandleHomingFlight()
    {
        float moveDistance = speed * Time.deltaTime;
        Vector3 directionToDestination = CalculateDirectionToDestination();

        // Xoay về hướng đích
        if (directionToDestination != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToDestination);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Di chuyển về phía trước
        transform.Translate(Vector3.forward * moveDistance, Space.Self);

        // Kiểm tra gần đích
        if (Vector3.Distance(transform.position, finalDestination) < 1.0f)
        {
            //Debug.Log("Rocket reached final destination.", this);
            Explode();
        }
    }

    private Vector3 CalculateDirectionToDestination()
    {
        if (currentTargetTransform != null)
        {
            return (currentTargetTransform.position - transform.position).normalized;
        }
        else
        {
            Vector3 dir = (finalDestination - transform.position).normalized;
            return dir == Vector3.zero ? transform.forward : dir;
        }
    }


    IEnumerator AutoExplodeTimer()
    {
        yield return new WaitForSeconds(autoExplodeTimer);
        if (gameObject.activeSelf) // Kiểm tra xem nó còn active không (chưa nổ do va chạm)
        {
            //Debug.Log("Rocket auto-exploded.", this);
            Explode();
        }
    }

    // Xử lý va chạm
    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    void HandleCollision(GameObject collidedObject)
    {
        if (isExploding || !gameObject.activeSelf) return;
        
        // Kiểm tra xem có va chạm với mục tiêu hoặc thứ gì đó nên kích nổ không
        // Ví dụ: kiểm tra tag "Player", "Enemy", "Environment"
        // if (collidedObject.CompareTag("Player") || collidedObject.CompareTag("Environment"))
        // {

        //Debug.Log($"Rocket collided with {collidedObject.name}", this);
        Explode();
        // }
        // else if (collidedObject.CompareTag("Projectile")) {
        // Có thể cho tên lửa bị phá hủy bởi đạn khác?
        // gameObject.SetActive(false); // Trả về pool mà không nổ
        //}
    }


    void Explode()
    {
        if (isExploding || !gameObject.activeSelf) return;
        isExploding = true; // Đặt cờ ngay lập tức
        // 1. Tạo hiệu ứng nổ tại vị trí hiện tại
        //GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        GameObject explosionModel = ObjectPool.Instance.PopFromPool(explosionPrefab, instantiateIfNone:true);
        explosionModel.transform.SetPositionAndRotation(transform.position, transform.rotation);
        explosionModel.SetActive(true);

        EventManager.Invoke<float>(EventName.OnTakeDamagePlayer, rocketDamage);
        // 2. Gây sát thương trong bán kính
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, rocketExplosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            // Kiểm tra xem đối tượng có component nhận sát thương không (ví dụ: PlayerHealth, EnemyHealth)
            // PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(rocketDamage);
            // }
            // EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
            // if (enemyHealth != null) { /* Gây sát thương cho enemy */ }

            //Debug.Log($"Potential damage target in explosion radius: {hitCollider.name}"); // Log để kiểm tra
        }

        // 3. Dọn dẹp
        if (explodeCoroutine != null) StopCoroutine(explodeCoroutine); // Dừng coroutine tự hủy nếu đang chạy
        if (rocketType == RocketType.Missile)
        {
            gameObject.SetActive(false); // Tắt tên lửa sau khi nổ
            // Nếu là tên lửa, có thể thêm hiệu ứng hoặc hành động khác
            // Ví dụ: Tạo một vụ nổ lớn hơn, hoặc thêm hiệu ứng âm thanh
        }
        else if (rocketType == RocketType.RocketRpg)
        {
            ObjectPool.Instance.PushToPool(this, gameObject);
            // Nếu là RPG, có thể thêm hiệu ứng khác
            // Ví dụ: Tạo một vụ nổ nhỏ hơn, hoặc thêm hiệu ứng âm thanh khác
        }


    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rocketExplosionRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, finalDestination);
    }

    public GameObject Prefab { get; set; }
    public void Init()
    {
        
    }

    public void OnPushToPool()
    {
        
    }
}
public enum RocketType
{
    Missile,
    RocketRpg,
}