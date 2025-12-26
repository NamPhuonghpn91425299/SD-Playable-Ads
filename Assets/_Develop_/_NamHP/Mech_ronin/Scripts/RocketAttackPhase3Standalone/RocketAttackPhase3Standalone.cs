using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;
using static GameConstants;
/// <summary>
/// HỆ THỐNG TẤN CÔNG TÊN LỬA GIAI ĐOẠN 3 ĐỘC LẬP
/// Giải pháp hoàn toàn độc lập - không cần phụ thuộc vào bên ngoài
/// Sẵn sàng sao chép sang bất kỳ dự án mới nào và chạy ngay lập tức
/// </summary>
public class RocketAttackPhase3Standalone : MonoBehaviour
{
    [Header("CÀI ĐẶT CỐT LÕI")]
    [Tooltip("Prefab tên lửa giả - dùng để hiển thị hiệu ứng ban đầu")]
    [SerializeField] private GameObject rocketFakePrefab;
    [Tooltip("Prefab tên lửa thật - gây sát thương khi nổ")]
    [SerializeField] private GameObject realRocketPrefab;
    [Tooltip("Vị trí phóng tên lửa: 0: trái, 1: phải, 2: trên")]
    [SerializeField] private Transform[] launchPositions = new Transform[3]; // 0: left, 1: right, 2: above
    [Tooltip("Các điểm đích cho hướng tấn công bên trái")]
    [SerializeField] private Transform[] leftDestinations = new Transform[6];
    [Tooltip("Các điểm đích cho hướng tấn công bên phải")]
    [SerializeField] private Transform[] rightDestinations = new Transform[6];
    [Tooltip("Các điểm đích cho hướng tấn công từ trên")]
    [SerializeField] private Transform[] aboveDestinations = new Transform[8];

    [Header("CÀI ĐẶT CHUYỂN ĐỘNG")]
    [Tooltip("Thời gian di chuyển của tên lửa (giây)")]
    [SerializeField] private float movementDuration = 1f;
    [Tooltip("Tốc độ xoay của tên lửa (độ/giây)")]
    [SerializeField] private float rotationSpeed = 20f;
    [Tooltip("Độ trễ trước khi kích hoạt tên lửa thật (giây)")]
    [SerializeField] private float realRocketDelay = 0.1f;
    [Tooltip("Độ trễ đặc biệt cho hướng tấn công từ trên (giây)")]
    [SerializeField] private float aboveDirectionDelay = 0.1f;


    // Biến runtime
    private List<RocketMoveStandalone> activeRockets = new List<RocketMoveStandalone>();
    private Transform target;
    private bool isInitialized = false;
    /// <summary>
    /// Khởi tạo hệ thống với mục tiêu và camera
    /// </summary>
    /// <param name="targetTransform">Transform của mục tiêu tấn công</param>
    public void Initialize(Transform targetTransform)
    {
        this.target = targetTransform;

        // Auto-create missing components
        SetupMissingComponents();

        isInitialized = true;
        Debug.Log("✅ Rocket attack system initialized successfully");
    }

    /// <summary>
    /// Thiết lập các thành phần còn thiếu
    /// </summary>
    private void SetupMissingComponents()
    {
        SetupLaunchPositions();
        SetupDestinations();
    }

    /// <summary>
    /// Thiết lập vị trí phóng
    /// </summary>
    private void SetupLaunchPositions()
    {
        if (launchPositions == null || launchPositions.Length != 3)
            launchPositions = new Transform[3];

        Vector3[] positions = { new Vector3(-20f, 5f, 0f), new Vector3(20f, 5f, 0f), new Vector3(0f, 15f, 0f) };
        string[] names = { "LaunchPosition_Left", "LaunchPosition_Right", "LaunchPosition_Above" };

        for (int i = 0; i < 3; i++)
        {
            if (launchPositions[i] == null)
            {
                GameObject pos = new GameObject(names[i]);
                pos.transform.SetParent(transform);
                pos.transform.localPosition = positions[i];
                launchPositions[i] = pos.transform;
            }
        }
    }

