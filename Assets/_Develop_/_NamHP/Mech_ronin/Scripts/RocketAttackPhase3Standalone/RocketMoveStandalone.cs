using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;
using static GameConstants;
/// <summary>
/// COMPONENT DI CHUYỂN TÊN LỬA ĐỘC LẬP - Giải pháp hoàn chỉnh độc lập
/// Sử dụng DOTween để tạo các animation chuyển động mượt mà
/// Component này quản lý việc di chuyển của tên lửa giả từ vị trí phóng đến điểm đích,
/// sau đó kích hoạt tên lửa thật để gây sát thương và hiệu ứng nổ.
/// </summary>
public class RocketMoveStandalone : GameUnit<ProjectileEnemy>
{
    #region Biến chuyển động cốt lõi
    private Transform destination;          // Điểm đích của tên lửa
    private Transform target;              // Mục tiêu tấn công (thường là player)
    private float rotationSpeed;           // Tốc độ xoay của tên lửa
    private float movementDuration;        // Thời gian di chuyển tổng thể
    #endregion

    #region Tham số tên lửa
    private GameObject realRocketPrefab;   // Prefab của tên lửa thật
    #endregion

    #region Components
    private Transform myTrans;             // Transform của component này
    private Tween movementTween;           // Tween xử lý di chuyển
    private Tween rotationTween;           // Tween xử lý xoay
    #endregion

    #region Performance Optimization
    private static readonly Vector3[] _positionCache = new Vector3[10];
    private static int _posCacheIndex = 0;
    private Quaternion _targetRotationCache;
    private bool _hasCachedRotation = false;
    #endregion

    /// <summary>
    /// Thiết lập tên lửa với tất cả các tham số cần thiết (tối ưu hiệu năng)
    /// </summary>
    /// <param name="inputDes">Điểm đích mà tên lửa sẽ di chuyển đến</param>
    /// <param name="inputTarget">Mục tiêu mà tên lửa sẽ hướng tới khi xoay</param>
    /// <param name="duration">Thời gian di chuyển từ vị trí phóng đến điểm đích (giây)</param>
    /// <param name="rotaSpeed">Tốc độ xoay của tên lửa (độ/giây)</param>
    /// <param name="realRocketPrefabObj">Prefab của tên lửa thật sẽ được tạo khi kích hoạt</param>
    /// <param name="rocketDamage">Lượng sát thương gây ra khi tên lửa nổ</param>
    /// <param name="explosionRadiusVal">Bán kính hiệu ứng vụ nổ</param>
    public void SetupRocketFake(Transform inputDes, Transform inputTarget, float duration, float rotaSpeed,
        GameObject realRocketPrefabObj)
    {
        this.destination = inputDes;
        this.target = inputTarget;
        this.movementDuration = duration;
        this.rotationSpeed = rotaSpeed;
        this.realRocketPrefab = realRocketPrefabObj;
        myTrans = transform;

        // Reset cache cho performance optimization
        _hasCachedRotation = false;

        // Bắt đầu chuyển động ngay sau khi thiết lập
        StartMovement();
    }

    /// <summary>
    /// Bắt đầu logic chuyển động cốt lõi sử dụng DOTween
    /// Khởi tạo cả chuyển động di chuyển và xoay của tên lửa
    /// </summary>
    private void StartMovement()
    {
        if (destination == null) return;

        // Hủy các tween đang hoạt động (nếu có)
        movementTween?.Kill();
        rotationTween?.Kill();

        // Bắt đầu tween di chuyển với DOTween
        movementTween = myTrans
            .DOMove(destination.position, movementDuration)
            .SetEase(Ease.OutCubic)  // Sử dụng easing OutCubic cho chuyển động mượt mà
            .OnComplete(OnReachDestination);

        // Bắt đầu xoay sau 40% thời gian di chuyển
        float rotationDelay = movementDuration * 0.4f;
        rotationTween = DOVirtual.DelayedCall(rotationDelay, () =>
        {
            if (target != null && this != null)
            {
                StartRotationTween();
            }
        });
    }

    /// <summary>
    /// Bắt đầu tween xoay sử dụng DOTween (tối ưu hiệu năng)
    /// Xoay tên lửa để hướng về phía mục tiêu
    /// </summary>
    private void StartRotationTween()
    {
        if (target == null || myTrans == null) return;

        // Cache target position calculation
        Vector3 targetPos = SetPosY(target.position, myTrans.position.y);

        // Tính toán góc xoay mục tiêu với optimization
        Vector3 direction = targetPos - myTrans.position;

        // Sử dụng sqrMagnitude thay vì so sánh Vector3.zero cho performance
        if (direction.sqrMagnitude < 0.001f) return;

        // Cache rotation calculation để tránh tính toán lại
        if (!_hasCachedRotation)
        {
            _targetRotationCache = Quaternion.LookRotation(direction);
            _hasCachedRotation = true;
        }

        // Sử dụng DOTween để xoay mượt mà
        rotationTween = myTrans
            .DORotateQuaternion(_targetRotationCache, 0.5f)
            .SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Xử lý khi tên lửa đến được điểm đích
    /// Gắn tên lửa vào điểm đích và chờ kích hoạt tên lửa thật
    /// </summary>
    private void OnReachDestination()
    {
        // Gắn tên lửa vào điểm đích cha
        if (destination != null)
        {
            myTrans.SetParent(destination);
        }
    }

    /// <summary>
    /// Tạo tên lửa thật với hiệu ứng nổ
    /// Phương thức công khai được gọi từ bên ngoài để kích hoạt vụ nổ
    /// </summary>
    public void SpawnRealRocket()
    {
        if (realRocketPrefab == null)
        {
            // Không có prefab tên lửa thật, chỉ hủy tên lửa giả
            OnDespawn();
            return;
        }

        // Tạo tên lửa thật tại vị trí hiện tại
        SimplePool<ProjectileEnemy>.Spawn<SnakeMovementController>(ProjectileEnemy.MiniRocket_RoninPhase3_Real, myTrans.position, myTrans.rotation);

        // Hủy tên lửa giả
        OnDespawn();
    }


    /// <summary>
    /// Phương thức helper để đặt vị trí Y (tối ưu hiệu năng)
    /// Giữ nguyên X và Z, chỉ thay đổi giá trị Y
    /// </summary>
    /// <param name="source">Vector3 nguồn</param>
    /// <param name="y">Giá trị Y mới</param>
    /// <returns>Vector3 với giá trị Y mới</returns>
    private Vector3 SetPosY(Vector3 source, float y)
    {
        // Dùng cache để giảm GC allocation
        _posCacheIndex = (_posCacheIndex + 1) % _positionCache.Length;
        _positionCache[_posCacheIndex] = new Vector3(source.x, y, source.z);
        return _positionCache[_posCacheIndex];
    }

    /// <summary>
    /// Xử lý khi đối tượng bị vô hiệu hóa (tối ưu hiệu năng)
    /// Dừng các tween đang hoạt động và dọn dẹp cache
    /// </summary>
    private void OnDisable()
    {
        // Xóa các tham chiếu
        destination = null;
        target = null;
        movementTween?.Kill();
        rotationTween?.Kill();

        // Dọn dẹp cache cho performance optimization
        _hasCachedRotation = false;
    }

    private void OnDespawn()
    {
        this.transform.SetParent(null);
        SimplePool<ProjectileEnemy>.Despawn(this);
    }
}

