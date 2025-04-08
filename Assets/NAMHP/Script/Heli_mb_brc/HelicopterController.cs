using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class HelicopterController : MonoBehaviour,IPoolObject
{
    // Các thành phần của trực thăng
    [SerializeField] private BotNetwork botNetwork;
    [SerializeField] private Transform mainRotorTransform;
    [SerializeField] private Transform secondaryRotorTransform;
    [SerializeField] private Transform mainBody;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject helicopterEffect;
    [SerializeField] private GameObject[] deadStep;
    [SerializeField] private HelicopterRocketAttack rocketAttack; 
    public bool isAttacking;
    public bool isDead;
    // Các biến có thể tùy chỉnh
    [Header("Rotor Settings")]
    [SerializeField] public float maxRotorSpeed = 3000f;
    [SerializeField] private float rotorAcceleration = 200f;
    [SerializeField] private float minLiftSpeed = 1500f;
    
    [Header("Movement Settings")]
    [SerializeField] public float maxHeight = 20f;
    [SerializeField] private float liftSpeed = 5f;
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float takeoffPitchAngle = -15f;
    [SerializeField] private float forwardPitchAngle = 10f;
    [SerializeField] private float turnSpeed = 1f;
    // Ngưỡng kiểm tra cho khoảng cách ngang và độ cao
    [SerializeField] private float horizontalThreshold = 6f;
    [SerializeField] private float verticalThreshold = 4f;
    [SerializeField] private float idleDuration = 2f; // Thời gian idle tối thiểu
    [SerializeField] private float deadPositionY = 3f; // Vị trí Y để xác định trạng thái chết
    // Các biến tùy chỉnh giá trị chuyển trạng thái
    [Header("TakeOff & Altitude Settings")]
    [SerializeField] private float takeoffTransitionHeight = 10f; // Độ cao chuyển từ TakeOff sang ReachingAltitude
    [SerializeField] private float takeoffTiltThreshold = 28f;    // Ngưỡng độ cao để chuyển đổi góc trong TakeOff
    
    [Header("Banking Settings")]
    [SerializeField] private float maxBankAngle = 20f; // Góc nghiêng tối đa (trục Z)
    [SerializeField] private float maxAngle = 100f; // Góc nghiêng tối đa (trục Z)
    [SerializeField] private float setAngleY = 45f;
    
    // Waypoints và theo dõi trạng thái
    [SerializeField] public List<Transform> waypoints;
    public int currentWaypointIndex = 0;
    
    // Các biến theo dõi trạng thái
    public float currentRotorSpeed = 0f;
    private float idleTimer = 0f;
    private bool isAttacked;
    public HelicopterState currentState = HelicopterState.Idle;

    // Enum để quản lý trạng thái
    public enum HelicopterState
    {
        Idle,
        StartingRotors,
        TakeOff,
        ReachingAltitude,
        Flying,
        Hovering,
        Attacking,
        Dead
    }

    private void Awake()
    {
        //botNetwork = GetComponent<BotNetwork>();
    }

    private void OnEnable()
    {

        botNetwork.OnBotDead += OnDeath;
    }

    private void Start()
    {
        playerTransform = LocalPlayer.Instance.GetTranformPlayer();
        waypoints = botNetwork.Path.AttackWayPoints;
        transform.rotation = Quaternion.Euler(0f, setAngleY, 0f);
    }

    private void OnDisable()
    {
        botNetwork.OnBotDead -= OnDeath;
    }

    public void OnDeath()
    {
        isDead = true;
        isAttacking = false;
        rocketAttack.StopAttack();
        currentState = HelicopterState.Dead;
        BotDeath.Instance.GetBotDeath();
    }
    // private void Update()
    // {
    //     // Nếu trực thăng đã chết, không xử lý gì thêm
    //     if (isDead)
    //     {
    //         currentState = HelicopterState.Dead;
    //     }
    //     // Xử lý trạng thái hiện tại
    //     switch (currentState)
    //     {
    //         case HelicopterState.Idle:
    //             idleTimer += Time.deltaTime;
    //             if (idleTimer >= idleDuration)
    //             {
    //                 currentState = HelicopterState.StartingRotors;
    //                 idleTimer = 0f; // Reset lại timer
    //             }
    //             break;
    //         
    //         case HelicopterState.StartingRotors:
    //             // Tăng tốc cánh quạt dần dần
    //             SpinRotors();
    //             helicopterEffect.SetActive(true);
    //             // Kiểm tra nếu đạt vận tốc cất cánh
    //             if (currentRotorSpeed >= minLiftSpeed)
    //             {
    //                 currentState = HelicopterState.TakeOff;
    //             }
    //             break;
    //             
    //         case HelicopterState.TakeOff:
    //             // Duy trì tốc độ cánh quạt
    //             SpinRotors();
    //             helicopterEffect.SetActive(true);    
    //             // Ngẩng đầu lên và bắt đầu cất cánh
    //             TakeOff();
    //             
    //             // Kiểm tra nếu đã cất cánh thành công
    //             if (transform.position.y > takeoffTransitionHeight)
    //             {
    //                 currentState = HelicopterState.ReachingAltitude;
    //             }
    //             break;
    //             
    //         case HelicopterState.ReachingAltitude:
    //             // Duy trì tốc độ cánh quạt
    //             SpinRotors();
    //             helicopterEffect.SetActive(true);
    //             // Bay lên đến độ cao mục tiêu
    //             ReachAltitude();
    //             
    //             // Kiểm tra nếu đạt độ cao mục tiêu
    //             if (transform.position.y >= maxHeight)
    //             {
    //                 currentState = HelicopterState.Flying;
    //             }
    //             break;
    //             
    //         case HelicopterState.Flying:
    //             // Duy trì tốc độ cánh quạt
    //             SpinRotors();
    //             helicopterEffect.SetActive(false);
    //             // Bay đến waypoint
    //             FlyToWaypoint();
    //             break;
    //             
    //         case HelicopterState.Hovering:
    //             // Duy trì tốc độ cánh quạt và hover
    //             SpinRotors();
    //             Hover();
    //             break;
    //         case HelicopterState.Attacking:
    //             // Duy trì tốc độ cánh quạt và tấn công
    //             SpinRotors();
    //             // Attack logic here
    //             Attacking();
    //             break;
    //         case HelicopterState.Dead:
    //             // Duy trì tốc độ cánh quạt và xử lý trạng thái chết
    //             SpinRotors();
    //             HandleDeadState();
    //             break;
    //     }
    // }
    
    // Xử lý quay cánh quạt
    private void Update()
    {
        // Nếu trực thăng đã chết, chuyển ngay về trạng thái Dead
        // if (isDead)
        // {
        //     currentState = HelicopterState.Dead;
        // }
        ChangeState();

    }
    private void ChangeState()
    {
        // Xử lý theo từng trạng thái riêng biệt
        switch (currentState)
        {
            case HelicopterState.Idle:
                HandleIdle();
                break;

            case HelicopterState.StartingRotors:
                HandleStartingRotors();
                break;

            case HelicopterState.TakeOff:
                HandleTakeOff();
                break;

            case HelicopterState.ReachingAltitude:
                HandleReachingAltitude();
                break;

            case HelicopterState.Flying:
                HandleFlying();
                break;

            case HelicopterState.Hovering:
                HandleHovering();
                break;

            case HelicopterState.Attacking:
                HandleAttacking();
                break;

            case HelicopterState.Dead:
                HandleDeadState();
                break;
        }
    }
    private void HandleIdle()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            currentState = HelicopterState.StartingRotors;
            idleTimer = 0f; // Reset lại timer
        }
    }

    private void HandleStartingRotors()
    {
        SpinRotors();
        helicopterEffect.SetActive(true);
        helicopterEffect.transform.SetParent(null);
        if (currentRotorSpeed >= minLiftSpeed)
        {
            currentState = HelicopterState.TakeOff;
        }
    }

    private void HandleTakeOff()
    {
        SpinRotors();
        helicopterEffect.SetActive(true);
        TakeOff();

        if (transform.position.y > takeoffTransitionHeight)
        {
            currentState = HelicopterState.ReachingAltitude;
        }
    }

    private void HandleReachingAltitude()
    {
        SpinRotors();
        helicopterEffect.SetActive(true);
        ReachAltitude();

        if (transform.position.y >= maxHeight)
        {
            currentState = HelicopterState.Flying;
        }
    }

    private void HandleFlying()
    {
        SpinRotors();
        helicopterEffect.SetActive(false);
        helicopterEffect.transform.SetParent(this.transform);
        FlyToWaypoint();
    }

    private void HandleHovering()
    {
        SpinRotors();
        Hover();
    }

    private void HandleAttacking()
    {
        SpinRotors();
        // Thực hiện các logic tấn công
        Attacking();
    }

    private void SpinRotors()
    {
        // Tăng tốc độ cánh quạt dần dần
        if (currentRotorSpeed < maxRotorSpeed)
        {
            currentRotorSpeed += rotorAcceleration * Time.deltaTime;
            currentRotorSpeed = Mathf.Min(currentRotorSpeed, maxRotorSpeed);
        }

        // Quay cánh quạt với tốc độ hiện tại
        float rotationThisFrame = currentRotorSpeed * Time.deltaTime;
        mainRotorTransform.Rotate(Vector3.up, rotationThisFrame);
        secondaryRotorTransform.Rotate(Vector3.up, -rotationThisFrame); // Quay ngược
    }
    
    // Xử lý cất cánh
    private void TakeOff()
    {
        // Lấy góc hiện tại của mainBody
        Vector3 currentEuler = mainBody.transform.eulerAngles;
    
        // Nếu dưới ngưỡng takeoffTiltThreshold, nâng máy bay với góc takeoffPitchAngle
        // Sau khi đạt ngưỡng, dần trả lại góc về 0 (ngang)
        float targetXAngle = (transform.position.y < takeoffTiltThreshold) ? takeoffPitchAngle : 0f;
    
        // Cập nhật góc pitch (trục X) một cách mượt mà
        currentEuler.x = Mathf.LerpAngle(currentEuler.x, targetXAngle, turnSpeed * Time.deltaTime);
        mainBody.transform.eulerAngles = currentEuler;
    
        // Điều chỉnh tốc độ nâng: ở giai đoạn đầu nâng nhẹ, sau đó nâng với tốc độ đầy đủ
        float speedMultiplier = (transform.position.y < takeoffTiltThreshold) ? 0.5f : 1f;
        transform.Translate(Vector3.up * liftSpeed * speedMultiplier * Time.deltaTime, Space.World);
    }
    
    // Bay lên độ cao mục tiêu
    private void ReachAltitude()
    {
        // Dần dần đưa mainBody về góc ngang
        Quaternion targetRotation = Quaternion.Euler(0, mainBody.transform.eulerAngles.y, 0);
        mainBody.transform.rotation = Quaternion.Slerp(mainBody.transform.rotation, targetRotation, Time.deltaTime);
        
        // Tiếp tục nâng cao
        transform.Translate(Vector3.up * liftSpeed * Time.deltaTime, Space.World);
    }
    private void FlyToWaypoint()
    {
        // Nếu đã đến tất cả waypoints thì chuyển sang trạng thái Hovering
        if (currentWaypointIndex >= waypoints.Count)
        {
            currentState = HelicopterState.Hovering;
            return;
        }

        // Lấy waypoint hiện tại
        Transform currentWaypoint = waypoints[currentWaypointIndex];

        // Tính toán hướng di chuyển theo mặt phẳng ngang (x,z)
        Vector3 directionToWaypoint = currentWaypoint.position - transform.position;
        Vector3 horizontalDirection = new Vector3(directionToWaypoint.x, 0, directionToWaypoint.z);

        if (horizontalDirection != Vector3.zero)
        {
            // Sử dụng LerpAngle cho trục x để tạo chuyển động mượt
            Vector3 mainBodyEuler = mainBody.transform.eulerAngles;
            mainBodyEuler.x = Mathf.LerpAngle(mainBodyEuler.x, forwardPitchAngle, turnSpeed * Time.deltaTime);
            mainBody.transform.eulerAngles = mainBodyEuler;
        
            // Tính toán hướng xoay mục tiêu (yaw) theo mặt phẳng ngang
            Quaternion targetYawRotation = Quaternion.LookRotation(horizontalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetYawRotation, turnSpeed * Time.deltaTime);

            // Tính góc chênh lệch giữa hướng hiện tại và hướng mục tiêu
            float angleDifference = Vector3.SignedAngle(transform.forward, horizontalDirection, Vector3.up);

            // Tính góc banking mong muốn: nếu rẽ thì trực thăng sẽ nghiêng theo hướng rẽ
            // Dấu âm để nghiêng về phía bên ngoài của vòng cung
            float targetBankAngle = Mathf.Clamp(-angleDifference, -maxBankAngle, maxBankAngle);

            // Điều chỉnh trục Z (bank)
            mainBodyEuler = mainBody.transform.eulerAngles;
            mainBodyEuler.z = Mathf.LerpAngle(mainBodyEuler.z, targetBankAngle, turnSpeed * Time.deltaTime);
            mainBody.transform.eulerAngles = mainBodyEuler;
        }

        // Di chuyển về phía trước theo mặt phẳng ngang (local forward)
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);
        
        // --- PHẦN ĐIỀU CHỈNH TRỤC Y (điều chỉnh độ cao) ---
        float desiredAltitude = currentWaypoint.position.y;
        float currentAltitude = transform.position.y;
        float altitudeDifference = desiredAltitude - currentAltitude;
        float altitudeAdjustmentSpeed = liftSpeed; 
        float altitudeAdjustment = Mathf.Clamp(altitudeDifference, -altitudeAdjustmentSpeed * Time.deltaTime, altitudeAdjustmentSpeed * Time.deltaTime);
        transform.Translate(Vector3.up * altitudeAdjustment, Space.World);
        
        // --- KIỂM TRA ĐIỂM ĐẾN ---
        Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 waypointPos2D = new Vector2(currentWaypoint.position.x, currentWaypoint.position.z);
        float horizontalDistance = Vector2.Distance(currentPos2D, waypointPos2D);
        float verticalDistance = Mathf.Abs(currentAltitude - desiredAltitude);
        
        if (horizontalDistance < horizontalThreshold && verticalDistance < verticalThreshold)
        {
            currentWaypointIndex++;
        }

    }

    private void Hover()
    {
        isAttacking = true;
        if (isAttacking)
        {
            currentState = HelicopterState.Attacking;
            // Nếu đang tấn công, không cần hover
            return;
        }
        // // Lấy vị trí của player (giả sử player có tag "Player")
        //
        // if (playerTransform == null) return;
        //
        // // Tính toán hướng di chuyển theo mặt phẳng ngang (x,z)
        // Vector3 directionToPlayer = playerTransform.transform.position - transform.position;
        // Vector3 horizontalDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
        //
        // if (horizontalDirection != Vector3.zero)
        // {
        //     // Tính toán hướng xoay mục tiêu (yaw) theo mặt phẳng ngang
        //     Quaternion targetYawRotation = Quaternion.LookRotation(horizontalDirection);
        //     transform.rotation = Quaternion.Slerp(transform.rotation, targetYawRotation, turnSpeed * Time.deltaTime);
        //
        //     // Tính góc chênh lệch giữa hướng hiện tại và hướng mục tiêu
        //     float angleDifference = Vector3.SignedAngle(transform.forward, horizontalDirection, Vector3.up);
        //
        //     // Tính góc banking mong muốn: nếu rẽ thì trực thăng sẽ nghiêng theo hướng rẽ
        //     // Dấu âm để nghiêng về phía bên ngoài của vòng cung
        //     float targetBankAngle = Mathf.Clamp(-angleDifference, -maxBankAngle, maxBankAngle);
        //
        //     // Lấy giá trị Euler hiện tại của mainBody để điều chỉnh trục Z (bank)
        //     Vector3 mainBodyEuler = mainBody.transform.eulerAngles;
        //     mainBodyEuler.z = Mathf.LerpAngle(mainBodyEuler.z, targetBankAngle, turnSpeed * Time.deltaTime);
        //     mainBody.transform.eulerAngles = mainBodyEuler;
        // }
        //
        // // Giữ độ cao bằng cách thêm dao động nhỏ để giống thật
        // float heightOffset = Mathf.Sin(Time.time) * 0.1f;
        // transform.Translate(Vector3.up * heightOffset * Time.deltaTime, Space.World);
    }
    

    private void Attacking()
    {
        if (isAttacking && !isAttacked)
        {
            isAttacked = true;
            // Bắt đầu tấn công
            rocketAttack.StartAttack();
        }
        else if (!isAttacking && isAttacked)
        {
            isAttacked = false;
            // Dừng tấn công
            rocketAttack.StopAttack();
        }
    }
    private void HandleDeadState()
    {

        helicopterEffect.SetActive(false);
        deadStep[1].SetActive(true);
        deadStep[1].transform.SetParent(null);
        mainBody.gameObject.SetActive(false);
        Invoke(nameof(DisableHelicopter), 2f);
        // // Thêm logic cho trạng thái chết, ví dụ: phát nổ, rơi xuống, v.v.
        // // Ví dụ: làm cho trực thăng rơi xuống
        // transform.Translate(Vector3.down * liftSpeed * Time.deltaTime, Space.World);
        // // thêm hiệu ứng rơi xoay vòng vòng
        // // rơi đến khi chạm đất thì dừng rơi
        // if (transform.position.y <= deadPositionY)
        // {
        //     // Dừng xoay
        //     deadStep[0].SetActive(false);
        //     deadStep[1].SetActive(true);
        //     mainBody.gameObject.SetActive(false);
        //     transform.position = new Vector3(transform.position.x, deadPositionY, transform.position.z);
        //     // tắt gameobject sau 2s
        //     Invoke(nameof(DisableHelicopter), 2f);
        //     
        // }
        // else
        // {
        //     transform.Rotate(Vector3.up, maxAngle * Time.deltaTime);
        // }
        // // Có thể thêm hiệu ứng phát nổ hoặc âm thanh
        // // Example: Instantiate(explosionEffect, transform.position, transform.rotation);
    }
    private void DisableHelicopter()
    {
        deadStep[1].transform.SetParent(this.transform);
        ObjectPool.Instance.PushToPool(this, gameObject);
        isDead = false;
        currentState = HelicopterState.Idle;
        currentWaypointIndex = 0;
        isAttacking = false;
        isAttacked = false;
        currentRotorSpeed = 0f;
        idleTimer = 0f;
        mainBody.gameObject.SetActive(true);
        deadStep[1].SetActive(false);
        deadStep[0].SetActive(false);
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(HelicopterController))]
    public class HelicopterControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            HelicopterController helicopterController = (HelicopterController)target;

            if (GUILayout.Button("Bot Dead"))
            {
                helicopterController.OnDeath();
            }
            if (GUILayout.Button("Stop Attack"))
            {
                helicopterController.rocketAttack.StopAttack();
            }
        }
    }
#endif
    public GameObject Prefab { get; set; }
    public void Init()
    {
        
    }

    public void OnPushToPool()
    {
        
    }
}