    /// <summary>
    /// Tìm hoặc tạo mục tiêu
    /// </summary>
    private Transform FindOrCreateTarget()
    {
        if (PlayerInstant.Instance != null)
            return PlayerInstant.Instance.TF;

        GameObject defaultTarget = new GameObject("DefaultTarget");
        defaultTarget.transform.position = Vector3.zero;
        return defaultTarget.transform;
    }

    /// <summary>
    /// Bắt đầu tấn công tên lửa theo hướng chỉ định
    /// </summary>
    /// <param name="direction">Hướng tấn công: 0 = trái, 1 = phải, 2 = trên</param>
    public void StartAttack(int direction)
    {
        // Auto-initialize if not initialized
        if (!isInitialized)
        {
            Initialize(FindOrCreateTarget());
        }

        if (target == null || launchPositions == null || launchPositions.Length != 3)
        {
            Debug.LogError("❌ Rocket attack system not properly initialized");
            return;
        }

        StartCoroutine(AttackRoutine(direction));
    }

    /// <summary>
    /// Thiết lập các điểm đích
    /// </summary>
    private void SetupDestinations()
    {
        CreateDestinationArray(ref leftDestinations, "LeftDest", 6, -15f, 0f, 10f);
        CreateDestinationArray(ref rightDestinations, "RightDest", 6, 15f, 0f, 10f);
        CreateDestinationArray(ref aboveDestinations, "AboveDest", 8, 0f, 0f, 15f);
    }

    /// <summary>
    /// Coroutine chính xử lý đợt tấn công
    /// </summary>
    /// <param name="direction">Hướng tấn công</param>
    /// <returns>IEnumerator cho coroutine</returns>
    private IEnumerator AttackRoutine(int direction)
    {
        // Get 4 random destinations for this direction
        Transform[] destinations = GetRandomDestinations(direction, 4);

        // Launch 4 rockets
        for (int i = 0; i < 4; i++)
        {
            // Special delay for above direction
            if (direction == 2)
            {
                yield return new WaitForSeconds(aboveDirectionDelay);
            }

            if (destinations[i] != null)
            {
                LaunchSingleRocket(launchPositions[direction], destinations[i]);
            }

            yield return new WaitForSeconds(0.2f); // Small delay between rockets
        }
    }

    /// <summary>
    /// Phóng một tên lửa đơn lẻ
    /// </summary>
    /// <param name="launchPos">Vị trí phóng tên lửa</param>
    /// <param name="destination">Điểm đích của tên lửa</param>
    private void LaunchSingleRocket(Transform launchPos, Transform destination)
    {
        if (rocketFakePrefab == null || launchPos == null || destination == null)
            return;

        var rocket = SimplePool<ProjectileEnemy>.Spawn<RocketMoveStandalone>(ProjectileEnemy.MiniRocket_RoninPhase3_Fake, launchPos.position, launchPos.rotation);
        rocket.SetupRocketFake(destination, target, movementDuration, rotationSpeed, realRocketPrefab);
        activeRockets.Add(rocket);
    }


    /// <summary>
    /// Kích hoạt tên lửa thật từ các tên lửa giả
    /// </summary>
    public void TriggerRealRockets()
    {
        StartCoroutine(TriggerRealRocketsRoutine());
    }

    /// <summary>
    /// Coroutine kích hoạt tên lửa thật
    /// </summary>
    /// <returns>IEnumerator cho coroutine</returns>
    private IEnumerator TriggerRealRocketsRoutine()
    {
        // Create copy to avoid modification during iteration
        List<RocketMoveStandalone> rocketsToTrigger = new List<RocketMoveStandalone>(activeRockets);

        foreach (var rocket in rocketsToTrigger)
        {
            if (rocket != null && rocket.gameObject.activeInHierarchy)
            {
                rocket.SpawnRealRocket();
                yield return new WaitForSeconds(realRocketDelay);
            }
        }

        // Clear the list
        activeRockets.Clear();
    }

