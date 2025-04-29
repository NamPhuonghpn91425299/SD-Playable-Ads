using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class VolleyRocketMovement : MonoBehaviour,IPoolObject
{
    [SerializeField] private BotNetwork botNetwork; // BotNetwork để lấy thông tin về Player
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
        botNetwork = GetComponent<BotNetwork>();
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
        botNetwork.Reset();
        explosionPrefab.SetActive(false); // Tắt hiệu ứng nổ khi kích hoạt
        botNetwork.OnBotDead += OnBotDead; // Đăng ký sự kiện khi bot chết
    }

    private void OnDisable()
    {
        botNetwork.OnBotDead -= OnBotDead; // Hủy đăng ký sự kiện
    }

    private void OnBotDead()
    {
        // Nếu bot chết, tên lửa sẽ tự động nổ
        if (gameObject.activeSelf && !isExploding)
        {
            Explode();
        }
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
    
    void Update()
    {
        if (isExploding || !gameObject.activeSelf) return;

        if (isFlyingStraight && rocketType == RocketType.Missile)
            //HandleStraightFlight();
            HandleChaoticStraightFlight();
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
            Debug.Log("Rocket finished straight phase, starting homing/steering.", this);
        }
    }
    [Header("Chaotic Flight Phase (Missile Only)")]
    [Tooltip("How strongly the missile steers randomly during the initial phase. Degrees per second.")]
    [SerializeField] private float chaoticSteeringStrength = 90f; // Điều chỉnh giá trị này
    private void HandleChaoticStraightFlight()
    {
        float moveDistance = speed * Time.deltaTime;

        // 1. Thêm một chút xoay ngẫu nhiên (chủ yếu là pitch và yaw)
        float randomPitch = Random.Range(-1f, 1f) * chaoticSteeringStrength * Time.deltaTime;
        float randomYaw = Random.Range(-1f, 1f) * chaoticSteeringStrength * Time.deltaTime;
        // Áp dụng xoay ngẫu nhiên vào hướng hiện tại
        transform.Rotate(randomPitch, randomYaw, 0, Space.Self);

        // 2. Di chuyển về phía trước theo hướng MỚI sau khi đã xoay
        transform.Translate(Vector3.forward * moveDistance, Space.Self);
        distanceTraveled += moveDistance;

        // 3. Kiểm tra nếu đã hoàn thành giai đoạn bay hỗn loạn
        if (distanceTraveled >= initialStraightDist)
        {
            isFlyingStraight = false;
            //Debug.Log("Rocket finished chaotic phase, starting homing.", this);
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
        //Debug.Log($"Rocket moving towards {finalDestination}", this);
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