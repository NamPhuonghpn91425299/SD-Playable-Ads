using System;
using UnityEngine;
public class HelicopterOrbit : MonoBehaviour
{
    [Header("Thiết Lập Quỹ Đạo")]
    [SerializeField, Tooltip("Tốc độ thay đổi bán kính quỹ đạo khi chuyển tiếp hoặc điều chỉnh.")]
    private float radiusChangeSpeed = 2f;
    [SerializeField, Tooltip("Tốc độ xoay của máy bay để hướng về người chơi.")]
    private float turnSpeed = 2f;
    [SerializeField, Tooltip("Góc nghiêng tối đa của máy bay khi bay trên quỹ đạo.")]
    private float rollAngle = 15f;
    [SerializeField, Tooltip("Thời gian (giây) để chuyển tiếp mượt mà từ waypoint cuối cùng vào quỹ đạo.")]
    private float transitionTime = 3f;

    [Header("Tham Chiếu")]
    [SerializeField, Tooltip("Transform của người chơi (mục tiêu để nhìn vào).")]
    private Transform player;
    [SerializeField, Tooltip("Transform của điểm làm tâm quỹ đạo.")]
    private Transform orbitCenter;
    [SerializeField, Tooltip("Tham chiếu đến HelicopterController để lấy chỉ số waypoint hiện tại.")]
    private HelicopterController helicopterController; // Cần được gán từ Inspector hoặc lấy trong Awake/Start

    [Header("Cài Đặt Debug")]
    [SerializeField, Tooltip("Số đoạn vẽ để tạo hình tròn debug.")]
    private int circleSegments = 100;
    [SerializeField, Tooltip("Màu của vòng tròn quỹ đạo debug.")]
    private Color circleColor = Color.yellow;
    [SerializeField, Tooltip("Màu của đường nối tâm và máy bay debug.")]
    private Color markerLineColor = Color.green;
    [SerializeField, Tooltip("Màu của tia chỉ hướng phía trước debug.")]
    private Color forwardRayColor = Color.red;

    #region Private State Variables (Biến trạng thái nội bộ)
    
    public bool isActiveFly; // Cho phép điều khiển từ bên ngoài

