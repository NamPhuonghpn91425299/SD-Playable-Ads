using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))] // Đảm bảo luôn có Rigidbody
public class HelicopterMi28Controller : MonoBehaviour,IPoolObject
{

    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] private HelicopterRocketAttack rocketAttack; 
    [SerializeField] private Transform mainRotorTransform;      // Transform cánh quạt chính
    [SerializeField] private Transform secondaryRotorTransform; // Transform cánh quạt phụ
    [SerializeField] private Transform mainBody;             // Transform thân chính (để nghiêng)
    [SerializeField] private Transform playerTransform;      // Transform người chơi (nếu cần)
    [SerializeField] private GameObject helicopterEffect;     // Hiệu ứng khi hoạt động (bụi đất?)
    [SerializeField] private GameObject explosionEffect;      // Prefab hiệu ứng nổ
    [SerializeField] private GameObject explosionEffect1;      // Prefab hiệu ứng nổ
    [SerializeField] private GameObject explosionEffect2;      // Prefab hiệu ứng nổ
    [SerializeField] private GameObject[] brakeDoor;      
    [SerializeField] private GameObject[] deadStep;           // [0]: Cánh quạt quay khi chết, [1]: Xác máy bay tĩnh

    // --- Trạng thái ---
    public bool isAttacking; // Đang tấn công?
    public bool isDead;      // Đã chết? (trigger bởi OnDeath)

    // --- Cài đặt Cánh quạt ---
    [Header("Rotor Settings")]
    [SerializeField] public float maxRotorSpeed = 3000f;     // Tốc độ quay tối đa
    [SerializeField] private float rotorAcceleration = 200f; // Gia tốc cánh quạt
    [SerializeField] private float minLiftSpeed = 1500f;     // Tốc độ tối thiểu để bắt đầu cất cánh

    // --- Cài đặt Di chuyển ---
    [Header("Movement Settings")]
    [SerializeField] public float maxHeight = 25f;            // Độ cao bay tối đa
    [SerializeField] private float liftSpeed = 6f;           // Tốc độ nâng/hạ
    [SerializeField] private float forwardSpeed = 10f;       // Tốc độ bay tới
    [SerializeField] private float takeoffPitchAngle = -15f; // Góc ngẩng đầu khi cất cánh
    [SerializeField] private float forwardPitchAngle = 10f;  // Góc chúc đầu khi bay tới
    [SerializeField] private float turnSpeed = 1f;           // Tốc độ xoay (và nghiêng)
    [SerializeField] private float horizontalThreshold = 6f; // Ngưỡng khoảng cách ngang để tới waypoint
    [SerializeField] private float verticalThreshold = 4f;   // Ngưỡng khoảng cách dọc để tới waypoint
    [SerializeField] private float idleDuration = 1f;        // Thời gian chờ tối thiểu ở trạng thái Idle
    [SerializeField] private float delayAttack = 2f;        // Thời gian chờ tối thiểu ở trạng thái Idle

    public float targetFlySpeed = 20f;       // Tốc độ bay mong muốn khi đạt đến (thay cho forwardSpeed)
    public float acceleration = 15f;         // Gia tốc (đơn vị/giây^2)
    public float deceleration = 5f;         // Giảm tốc (đơn vị/giây^2)
    public float slowingDistance = 10f;     // Khoảng cách bắt đầu giảm tốc khi đến gần waypoint CUỐI CÙNG
    public float stopThreshold = 0.1f;      // Ngưỡng tốc độ coi như đã dừng

    private float currentSpeed = 0f;        // Tốc độ hiện tại của trực thăng
    private float targetSpeed = 0f;     // Tốc độ mục tiêu mà trực thăng đang hướng tới
    // --- Cài đặt Cất cánh & Độ cao ---
    [Header("TakeOff & Altitude Settings")]
    [SerializeField] private float takeoffTransitionHeight = 15f; // Độ cao chuyển từ TakeOff sang ReachingAltitude
    [SerializeField] private float takeoffTiltThreshold = 24f;    // Ngưỡng độ cao để thay đổi góc nghiêng trong TakeOff

    // --- Cài đặt Nghiêng ---
    [Header("Banking Settings")]
    [SerializeField] private float maxBankAngle = 20f; // Góc nghiêng tối đa khi rẽ (trục Z)
    [SerializeField] private float setAngleY = 45f;    // Góc Y ban đầu khi khởi tạo
    [SerializeField] private float maxAngle = 150f; // Góc nghiêng tối đa (trục Z)
    // --- Cài đặt Chết & Nổ ---
    [Header("Death Settings")]
    [SerializeField] private float initialYSpinForce = 3f; // Lực xoay ban đầu quanh trục Y khi chết
    [SerializeField] private LayerMask collisionLayers;                 // Các Layer sẽ kích hoạt nổ khi va chạm lúc chết
    // [SerializeField] private float disableDelayAfterExplosion = 2f; // Sẽ dùng delay sau khi trượt xong
    [SerializeField] private float slideDuration = 1.5f; // Thời gian trượt trên mặt đất
    [SerializeField] private float slideInitialSpeedFactor = 0.5f; // Hệ số tốc độ trượt ban đầu (so với tốc độ va chạm)
    [SerializeField] private float slideFriction = 3f; // Lực "ma sát" làm chậm khi trượt (giá trị càng lớn càng dừng nhanh)
    [SerializeField] private float finalDisableDelay = 2f; // Thời gian chờ sau khi trượt xong mới ẩn đi
    [SerializeField] private bool explosionOnImpact = true; // Có nổ ngay khi chạm đất không?
    [SerializeField] private bool explosionAfterSlide = false; // Có nổ thêm sau khi trượt xong không?

    // --- Waypoints ---
    [SerializeField] public List<Transform> waypoints; // Danh sách các điểm cần bay tới
    public int currentWaypointIndex = 0;               // Index của waypoint hiện tại

    // --- Biến theo dõi nội bộ ---
    public float currentRotorSpeed = 0f; // Tốc độ cánh quạt hiện tại
    private float idleTimer = 0f;        // Bộ đếm thời gian cho trạng thái Idle
    private bool isAttacked;             // Đã thực hiện logic tấn công?
    public HelicopterState currentState = HelicopterState.Idle; // Trạng thái hiện tại
    [SerializeField] private bool isEndGame; // Cờ kiểm tra kết thúc game (nếu cần)
    [SerializeField] private AudioSource audioSource; // Nguồn phát âm thanh động cơ

    // --- Components nội bộ ---
    private Rigidbody rb;           // Component Rigidbody
    private bool hasExploded = false; // Cờ đánh dấu đã nổ hay chưa


    // Enum quản lý trạng thái
    public enum HelicopterState
    {
        Idle,           // Chờ
        StartingRotors, // Khởi động cánh quạt
        TakeOff,        // Cất cánh
        ReachingAltitude,// Đạt độ cao mục tiêu
        Flying,         // Bay tới waypoint
        Hovering,       // Bay tại chỗ
        Attacking,      // Tấn công
        Dead,           // Đang rơi (sau khi bị bắn hạ)

    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("HelicopterController yêu cầu phải có Rigidbody component!", this);
            enabled = false; // Vô hiệu hóa script nếu không có Rigidbody
            return;
        }
        // Bắt đầu với Rigidbody kinematic để điều khiển chuyển động thủ công
        rb.isKinematic = true;
        rb.useGravity = false;

        botNetwork = GetComponent<BotNetwork>();
        waypoints = botNetwork.Path.AttackWayPoints;
    }

    private void OnEnable()
    {
        // Reset trạng thái khi được kích hoạt (quan trọng cho pooling)
        ResetState();
        if(audioSource && !audioSource.isPlaying) audioSource.Play(); // Chơi âm thanh nếu chưa chạy
        botNetwork.OnBotDead += OnDeath;
    }

    private void Start()
    {
        playerTransform = LocalPlayer.Instance.GetTranformPlayer();
        waypoints = botNetwork.Path.AttackWayPoints;
        // Đặt góc quay ban đầu (chỉ chạy 1 lần khi Start)
         transform.rotation = Quaternion.Euler(0f, setAngleY, 0f);
    }

    private void OnDisable()
    {
        // Đảm bảo dừng âm thanh nếu bị vô hiệu hóa bất ngờ
        if (audioSource != null && audioSource.isPlaying)
        {
             audioSource.Stop();
        }
        CancelInvoke(nameof(DisableHelicopter)); // Hủy invoke nếu có
        botNetwork.OnBotDead -= OnDeath;
    }
    private void SpawnExpolosion()
    {
        // Tạo hiệu ứng nổ tại vị trí này
        GameObject explosion = ObjectPool.Instance.PopFromPool(explosionEffect2, instantiateIfNone: true);
        explosion.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        explosion.SetActive(true);
    }
    // Hàm được gọi từ bên ngoài để kích hoạt trạng thái chết
    public void OnDeath()
    {
        rocketAttack.StopAttack();
        // gọi vụ nổ
        SpawnExpolosion();
        currentState = HelicopterState.Dead;
        isDead = true;
        isAttacking = false;
        hasExploded = false; // Reset cờ đã nổ
        helicopterEffect.SetActive(false); // Tắt hiệu ứng bình thường
        brakeDoor[0].SetActive(false);
        brakeDoor[1].SetActive(true);
        deadStep[0].SetActive(true); // Kích hoạt cánh quạt quay lúc chết
        deadStep[1].SetActive(false); // Tạm thời ẩn xác máy bay tĩnh
        BotDeath.Instance.GetBotDeath();
        EventManager.Invoke(EventName.OnStaticBotDead, true);
        Explode();
        // Chuyển sang chế độ rơi bằng vật lý
        if (rb != null)
        {
            rb.isKinematic = false; // Cho phép vật lý tác động
            rb.useGravity = true; // Bật trọng lực
            rb.velocity = Vector3.zero; // Reset vận tốc cũ
            rb.angularVelocity = Vector3.zero; // Reset vận tốc góc cũ

            // ---> ÁP DỤNG LỰC XOAY QUANH TRỤC Y <---
            // Áp dụng lực xoay tức thời chỉ quanh trục Y của thế giới (Vector3.up)
            //rb.AddTorque(Vector3.up * initialYSpinForce, ForceMode.VelocityChange);
            // set trục xoay là trục Y của thế giới, nhưng giữ lại trục X và Z của helicopter ở thời điêmr đó
            
            rb.AddRelativeTorque(Vector3.up * initialYSpinForce, ForceMode.VelocityChange);
            
            Debug.Log($"OnDeath: Applied Y-axis torque. Force: {initialYSpinForce}");
            // ---> KẾT THÚC ÁP DỤNG LỰC XOAY <---
        }
    }

    private void Update()
    {
        // Ví dụ kiểm tra dừng âm thanh khi end game
        if (UIEndGame.Instance.IsShowEndGame && !isEndGame)
        {
            if(audioSource) audioSource.Stop();
            isEndGame = true;
        }

        // Xử lý theo trạng thái hiện tại
        switch (currentState)
        {
            case HelicopterState.Idle:
                HandleIdleState();
                break;
            case HelicopterState.StartingRotors:
                HandleStartingRotorsState();
                break;
            case HelicopterState.TakeOff:
                 HandleTakeOffState();
                break;
            case HelicopterState.ReachingAltitude:
                HandleReachingAltitudeState();
                break;
            case HelicopterState.Flying:
                HandleFlyingState();
                break;
            case HelicopterState.Hovering:
                HandleHoveringState();
                break;
            case HelicopterState.Attacking:
                HandleAttackingState();
                break;
            case HelicopterState.Dead:
                // Vật lý đang điều khiển việc rơi. Chỉ cần quay cánh quạt hỏng cho đẹp (nếu cần).
                HandleDeadState();
                // Việc xử lý va chạm nằm trong OnCollisionEnter
                break;
        }
    }

    // --- Các hàm xử lý State ---

    private void HandleIdleState()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            currentState = HelicopterState.StartingRotors;
            idleTimer = 0f; // Reset bộ đếm
        }
    }

     private void HandleStartingRotorsState()
    {
        SpinRotors(); // Tăng tốc cánh quạt
        if(helicopterEffect) helicopterEffect.SetActive(true);
        if (currentRotorSpeed >= minLiftSpeed)
        {
            currentState = HelicopterState.TakeOff;
        }
    }

     private void HandleTakeOffState()
    {
        SpinRotors(); // Duy trì tốc độ cánh quạt
        helicopterEffect.SetActive(true);
        TakeOff(); // Xử lý chuyển động và nghiêng khi cất cánh
        if (transform.position.y > takeoffTransitionHeight)
        {
            currentState = HelicopterState.ReachingAltitude;
        }
    }

    private void HandleReachingAltitudeState()
    {
        SpinRotors(); // Duy trì tốc độ cánh quạt
        helicopterEffect.SetActive(true);
        ReachAltitude(); // Xử lý chuyển động lên độ cao
        if (transform.position.y >= maxHeight)
        {
            currentState = HelicopterState.Flying;
            helicopterEffect.SetActive(false); // Tắt hiệu ứng mặt đất khi bay cao
        }
    }

    private void HandleFlyingState()
    {
        SpinRotors(); // Duy trì tốc độ cánh quạt
        FlyToWaypoint(); // Xử lý bay tới waypoint
    }

    private void HandleHoveringState()
    {
        SpinRotors(); // Duy trì tốc độ cánh quạt
        Hover(); // Xử lý bay tại chỗ (và có thể bắt đầu tấn công)
    }

    private void HandleAttackingState()
    {
        SpinRotors(); // Duy trì tốc độ cánh quạt
        Attacking(); // Xử lý logic tấn công
    }

    private void HandleDeadState()
    {
        if (hasExploded) return; // Nếu đã nổ thì không làm gì thêm
        //transform.Rotate(Vector3.up, maxAngle * Time.deltaTime);
        //transform.Translate(Vector3.down * liftSpeed1 * Time.deltaTime, Space.World);
    }
    // --- Các hàm Di chuyển & Hành động ---

    private void SpinRotors()
    {
        // Không quay bình thường khi đã chết hoặc nổ
        if (currentState == HelicopterState.Dead) return;

        // Tăng tốc độ cánh quạt nếu chưa đạt max
        if (currentRotorSpeed < maxRotorSpeed)
        {
            currentRotorSpeed += rotorAcceleration * Time.deltaTime;
            currentRotorSpeed = Mathf.Min(currentRotorSpeed, maxRotorSpeed);
        }
        RotateRotorTransforms(currentRotorSpeed);
    }
    private void RotateRotorTransforms(float speed)
    {
        float rotationThisFrame = speed * Time.deltaTime;
        mainRotorTransform.Rotate(Vector3.up, rotationThisFrame);
        secondaryRotorTransform.Rotate(Vector3.right, -rotationThisFrame);
    }
    

    private void TakeOff()
    {
        if (!mainBody) return;
        // Lấy góc hiện tại của mainBody
        Vector3 currentEuler = mainBody.transform.eulerAngles;

        // Xác định góc nghiêng mục tiêu (pitch - trục X)
        float targetXAngle = (transform.position.y < takeoffTiltThreshold) ? takeoffPitchAngle : 0f;

        // Lerp góc pitch hiện tại tới góc mục tiêu
        currentEuler.x = Mathf.LerpAngle(currentEuler.x, targetXAngle, turnSpeed * Time.deltaTime);
        mainBody.transform.eulerAngles = currentEuler;

        // Điều chỉnh tốc độ nâng dựa trên độ cao
        float speedMultiplier = (transform.position.y < takeoffTiltThreshold) ? 0.5f : 1f;
        transform.Translate(Vector3.up * liftSpeed * speedMultiplier * Time.deltaTime, Space.World);
    }

    private void ReachAltitude()
    {
         if (!mainBody) return;
        // Dần dần đưa mainBody về góc ngang (pitch = 0, roll = 0)
        Quaternion targetRotation = Quaternion.Euler(0, mainBody.transform.eulerAngles.y, 0);
        mainBody.transform.rotation = Quaternion.Slerp(mainBody.transform.rotation, targetRotation, Time.deltaTime * turnSpeed); // Tăng tốc độ ổn định

        // Tiếp tục nâng độ cao
        transform.Translate(Vector3.up * liftSpeed * Time.deltaTime, Space.World);
    }

    private void FlyToWaypoint()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("Không có waypoints được gán!");
            // Khi không có waypoint, mục tiêu là dừng lại
            if (currentSpeed > stopThreshold)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            }
            else
            {
                currentSpeed = 0f;
                if (currentState != HelicopterState.Hovering)
                    currentState = HelicopterState.Hovering;
            }
            // Di chuyển với tốc độ giảm dần (nếu có) và cố gắng giữ cân bằng
            ApplyMovementAndNeutralTilt();
            return;
        }

        // Nếu đã đến tất cả waypoints thì giảm tốc và chuyển sang trạng thái Hovering
        if (currentWaypointIndex >= waypoints.Count)
        {
            // Mục tiêu là dừng lại
            if (currentSpeed > stopThreshold)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            }
            else
            {
                currentSpeed = 0f;
                if (currentState != HelicopterState.Hovering)
                    currentState = HelicopterState.Hovering;
            }
            // Di chuyển với tốc độ giảm dần (nếu có) và cố gắng giữ cân bằng
            ApplyMovementAndNeutralTilt();
            return;
        }

        // Lấy waypoint hiện tại
        Transform currentWaypoint = waypoints[currentWaypointIndex];
        if (currentWaypoint == null)
        {
            Debug.LogWarning($"Waypoint tại index {currentWaypointIndex} là null!");
            currentWaypointIndex++; // Bỏ qua waypoint lỗi
            return;
        }

        // --- Tính toán hướng ---
        Vector3 directionToWaypoint = currentWaypoint.position - transform.position;
        float distanceToCurrentWaypoint = directionToWaypoint.magnitude;
        Vector3 horizontalDirection = new Vector3(directionToWaypoint.x, 0, directionToWaypoint.z);

        // --- TÍNH TOÁN TỐC ĐỘ MỤC TIÊU VÀ CẬP NHẬT TỐC ĐỘ HIỆN TẠI ---
        float desiredSpeedThisFrame = targetFlySpeed;

        // Nếu là waypoint cuối cùng VÀ đang đến gần nó, thì giảm tốc
        bool isLastWaypoint = (currentWaypointIndex == waypoints.Count - 1);
        if (isLastWaypoint && distanceToCurrentWaypoint < slowingDistance)
        {
            // Giảm tốc độ mục tiêu khi đến gần waypoint cuối
            // Càng gần, tốc độ càng thấp, về 0 tại đích
            desiredSpeedThisFrame = Mathf.Lerp(0f, targetFlySpeed, distanceToCurrentWaypoint / slowingDistance);
            desiredSpeedThisFrame = Mathf.Max(0f, desiredSpeedThisFrame); // Đảm bảo không âm
        }

        // Tăng hoặc giảm tốc độ hiện tại để đạt desiredSpeedThisFrame
        if (currentSpeed < desiredSpeedThisFrame)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeedThisFrame, acceleration * Time.deltaTime);
        }
        else if (currentSpeed > desiredSpeedThisFrame)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeedThisFrame, deceleration * Time.deltaTime);
        }
        currentSpeed = Mathf.Clamp(currentSpeed, 0, targetFlySpeed); // Đảm bảo tốc độ không vượt quá tốc độ bay mục tiêu

        // --- Xoay và Nghiêng ---
        if (horizontalDirection != Vector3.zero && mainBody != null)
        {
            horizontalDirection.Normalize();

            Vector3 mainBodyEuler = mainBody.transform.localEulerAngles;

            // -- Pitch (Chúc/Ngẩng đầu - trục X của mainBody) --
            // (Giữ nguyên logic pitch cũ hoặc bạn có thể điều chỉnh dựa trên currentSpeed nếu muốn)
            float pitchToApply = 0f;
            if (currentSpeed > 0.1f) // Chỉ pitch khi có tốc độ
            {
                // Ví dụ: Pitch tỷ lệ với tốc độ hiện tại / tốc độ bay mục tiêu
                pitchToApply = (currentSpeed / (targetFlySpeed + 0.001f)) * forwardPitchAngle;
            }
            mainBodyEuler.x = Mathf.LerpAngle(mainBodyEuler.x, pitchToApply, turnSpeed * Time.deltaTime);

            // -- Yaw (Xoay hướng - trục Y của transform chính) --
            Quaternion targetYawRotation = Quaternion.LookRotation(horizontalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetYawRotation, turnSpeed * Time.deltaTime);

            // -- Bank (Nghiêng người - trục Z của mainBody) --
            float angleDifference = Vector3.SignedAngle(transform.forward, horizontalDirection, Vector3.up);
            float targetBankAngle = 0f;
            if (currentSpeed > 0.1f) // Chỉ nghiêng khi có tốc độ và đang rẽ
            {
                targetBankAngle = Mathf.Clamp(-angleDifference, -maxBankAngle, maxBankAngle);
            }
            mainBodyEuler.z = Mathf.LerpAngle(mainBodyEuler.z, targetBankAngle, turnSpeed * Time.deltaTime);

            mainBody.transform.localEulerAngles = mainBodyEuler;
        }
        
        // --- Di chuyển tới ---
        // Sử dụng currentSpeed đã được tính toán
        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);

        // --- Điều chỉnh Độ cao (trục Y) ---
        // (Giữ nguyên logic điều chỉnh độ cao)
        float desiredAltitude = currentWaypoint.position.y;
        float currentAltitude = transform.position.y;
        float altitudeDifference = desiredAltitude - currentAltitude;

        float altitudeAdjustmentSpeedFactor = Mathf.Clamp01(Mathf.Abs(altitudeDifference) / (verticalThreshold + 0.1f));
        float currentLiftSpeed = Mathf.Lerp(liftSpeed * 0.1f, liftSpeed, altitudeAdjustmentSpeedFactor);
        float altitudeAdjustment = Mathf.Clamp(altitudeDifference, -currentLiftSpeed * Time.deltaTime, currentLiftSpeed * Time.deltaTime);
        transform.Translate(Vector3.up * altitudeAdjustment, Space.World);

        // --- Kiểm tra Đã tới Waypoint chưa ---
        // (Giữ nguyên logic kiểm tra)
        float horizontalDistance = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(currentWaypoint.position.x, currentWaypoint.position.z)
        );
        float verticalDistance = Mathf.Abs(currentAltitude - desiredAltitude);

        if (horizontalDistance < horizontalThreshold && verticalDistance < verticalThreshold)
        {
            currentWaypointIndex++;
            // Nếu là waypoint cuối và đã đến, logic giảm tốc ở đầu hàm hoặc phần tính desiredSpeedThisFrame
            // sẽ đảm bảo nó dừng lại.
        }
    }

    // Hàm tiện ích mới để áp dụng di chuyển (khi dừng) và đưa về cân bằng
    private void ApplyMovementAndNeutralTilt()
    {
        if (mainBody != null)
        {
            Vector3 mainBodyEuler = mainBody.transform.localEulerAngles;
            mainBodyEuler.z = Mathf.LerpAngle(mainBodyEuler.z, 0f, turnSpeed * Time.deltaTime); // Đưa bank về 0
            mainBody.transform.localEulerAngles = mainBodyEuler;
        }
        // Vẫn di chuyển một chút nếu currentSpeed chưa về 0 hẳn
        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
    }

    private void Hover()
    {
        // delay 1 chút
        if (idleTimer < delayAttack)
        {
            idleTimer += Time.deltaTime;
            return; // Chưa đủ thời gian để tấn công
        }
        idleTimer = 0f; // Reset bộ đếm
        isAttacking = true; // Tạm thời luôn tấn công khi hover xong
        if (isAttacking)
        {
            currentState = HelicopterState.Attacking;
        }
    }

    private void Attacking()
    {
        // Thực hiện logic tấn công
        if (isAttacking && !isAttacked)
        {
            isAttacked = true;
            // Bắt đầu tấn công
            rocketAttack.StartAttack(); // Ví dụ
            Debug.Log("Helicopter Bắt đầu Tấn công!");
        }
        else if (!isAttacking && isAttacked) // Khi nào thì dừng tấn công?
        {
             isAttacked = false;
            // Dừng tấn công
            rocketAttack.StopAttack(); // Ví dụ
             Debug.Log("Helicopter Dừng Tấn công!");
        }
        
    }

    // --- Xử lý Va chạm ---
    private void OnCollisionEnter(Collision collision)
    {
        // Chỉ xử lý va chạm khi đang ở trạng thái Dead và chưa xử lý chạm đất/nổ
        if (currentState != HelicopterState.Dead || hasExploded) // Sử dụng hasExploded để đảm bảo chỉ xử lý 1 lần chạm đất
        {
            return;
        }

        // Kiểm tra layer va chạm
        if (((1 << collision.gameObject.layer) & collisionLayers.value) != 0)
        {
            // Đánh dấu đã xử lý chạm đất để tránh gọi lại logic này
            hasExploded = true; // Đổi tên biến này có thể hợp lý hơn, ví dụ isGrounded, nhưng dùng tạm hasExploded vẫn được về mặt chức năng chặn gọi lại

            // Lấy thông tin va chạm
            ContactPoint contact = collision.contacts[0];
            Vector3 impactPoint = contact.point;
            Vector3 impactNormal = contact.normal;

            // Lấy vận tốc ngay TRƯỚC khi dừng vật lý
            Vector3 impactVelocity = Vector3.zero;
            if (rb != null)
            {
                impactVelocity = rb.velocity;
                //Debug.Log($"Va chạm với vận tốc: {impactVelocity}");
            }

            // --- Kích hoạt nổ/hiệu ứng va chạm ban đầu (TÙY CHỌN) ---
            if (explosionOnImpact && explosionEffect != null)
            {
                explosionEffect.SetActive(true); // Kích hoạt hiệu ứng nổ
                explosionEffect1.SetActive(true); // Kích hoạt hiệu ứng nổ
                
                // Có thể hủy hiệu ứng này sau một thời gian ngắn
                // Destroy(initialExplosion, 3f);
            }

            // --- Dừng vật lý và chuẩn bị trượt ---
            if (rb != null)
            {
                // rb.isKinematic = true; // Ngừng tác động vật lý ngay bây giờ
                 rb.velocity = Vector3.zero;
                 rb.angularVelocity = Vector3.zero; // Ngừng xoay tròn
            }
            
            // --- Bắt đầu Coroutine trượt ---
            StartCoroutine(SlideOnGround(impactVelocity, impactNormal));
            
        }
    }
    IEnumerator SlideOnGround(Vector3 initialVelocity, Vector3 groundNormal)
    {

        // --- Tính toán hướng và tốc độ trượt ban đầu ---
        // Chiếu vận tốc va chạm lên mặt phẳng vuông góc với pháp tuyến mặt đất để có hướng trượt
        Vector3 slideDirection = Vector3.ProjectOnPlane(initialVelocity, groundNormal).normalized;
        float calculatedSlideSpeed = Vector3.ProjectOnPlane(initialVelocity, groundNormal).magnitude * slideInitialSpeedFactor;

        // --- Đặt tốc độ trượt tối thiểu ---
        float minSlideSpeed = 3.0f; // Đặt tốc độ tối thiểu mong muốn (ví dụ: 3.0f) - Có thể đưa ra Inspector
        float currentSlideSpeed = Mathf.Max(calculatedSlideSpeed, minSlideSpeed);

        // Xử lý trường hợp vận tốc gần như bằng 0 hoặc hướng thẳng đứng
        if (slideDirection == Vector3.zero || float.IsNaN(slideDirection.x) || currentSlideSpeed < 0.1f)
        {
            // Không trượt nếu không có vận tốc ngang đáng kể
             Debug.Log("No significant horizontal velocity for sliding.");
             // Có thể gọi hiệu ứng nổ cuối cùng ở đây nếu muốn

             // Gọi disable trực tiếp sau delay ngắn
             if(mainBody) mainBody.gameObject.SetActive(false); // Ẩn thân chính nếu còn hiện
             // --- Chuyển đổi hình ảnh ---
             if(deadStep.Length > 0 && deadStep[0] != null) deadStep[0].SetActive(false); // Ẩn cánh quạt quay
             if(deadStep.Length > 1 && deadStep[1] != null) deadStep[1].SetActive(true);  // Hiện xác máy bay tĩnh
             Invoke(nameof(DisableHelicopter), finalDisableDelay);
             yield break; // Kết thúc coroutine
        }

        float elapsedTime = 0f;
        //Debug.Log($"Starting slide. Direction: {slideDirection}, Initial Speed: {currentSlideSpeed}");

        // --- Vòng lặp trượt ---
        while (elapsedTime < slideDuration && currentSlideSpeed > 0.1f) // Trượt trong thời gian hoặc đến khi dừng
        {
            // Tính toán di chuyển trong frame này
            Vector3 movement = slideDirection * currentSlideSpeed * Time.deltaTime;
            transform.position += movement;

            // Giảm tốc độ do "ma sát"
            currentSlideSpeed -= slideFriction * Time.deltaTime;
            currentSlideSpeed = Mathf.Max(0, currentSlideSpeed); // Đảm bảo tốc độ không âm

            elapsedTime += Time.deltaTime;
            yield return null; // Chờ frame tiếp theo
        }

         //Debug.Log($"Slide finished. Final Speed: {currentSlideSpeed}, Elapsed Time: {elapsedTime}");

        // --- Kết thúc trượt ---
        // Có thể thêm hiệu ứng tóe lửa, âm thanh rít... khi dừng

        // Kích hoạt nổ cuối cùng (TÙY CHỌN)
        explosionAfterSlide = true; // Đánh dấu đã nổ sau khi trượt
        if (explosionAfterSlide && explosionEffect != null)
        {
            SpawnExpolosion();

            // Tạo vụ nổ tại vị trí hiện tại
            //Instantiate(explosionEffect2, transform.position, Quaternion.identity);
        }
        if(mainBody) mainBody.gameObject.SetActive(false); // Ẩn thân chính nếu còn hiện
        
        if(deadStep.Length > 0 && deadStep[0] != null) deadStep[0].SetActive(false); // Ẩn cánh quạt quay
        if(deadStep.Length > 1 && deadStep[1] != null) deadStep[1].SetActive(true);  // Hiện xác máy bay tĩnh
        
        // Lên lịch ẩn GameObject sau một khoảng chờ ngắn
        Invoke(nameof(DisableHelicopter), finalDisableDelay);
    }
    // --- Các hàm Tiện ích ---
    [Header("Cài Đặt Nổ")]
    public float explosionRadius = 5f;
    public int explosionDamage = 50;
    public LayerMask botLayer; // Chọn layer của bot
    private void Explode()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, botLayer);
        //Debug.Log($"🔴 Số collider trong vùng nổ: {hitColliders.Length}");
        HashSet<BotNetwork> affectedBots = new HashSet<BotNetwork>(); // Để tránh gây damage trùng

        foreach (Collider col in hitColliders)
        {
            var botNetwork = col.GetComponentInParent<BotNetwork>(); // Tìm BotNetwork trên object cha
            if (botNetwork != null && !affectedBots.Contains(botNetwork))
            {
                affectedBots.Add(botNetwork); // Thêm bot vào danh sách (tránh trùng lặp)

                var damageInfo = new DamageInfo()
                {
                    damageType = DamageType.Normal,
                    damage = explosionDamage,
                };

                botNetwork.TakeDamage(damageInfo);
                //Debug.Log($"💥 Gây {explosionDamage} damage lên bot: {botNetwork.gameObject.name}");
            }
        }
    }
    private void OnDrawGizmos()
    {
        // Vẽ phạm vi nổ trong Scene View
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
    // Vô hiệu hóa GameObject (để trả về pool hoặc hủy)
    private void DisableHelicopter()
    {

        // Hoặc trả về pool nếu bạn dùng pooling system
        ObjectPool.Instance.PushToPool(this, gameObject);
        
    }

    // Reset lại trạng thái của Helicopter (quan trọng cho pooling)
    private void ResetState()
    {
        // Đặt lại vị trí/góc quay ban đầu (nếu cần)
        // transform.position = initialPosition; // Cần lưu vị trí ban đầu nếu muốn reset
        transform.rotation = Quaternion.Euler(0f, setAngleY, 0f);
        if(mainBody) mainBody.transform.localRotation = Quaternion.identity; // Reset góc nghiêng thân
        explosionEffect.SetActive(false);
        // Đặt lại trạng thái hình ảnh
        brakeDoor[0].SetActive(true);
        brakeDoor[1].SetActive(false);
        deadStep[0].SetActive(false);
        deadStep[1].SetActive(false);
        if(mainBody) mainBody.gameObject.SetActive(true);
        if(helicopterEffect) helicopterEffect.SetActive(false);

        // Đặt lại các cờ và biến trạng thái
        isDead = false;
        currentState = HelicopterState.Idle;
        currentWaypointIndex = 0;
        isAttacking = false;
        isAttacked = false;
        currentRotorSpeed = 0f;
        idleTimer = 0f;
        hasExploded = false;
        isEndGame = false; // Reset cờ endgame

        // Đặt lại trạng thái Rigidbody
        if (rb != null)
        {
             rb.isKinematic = true;
             rb.useGravity = false;
             rb.velocity = Vector3.zero;
             rb.angularVelocity = Vector3.zero;
        }

        // Đặt lại parent cho deadStep[1] nếu trước đó đã tách ra (nếu có logic đó)
        // if(deadStep.Length > 1 && deadStep[1] != null && mainBody != null)
        // {
        //     deadStep[1].transform.SetParent(mainBody.transform);
        //     deadStep[1].transform.localPosition = Vector3.zero;
        //     deadStep[1].transform.localRotation = Quaternion.identity;
        // }


        CancelInvoke(nameof(DisableHelicopter)); // Hủy invoke DisableHelicopter nếu đang chờ
    }

    // --- Triển khai Interface IPoolObject (Nếu có) ---
    public GameObject Prefab { get; set; }
    public void Init() // Được gọi khi lấy ra từ pool
    {
        ResetState(); // Reset trạng thái khi được tái sử dụng
    }
    public void OnPushToPool() // Được gọi khi trả về pool
    {
        // Dừng các hoạt động còn dang dở
        if (audioSource != null) audioSource.Stop();
        CancelInvoke(); // Hủy tất cả các Invoke
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(HelicopterMi28Controller))]
    public class HelicopterControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Vẽ giao diện Inspector mặc định
            DrawDefaultInspector();

            // Lấy đối tượng HelicopterController đang được chọn
            HelicopterMi28Controller helicopterController = (HelicopterMi28Controller)target;

            // Thêm một nút bấm vào Inspector
            if (GUILayout.Button("Test Death Sequence (Chạy)"))
            {
                // Chỉ chạy khi game đang Play
                if (Application.isPlaying)
                {
                    helicopterController.OnDeath();
                }
                else
                {
                    Debug.LogWarning("Vào Play Mode để kiểm tra chuỗi hành động khi chết.");
                }
            }
        }
    }
    #endif
}