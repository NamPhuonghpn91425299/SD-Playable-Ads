using UnityEngine;

public class F15TrackingMovement : MonoBehaviour
{
    [Header("Hướng Máy Bay")]
    public float trackingSpeed = 3f;

    [Header("Đảo Góc")]
    public float maxSwayAngle = 1f;    // Góc đảo tối đa
    public float swaySmoothing = 5f;    // Độ mịn chuyển động
    public float Smoothing = 1f;    // Độ mịn chuyển động
    public float minSwayInterval = 1.5f;  // Khoảng thời gian ngắn nhất giữa các lần đảo
    public float maxSwayInterval = 3f;  // Khoảng thời gian dài nhất giữa các lần đảo

    private Transform playerTransform;
    private float targetSwayAngle = 0f;
    private float currentSwayAngle = 0f;
    private float swayTimer = 0f;
    private float nextSwayInterval;
    //[Header("Di chuyển quanh điểm đặt")]
    //[SerializeField] private Transform centerPoint;  // Điểm trung tâm để quay quanh
    //[SerializeField] private BotNetwork botNetwork;  // Điểm trung tâm để quay quanh
    //[SerializeField] private float radius = 3f;      // Bán kính quỹ đạo
    //[SerializeField] private float angularSpeed = 30f; // Tốc độ góc (độ/giây)
    //private float currentAngle = 0f;
    //private int rotationDirection;   // Hướng quay (1: thuận chiều kim đồng hồ, -1: ngược chiều)
    //[SerializeField] private float minSpeed = 20f;
    //[SerializeField] private float maxSpeed = 50f;
    //[SerializeField] private float minRadius = 3f;
    //[SerializeField] private float maxRadius = 8f;
    private void Awake()
    {

    }
    private void OnEnable()
    {
        playerTransform = LocalPlayer.Instance.GetTranformPlayer();
        ResetSwayInterval();

    }
    void Start()
    {
        //centerPoint = botNetwork.Path.WayPoints[1];
        //currentAngle = Random.Range(0f, 360f); // Góc khởi đầu ngẫu nhiên
        //angularSpeed = Random.Range(minSpeed, maxSpeed); // Tốc độ góc ngẫu nhiên
        //radius = Random.Range(minRadius, maxRadius); // Bán kính ngẫu nhiên
        //rotationDirection = Random.Range(0, 2) == 0 ? 1 : -1; // Random chiều quay

    }

    void Update()
    {
        //MoveOnRadius();
    }

    private void LateUpdate()
    {
        LockAtTagert();
        NextSwayCount();
        // Làm mượt chuyển động góc đảo
        currentSwayAngle = Mathf.LerpAngle(currentSwayAngle, targetSwayAngle, Time.deltaTime * swaySmoothing);
        // Áp dụng góc đảo
        transform.rotation *= Quaternion.Euler(0, 0, currentSwayAngle);
        //MoveOnRadius();
    }
    void ResetSwayInterval()
    {
        // Đặt lại thời gian giữa các lần đảo
        nextSwayInterval = Random.Range(minSwayInterval, maxSwayInterval);
        swayTimer = 0f;
    }

    private void LockAtTagert()
    {
        // Hướng về người chơi mượt mà
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        //Vector3 directionToPlayer = transform.forward;
        directionToPlayer.Normalize();
        var up = Vector3.Cross(directionToPlayer, playerTransform.right);
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, trackingSpeed * Time.deltaTime);
    }

    private void NextSwayCount()
    {

        // Đếm thời gian để đảo
        swayTimer += Time.deltaTime;

        // Kiểm tra đến thời điểm đảo
        if (swayTimer >= nextSwayInterval)
        {
            // Chọn góc đảo mới ngẫu nhiên
            targetSwayAngle = Random.Range(-maxSwayAngle, maxSwayAngle);
            //rotationDirection *= -1; // Đảo chiều quay
            ResetSwayInterval();
        }
    }

    //private void MoveOnRadius()
    //{
    //    // Tăng góc dựa trên tốc độ góc và chiều quay
    //    currentAngle += angularSpeed * rotationDirection * Time.deltaTime;

    //    // Giới hạn góc trong khoảng 0 - 360
    //    if (currentAngle >= 360f) currentAngle -= 360f;
    //    if (currentAngle < 0f) currentAngle += 360f;

    //    // Tính vị trí mới quanh trung tâm (trong mặt phẳng XY)
    //    float radians = currentAngle * Mathf.Deg2Rad;
    //    float targetX = centerPoint.position.x + Mathf.Cos(radians) * radius;
    //    float targetY = centerPoint.position.y + Mathf.Sin(radians) * radius;

    //    // Cập nhật vị trí mượt mà
    //    Vector3 targetPosition = new Vector3(targetX, targetY, transform.position.z);
    //    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * Smoothing);

    //    // Vẽ đường từ centerPoint đến vị trí mới
    //    Debug.DrawLine(centerPoint.position, transform.position, Color.red);
    //}


}