    // Biến lưu trữ trạng thái
    private float currentOrbitRadius; // Bán kính quỹ đạo hiện tại (thay đổi trong quá trình chuyển tiếp)
    private float targetOrbitRadius; // Bán kính quỹ đạo mục tiêu (tính toán một lần)
    private float radiusSmoothVelocity = 0f; // Biến nội bộ cho hàm SmoothDamp (bán kính)
    private bool inFinalOrbit = false; // Cờ báo đã hoàn thành waypoint và đang trong chế độ quỹ đạo
    private Vector3 lastWaypointPosition; // Vị trí của máy bay tại waypoint cuối cùng (dùng cho Lerp chuyển tiếp)
    private float transitionStartTime; // Thời điểm bắt đầu chuyển tiếp vào quỹ đạo
    private float transitionProgress = 0f; // Tiến trình chuyển tiếp (0 đến 1)
    private int waypointsCount; // Số lượng waypoint (cache lại để tránh truy cập .Length liên tục)
    private bool isDead = false; // Biến trạng thái để kiểm tra xem máy bay có đang chết hay không
    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Lấy tham chiếu nếu chưa được gán từ Inspector (tùy chọn)
        if (helicopterController == null)
        {
            helicopterController = GetComponent<HelicopterController>();
            if (helicopterController == null)
            {
                Debug.LogError("HelicopterOrbit: Không tìm thấy HelicopterController!", this);
                enabled = false; // Vô hiệu hóa script nếu thiếu thành phần quan trọng
                return;
            }
        }


    }

    private void Start()
    {
        waypointsCount = helicopterController.waypoints.Count; // Lưu số lượng waypoint để dùng trong logic
        player = LocalPlayer.Instance.GetTranformPlayer(); // Lấy tham chiếu đến người chơi từ singleton
        orbitCenter = LocalPlayer.Instance.GetTranCenter();
    }

    private void OnEnable()
    {
        // Khởi tạo trạng thái ban đầu
        inFinalOrbit = false;
        transitionProgress = 0f;
        // Có thể đặt currentOrbitRadius ban đầu ở đây nếu cần giá trị mặc định sớm
        // currentOrbitRadius = 10f; // Ví dụ
    }

    private void Update()
    {
        // Kiểm tra các tham chiếu cần thiết trước khi thực hiện logic
        if (!ValidateReferences()) return;

        // Logic chính: hoặc di chuyển theo waypoint hoặc bay quỹ đạo
        if (!inFinalOrbit)
        {
            HandleWaypointMovement();
        }
        // Chỉ bay quỹ đạo nếu đã hoàn thành waypoint VÀ được phép bay (isActiveFly)
        else if (inFinalOrbit && isActiveFly)
        {
            // Cache vị trí tâm và người chơi để dùng trong frame này
            Vector3 centerPos = orbitCenter.position;
            Vector3 playerPos = player.position;
            HandleOrbitMovement(centerPos, playerPos);
            DrawDebugInfo(centerPos, playerPos, transform.position, currentOrbitRadius);
        }
        // Nếu đang trong quỹ đạo nhưng không active, có thể thêm logic dừng hoặc giữ nguyên vị trí ở đây (tùy yêu cầu)
        // else if (inFinalOrbit && !isActiveFly) { /* Logic khi dừng bay quỹ đạo */ }
    }

    #endregion

    #region Core Logic Methods (Phương thức xử lý logic chính)

    /// <summary>
    /// Kiểm tra xem các tham chiếu cần thiết đã được gán hay chưa.
    /// </summary>
    /// <returns>True nếu hợp lệ, False nếu thiếu.</returns>
    private bool ValidateReferences()
    {
        if (player == null)
        {
            Debug.LogWarning("HelicopterOrbit: Tham chiếu 'player' chưa được gán.", this);
            return false;
        }
        if (orbitCenter == null)
        {
            Debug.LogWarning("HelicopterOrbit: Tham chiếu 'orbitCenter' chưa được gán.", this);
            return false;
        }
        if (helicopterController.isDead)
        {
            Debug.LogWarning("HelicopterOrbit: Máy bay đã chết, không thể bay quỹ đạo.", this);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Xử lý logic khi máy bay đang di chuyển theo waypoints (chủ yếu là kiểm tra khi nào xong).
    /// Việc di chuyển thực tế được giả định là do HelicopterController xử lý.
    /// </summary>
    private void HandleWaypointMovement()
    {
        // Lấy chỉ số waypoint hiện tại từ HelicopterController
        int currentWaypointIndexFromController = helicopterController.currentWaypointIndex;
        
        // Nếu chỉ số waypoint lấy từ controller >= tổng số waypoint, tức là đã đi hết
        if (currentWaypointIndexFromController >= waypointsCount)
        {
            // Khởi tạo chế độ bay quỹ đạo
            InitializeOrbitMode();
        }
        // Ngược lại, vẫn đang trên đường đi waypoint (không cần làm gì trong script này)
    }

    /// <summary>
    /// Khởi tạo các giá trị cần thiết khi bắt đầu chuyển sang chế độ bay quỹ đạo.
    /// </summary>
    private void InitializeOrbitMode()
    {
        if (inFinalOrbit) return; // Chỉ khởi tạo một lần

        inFinalOrbit = true;
        lastWaypointPosition = transform.position; // Lưu vị trí hiện tại làm điểm bắt đầu chuyển tiếp
        transitionStartTime = Time.time; // Ghi lại thời điểm bắt đầu chuyển tiếp
        transitionProgress = 0f; // Reset tiến trình chuyển tiếp

        // Tính toán bán kính quỹ đạo mục tiêu dựa trên khoảng cách từ tâm đến waypoint cuối cùng
        // Nếu không có waypoint, dùng khoảng cách hiện tại hoặc một giá trị mặc định
        if (waypointsCount > 0 && helicopterController.waypoints[waypointsCount - 1] != null)
        {
            targetOrbitRadius = Vector3.Distance(orbitCenter.position, helicopterController.waypoints[waypointsCount - 1].position);
        }
        else
        {
            // Nếu không có waypoint, dùng khoảng cách hiện tại từ tâm hoặc giá trị mặc định
            targetOrbitRadius = Vector3.Distance(orbitCenter.position, transform.position);
            // targetOrbitRadius = 15f; // Hoặc một giá trị mặc định an toàn
        }

        // Đặt bán kính hiện tại bằng bán kính của điểm bắt đầu chuyển tiếp
        currentOrbitRadius = Vector3.Distance(orbitCenter.position, lastWaypointPosition);
        radiusSmoothVelocity = 0f; // Reset biến nội bộ của SmoothDamp

        //Debug.Log("Helicopter entering orbit mode. Target Radius: " + targetOrbitRadius);
    }

    /// <summary>
    /// Xử lý toàn bộ logic chuyển động và xoay khi đang trong chế độ bay quỹ đạo.
    /// </summary>
    /// <param name="centerPos">Vị trí tâm quỹ đạo (đã cache).</param>
    /// <param name="playerPos">Vị trí người chơi (đã cache).</param>
    private void HandleOrbitMovement(Vector3 centerPos, Vector3 playerPos)
    {
        // 1. Cập nhật tiến trình chuyển tiếp từ waypoint cuối -> quỹ đạo
        UpdateTransitionProgress();

        // 2. Cập nhật bán kính quỹ đạo hiện tại (tiến dần về bán kính mục tiêu)
        UpdateOrbitRadius();

        // 3. Tính toán vị trí lý tưởng trên vòng tròn quỹ đạo (đối diện người chơi)
        Vector3 idealOrbitalPosition = CalculateIdealOrbitPosition(centerPos, playerPos);

        // 4. Tính toán vị trí mục tiêu thực tế cho frame này (nội suy giữa điểm cuối waypoint và vị trí quỹ đạo lý tưởng)
        Vector3 targetPositionThisFrame = CalculateTargetPosition(idealOrbitalPosition);

        // 5. Di chuyển máy bay tới vị trí mục tiêu
        MoveTowardsPosition(targetPositionThisFrame);

        // 6. Xoay máy bay hướng về người chơi, áp dụng hiệu ứng nghiêng (roll)
        RotateTowardsPlayer(targetPositionThisFrame, centerPos, playerPos);
    }

    #endregion

    #region Calculation Methods (Phương thức tính toán)

    /// <summary>
    /// Cập nhật tiến trình chuyển tiếp, giới hạn trong khoảng 0 đến 1.
    /// </summary>
    private void UpdateTransitionProgress()
    {
        // Chỉ cập nhật nếu chưa hoàn thành chuyển tiếp
        if (transitionProgress < 1.0f)
        {
            transitionProgress = Mathf.Clamp01((Time.time - transitionStartTime) / transitionTime);
        }
    }

    /// <summary>
    /// Cập nhật bán kính quỹ đạo hiện tại một cách mượt mà tiến về bán kính mục tiêu.
    /// </summary>
    private void UpdateOrbitRadius()
    {
        // Sử dụng SmoothDamp để thay đổi bán kính hiện tại về bán kính mục tiêu một cách mượt mà
        // Thời gian ước tính để đạt gần đến đích là tham số thứ 4 (0.5f giây ở đây)
        currentOrbitRadius = Mathf.SmoothDamp(currentOrbitRadius, targetOrbitRadius, ref radiusSmoothVelocity, 0.5f);
    }

    /// <summary>
    /// Tính toán vị trí lý tưởng trên vòng tròn quỹ đạo, nằm đối diện với người chơi so với tâm.
    /// </summary>
    /// <param name="centerPos">Vị trí tâm quỹ đạo.</param>
    /// <param name="playerPos">Vị trí người chơi.</param>
    /// <returns>Vị trí lý tưởng trên quỹ đạo (cùng độ cao với máy bay hiện tại).</returns>
    private Vector3 CalculateIdealOrbitPosition(Vector3 centerPos, Vector3 playerPos)
    {
        // Tính vector hướng từ tâm đến người chơi (chỉ trên mặt phẳng XZ)
        Vector3 directionToPlayer = playerPos - centerPos;
        directionToPlayer.y = 0; // Bỏ qua sự khác biệt về độ cao

        // Tính góc của người chơi so với tâm (trên mặt phẳng XZ)
        float playerAngleRad = Mathf.Atan2(directionToPlayer.z, directionToPlayer.x);

        // Góc của máy bay nên ở phía đối diện (cộng thêm PI radian hoặc 180 độ)
        float helicopterAngleRad = playerAngleRad + Mathf.PI;

        // Tính toán tọa độ X, Z trên vòng tròn quỹ đạo dựa vào góc và bán kính hiện tại
        float targetX = centerPos.x + currentOrbitRadius * Mathf.Cos(helicopterAngleRad);
        float targetZ = centerPos.z + currentOrbitRadius * Mathf.Sin(helicopterAngleRad);

        // Trả về vị trí với độ cao Y giữ nguyên như của máy bay hiện tại
        return new Vector3(targetX, transform.position.y, targetZ);
    }

    /// <summary>
    /// Tính toán vị trí đích thực tế cho máy bay trong frame này.
    /// Trong giai đoạn chuyển tiếp, vị trí này là kết quả nội suy giữa điểm waypoint cuối và vị trí quỹ đạo lý tưởng.
    /// Sau khi chuyển tiếp xong, vị trí này chính là vị trí quỹ đạo lý tưởng.
    /// </summary>
    /// <param name="idealOrbitalPosition">Vị trí lý tưởng trên quỹ đạo đã tính.</param>
    /// <returns>Vị trí đích cuối cùng cho frame này.</returns>
    private Vector3 CalculateTargetPosition(Vector3 idealOrbitalPosition)
    {
        // Nếu chưa hoàn thành chuyển tiếp (progress < 1)
        if (transitionProgress < 1.0f)
        {
            // Nội suy tuyến tính (Lerp) giữa vị trí waypoint cuối và vị trí quỹ đạo lý tưởng
            // Dựa trên tiến trình chuyển tiếp (transitionProgress)
            return Vector3.Lerp(lastWaypointPosition, idealOrbitalPosition, transitionProgress);
        }
        else
        {
            // Nếu đã chuyển tiếp xong, đích đến chính là vị trí quỹ đạo lý tưởng
            return idealOrbitalPosition;
        }
    }

    /// <summary>
    /// Di chuyển máy bay về phía vị trí mục tiêu bằng cách sử dụng Lerp.
    /// Lưu ý: Lerp với Time.deltaTime * speed không đảm bảo tốc độ không đổi,
    /// nhưng giữ nguyên logic gốc theo yêu cầu. Để tốc độ không đổi nên dùng MoveTowards.
    /// </summary>
    /// <param name="targetPosition">Vị trí đích cần di chuyển tới.</param>
    private void MoveTowardsPosition(Vector3 targetPosition)
    {
        // Nội suy vị trí hiện tại tới vị trí đích
        // Tốc độ di chuyển phụ thuộc vào 'radiusChangeSpeed' và khoảng cách còn lại
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * radiusChangeSpeed);
        // Để di chuyển với tốc độ không đổi 'flySpeed', dùng:
        // transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);
    }

    /// <summary>
    /// Xoay máy bay để hướng về phía người chơi và áp dụng góc nghiêng (roll).
    /// </summary>
    /// <param name="currentHelicopterPosOnOrbit">Vị trí hiện tại của máy bay trên quỹ đạo (dùng để tính roll).</param>
    /// <param name="centerPos">Vị trí tâm quỹ đạo.</param>
    /// <param name="playerPos">Vị trí người chơi.</param>
    private void RotateTowardsPlayer(Vector3 currentHelicopterPosOnOrbit, Vector3 centerPos, Vector3 playerPos)
    {
        // Tính vector hướng nhìn từ máy bay tới người chơi
        Vector3 lookDirection = playerPos - transform.position;
        lookDirection.y = 0; // Chỉ xoay trên mặt phẳng XZ

        // Chỉ thực hiện xoay nếu hướng nhìn đủ lớn (tránh lỗi khi ở quá gần)
        if (lookDirection.magnitude > 0.01f)
        {
            // Tính góc xoay (Quaternion) để nhìn về hướng đó
            Quaternion targetLookRotation = Quaternion.LookRotation(lookDirection);

            // --- Tính toán góc nghiêng (Roll) ---
            // Tính vector từ tâm đến vị trí hiện tại của máy bay trên quỹ đạo (trên mặt phẳng XZ)
            Vector3 directionFromCenter = currentHelicopterPosOnOrbit - centerPos;
            directionFromCenter.y = 0;
            // Tính góc của máy bay trên quỹ đạo (radian)
            float helicopterAngleOnOrbitRad = Mathf.Atan2(directionFromCenter.z, directionFromCenter.x);

            // Tính góc nghiêng roll: dùng Sin của góc trên quỹ đạo để có hiệu ứng nghiêng vào tâm/ra ngoài tâm
            // Nhân với rollAngle (góc nghiêng tối đa) và transitionProgress (nghiêng tăng dần khi vào quỹ đạo)
            // Dấu trừ có thể cần thiết tùy thuộc vào hệ tọa độ và mong muốn nghiêng vào hay ra
            //float roll = -Mathf.Sin(helicopterAngleOnOrbitRad) * rollAngle * transitionProgress;
            
            float roll = rollAngle * transitionProgress; // Chỉ cần dùng giá trị rollAngle đã đặt
            // Tạo Quaternion cho góc nghiêng roll quanh trục Z cục bộ (trục forward)
            Quaternion rollRotation = Quaternion.Euler(0, 0, roll);

            // Kết hợp góc xoay nhìn mục tiêu và góc nghiêng roll
            // Lưu ý: Thứ tự nhân Quaternion quan trọng. Ở đây, áp dụng roll *sau khi* đã xoay nhìn mục tiêu.
            Quaternion finalTargetRotation = targetLookRotation * rollRotation;
            // --- Kết thúc tính toán góc nghiêng ---

            // Xoay máy bay một cách mượt mà từ góc hiện tại đến góc đích cuối cùng
            transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * turnSpeed);
        }
    }


    #endregion

    #region Debug Methods (Phương thức vẽ Debug)

    /// <summary>
    /// Vẽ các thông tin gỡ lỗi trong Scene view.
    /// </summary>
    /// <param name="centerPos">Vị trí tâm.</param>
    /// <param name="playerPos">Vị trí người chơi.</param>
    /// <param name="helicopterPos">Vị trí máy bay.</param>
    /// <param name="orbitRadius">Bán kính quỹ đạo hiện tại.</param>
    private void DrawDebugInfo(Vector3 centerPos, Vector3 playerPos, Vector3 helicopterPos, float orbitRadius)
    {
        // Chỉ vẽ nếu ứng dụng đang chạy trong Editor (tránh lỗi build) và có tham chiếu hợp lệ
        #if UNITY_EDITOR
        if (!Application.isPlaying || orbitCenter == null) return;

        DrawOrbitCircle(centerPos, orbitRadius);
        DrawCenterToHelicopterLine(centerPos, helicopterPos);
        DrawForwardRay(helicopterPos);
        DrawCenterMarker(centerPos);
        if (player != null) DrawHelicopterToPlayerLine(helicopterPos, playerPos);
        #endif
    }

    private void DrawCenterToHelicopterLine(Vector3 center, Vector3 helicopter)
    {
        Debug.DrawLine(center, helicopter, markerLineColor);
    }

    private void DrawOrbitCircle(Vector3 center, float radius)
    {
        if (circleSegments <= 0) return;
        float angleStep = 360f / circleSegments;
        // Bắt đầu từ điểm trên trục X dương
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= circleSegments; i++)
        {
            float currentAngle = angleStep * i;
            float rad = currentAngle * Mathf.Deg2Rad;
            // Tính điểm tiếp theo trên vòng tròn ở cùng độ cao Y với tâm
            Vector3 nextPoint = center + new Vector3(radius * Mathf.Cos(rad), 0, radius * Mathf.Sin(rad));
            Debug.DrawLine(prevPoint, nextPoint, circleColor);
            prevPoint = nextPoint;
        }
    }

    private void DrawForwardRay(Vector3 helicopterPos)
    {
        Debug.DrawRay(helicopterPos, transform.forward * 5f, forwardRayColor); // Kéo dài tia để dễ thấy hơn
    }

    private void DrawCenterMarker(Vector3 center)
    {
        Debug.DrawRay(center, Vector3.up * 3f, Color.cyan); // Tăng chiều cao marker
        Debug.DrawRay(center, Vector3.down * 3f, Color.cyan);
    }

    private void DrawHelicopterToPlayerLine(Vector3 helicopter, Vector3 player)
    {
        Debug.DrawLine(helicopter, player, Color.magenta);
    }

    #endregion
}