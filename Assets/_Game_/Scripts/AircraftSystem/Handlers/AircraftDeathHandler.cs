using UnityEngine;

namespace AircraftSystem.Handlers
{
    /// <summary>
    /// Xử lý logic chết và rơi của máy bay bằng Raycast.
    /// </summary>
    public class AircraftDeathHandler
    {
        #region Fields

        private readonly Transform _aircraftTransform;
        private readonly AircraftFlightController.DeathType _deathType;
        private readonly float _fallSpeed;
        private readonly float _deathSpinSpeed;
        private readonly float _minFallHeight;
        private readonly bool _useCollisionDetection;
        private readonly LayerMask _explosionTriggerLayers;
        private Vector3 _currentVelocity;
        private bool _isExplosion = false;
        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo AircraftDeathHandler.
        /// </summary>
        /// <param name="aircraftTransform">Transform của máy bay.</param>
        /// <param name="deathType">Loại chết.</param>
        /// <param name="fallSpeed">Tốc độ rơi.</param>
        /// <param name="deathSpinSpeed">Tốc độ xoay khi chết.</param>
        /// <param name="minFallHeight">Độ cao tối thiểu trước khi dừng rơi.</param>
        /// <param name="useCollisionDetection">Có sử dụng collision detection không.</param>
        /// <param name="explosionTriggerLayers">Các layer sẽ kích nổ.</param>
        public AircraftDeathHandler(Transform aircraftTransform,
                                    AircraftFlightController.DeathType deathType,
                                    float fallSpeed, float deathSpinSpeed, float minFallHeight,
                                    bool useCollisionDetection, LayerMask explosionTriggerLayers)
        {
            _aircraftTransform = aircraftTransform;
            _deathType = deathType;
            _fallSpeed = fallSpeed;
            _deathSpinSpeed = deathSpinSpeed;
            _minFallHeight = minFallHeight;
            _useCollisionDetection = useCollisionDetection;
            _explosionTriggerLayers = explosionTriggerLayers;
            // Vận tốc ban đầu sẽ được set bởi Initialize
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Khởi tạo vận tốc ban đầu cho máy bay khi chết.
        /// </summary>
        /// <param name="initialVelocity">Vận tốc ban đầu.</param>
        public void Initialize(Vector3 initialVelocity)
        {
            _currentVelocity = initialVelocity;
        }

        /// <summary>
        /// Xử lý logic chết cho một frame.
        /// </summary>
        /// <returns>True nếu máy bay cần nổ, False nếu tiếp tục rơi.</returns>
        public bool ProcessDeathFrame()
        {
            // Chỉ xử lý rơi cho ShotDown và EngineFailure (Explosion đã nổ rồi)
            
            if (_deathType == AircraftFlightController.DeathType.Explosion || _isExplosion)
            {
                // Explosion nổ ngay lập tức, không cần xử lý frame-by-frame
                return true; // Báo hiệu cần nổ ngay
            }
            Vector3 currentPosition = _aircraftTransform.position;
            Vector3 newPosition = currentPosition;

            // Xoay loay sòay
            if (_deathSpinSpeed > 0)
            {
                _aircraftTransform.Rotate(0, 0, _deathSpinSpeed * Time.deltaTime, Space.Self);
            }

            // Tính vận tốc rơi
            Vector3 fallVelocity = new Vector3(0, _fallSpeed, 0) * Time.deltaTime;

            // Vận tốc ngang giảm dần (nếu có)
            _currentVelocity = Vector3.Lerp(_currentVelocity, Vector3.zero, Time.deltaTime * 0.5f);
            Vector3 horizontalVelocity = _currentVelocity * Time.deltaTime;

            // Vị trí mới
            newPosition += horizontalVelocity + fallVelocity;

            // --- RAYCAST CHECK ---
            bool hasHitGround = false;

            // Tính toán hướng raycast: từ vị trí hiện tại xuống dưới, hoặc theo hướng vận tốc rơi
            Vector3 rayDirection = (newPosition - currentPosition).normalized;
            // Nếu vận tốc rơi quá nhỏ (gần 0), dùng hướng down
            if (rayDirection.magnitude < 0.1f)
            {
                rayDirection = Vector3.down;
            }
            float rayDistance = Vector3.Distance(currentPosition, newPosition) + 0.1f; // Thêm một chút để chắc chắn

            // Thực hiện raycast
            // 1. Nếu bật collision detection, kiểm tra layer
            if (_useCollisionDetection)
            {
                // Chỉ raycast với các layer được chỉ định
                if (Physics.Raycast(currentPosition, rayDirection, out RaycastHit hitInfo, rayDistance, _explosionTriggerLayers))
                {
                    // Kiểm tra nếu điểm va chạm đủ thấp (dưới minFallHeight) hoặc chạm trực tiếp
                    // (Bạn có thể điều chỉnh điều kiện này)
                    if (hitInfo.point.y <= _minFallHeight || hitInfo.distance < 0.2f)
                    {
                        hasHitGround = true;
                        // Optional: Log tên object va chạm
                        // Debug.Log($"[AircraftDeathHandler] Hit ground via Raycast: {hitInfo.collider.gameObject.name}");
                    }
                }
            }
            // 2. Nếu tắt collision detection, chỉ cần kiểm tra độ cao
            else
            {
                // Kiểm tra nếu vị trí mới thấp hơn m_minFallHeight
                if (newPosition.y <= _minFallHeight)
                {
                    hasHitGround = true;
                }
            }

            // --- Xử lý kết quả ---
            if (hasHitGround || newPosition.y <= _minFallHeight)
            {
                // Đã chạm đất hoặc độ cao tối thiểu -> Báo hiệu cần nổ!
                _isExplosion = true;
                return true;
            }
            else
            {
                // Chưa chạm, update vị trí
                _aircraftTransform.position = newPosition;
                return false; // Tiếp tục rơi
            }
        }

        #endregion
    }
}