    /// <summary>
    /// Lấy các điểm đích ngẫu nhiên cho hướng chỉ định
    /// </summary>
    /// <param name="direction">Hướng tấn công</param>
    /// <param name="count">Số lượng điểm đích cần lấy</param>
    /// <returns>Mảng các điểm đích ngẫu nhiên</returns>
    private Transform[] GetRandomDestinations(int direction, int count)
    {
        Transform[] destinations;
        switch (direction)
        {
            case 0:
                destinations = leftDestinations;
                break;
            case 1:
                destinations = rightDestinations;
                break;
            case 2:
                destinations = aboveDestinations;
                break;
            default:
                destinations = new Transform[0];
                break;
        }

        if (destinations == null || destinations.Length == 0)
            return new Transform[0];

        // Random selection
        List<Transform> available = destinations.Where(d => d != null).ToList();
        if (available.Count == 0)
            return new Transform[0];

        Transform[] result = new Transform[count];
        for (int i = 0; i < count && i < available.Count; i++)
        {
            int index = Random.Range(0, available.Count);
            result[i] = available[index];
            available.RemoveAt(index);
        }

        return result;
    }


    /// <summary>
    /// Tạo mảng điểm đích với các tham số chỉ định
    /// </summary>
    /// <param name="destinations">Tham chiếu đến mảng điểm đích cần tạo</param>
    /// <param name="prefix">Tiền tố tên cho các điểm đích</param>
    /// <param name="count">Số lượng điểm đích cần tạo</param>
    /// <param name="offsetX">Độ lệch trục X</param>
    /// <param name="offsetY">Độ lệch trục Y</param>
    /// <param name="offsetZ">Độ lệch trục Z</param>
    private void CreateDestinationArray(ref Transform[] destinations, string prefix, int count, float offsetX, float offsetY, float offsetZ)
    {
        // Check if destinations already exist
        if (destinations != null && destinations.Length == count)
        {
            bool allExist = true;
            foreach (var dest in destinations)
            {
                if (dest == null)
                {
                    allExist = false;
                    break;
                }
            }
            if (allExist) return;
        }

        // Create new array
        destinations = new Transform[count];

        // Create destination objects
        for (int i = 0; i < count; i++)
        {
            string destName = $"{prefix}_{i}";

            // Try to find existing destination
            Transform existingDest = transform.Find(destName);
            if (existingDest != null)
            {
                destinations[i] = existingDest;
            }
            else
            {
                // Create new destination
                GameObject destObj = new GameObject(destName);
                destObj.transform.SetParent(transform);

                // Set position with some randomness
                Vector3 position = new Vector3(
                    offsetX + Random.Range(-3f, 3f),
                    offsetY + Random.Range(-2f, 2f),
                    offsetZ + Random.Range(-3f, 3f)
                );

                destObj.transform.position = position;
                destinations[i] = destObj.transform;

                Debug.Log($"Created destination: {destName} at {position}");
            }
        }
    }

    /// <summary>
    /// Dừng tất cả các cuộc tấn công
    /// </summary>
    public void StopAllAttacks()
    {
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        StopAllAttacks();
    }

    private void OnDestroy()
    {
        StopAllAttacks();
    }

    #region Editor Helpers
    [ContextMenu("Test Attack Left")]
    private void TestAttackLeft() => StartAttack(0);

    [ContextMenu("Test Attack Right")]
    private void TestAttackRight() => StartAttack(1);

    [ContextMenu("Test Attack Above")]
    private void TestAttackAbove() => StartAttack(2);

    [ContextMenu("Trigger Real Rockets")]
    private void TestTriggerRealRockets() => TriggerRealRockets();

    [ContextMenu("Create Destinations")]
    private void TestCreateDestinations()
    {
        SetupDestinations();
        Debug.Log("Auto-created destination points");
    }
    #endregion
}
