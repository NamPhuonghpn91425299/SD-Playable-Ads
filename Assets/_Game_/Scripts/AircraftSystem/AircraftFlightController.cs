using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using AircraftSystem.Components;
using AircraftSystem.Handlers;
using Assets._Develop_.ThanhNT.Scripts.Observer; // Thêm dòng này

namespace AircraftSystem
{
    /// <summary>
    /// Controller điều khiển máy bay bay theo pattern: 
    /// Spawn → MainPath → Attack Loop (Attack → Left/Right Exit → Attack → ...)
    /// 
    /// LOGIC BAY:
    /// 1. Lần đầu spawn: Bay qua waypoints (MainPath)
    /// 2. Chuyển sang Attack phase: Bay qua attack points và trigger tấn công
    /// 3. Chọn Exit: Bay lượn trái hoặc phải (LeftExit/RightExit)
    /// 4. Vòng lặp: Quay lại Attack → chọn Exit khác → Attack → ... (vô tận)
    /// 
    /// ROTATION MƯỢT:
    /// - Look Ahead Distance: Bắt đầu xoay sớm khi gần đến điểm tiếp theo
    /// - Blend Factor: Trộn hướng hiện tại với hướng tiếp theo để mượt
    /// - Rotation Speed: Tốc độ xoay có giới hạn (độ/giây)
    /// 
    /// SETUP:
    /// - Chỉ cần 1 PointGroup với tất cả các loại điểm
    /// - points: waypoints chính (lần đầu)
    /// - attackPoints: điểm tấn công (tự động detect tên có "attack")
    /// - leftPoints: điểm lượn trái (tự động detect tên có "left")
    /// - rightPoints: điểm lượn phải (tự động detect tên có "right")
    /// </summary>
    public class AircraftFlightController : MonoBehaviour
    {
        [SerializeField] private A10_Network _a10Network;
        [SerializeField] private Dead_Explosions deadExplosions;
        [SerializeField] private Transform body;
        public int CurrentDamage => _a10Network.Damage;
        [Header("<color=green>Flight Path</color>")]
        [Tooltip("PointGroup chứa tất cả waypoints (main), attack points (có 'attack' trong tên), left points (có 'left' trong tên), right points (có 'right' trong tên)")]
        [SerializeField] private PointGroup m_pointGroup;
        
        [Header("<color=green>Flight Settings</color>")]
        [Tooltip("Tốc độ bay của máy bay (units/giây). Tốc độ càng cao, máy bay bay càng nhanh. Range: 1-100")]
        [SerializeField] private float m_flySpeed = 30f;
        /// <summary>Tốc độ xoay tối đa của máy bay (độ/giây)</summary>
        [Tooltip("Tốc độ xoay tối đa của máy bay (độ/giây). Tốc độ xoay càng cao, máy bay rẽ càng nhanh. Range: 10-360")]
        [SerializeField] private float m_rotationSpeed = 30f;
        /// <summary>Khoảng cách đến điểm để coi như đã đến</summary>
        [Tooltip("Khoảng cách đến điểm để coi như đã đến. Distance càng nhỏ, máy bay bay càng chính xác. Range: 0.5-10")]
        [SerializeField] private float m_arrivalDistance = 5f;
        /// <summary>Khoảng cách bắt đầu xoay về điểm tiếp theo để cho rotation mượt</summary>
        [Tooltip("Khoảng cách bắt đầu xoay về điểm tiếp theo để rotation mượt. Distance càng lớn, máy bay xoay càng sớm. Range: 5-50")]
        [SerializeField] private float m_lookAheadDistance = 20f; // Giảm từ 15f để ổn định hơn
        
        [Header("<color=green>Advanced Rotation</color>")]
        [Tooltip("Độ mượt của rotation (0-1). 0 = responsive nhất, 1 = mượt nhất. Cao = ít giật cục nhưng chậm phản ứng")]
        [SerializeField] private float m_rotationSmoothness = 0.6f; // Giảm từ 0.8f để responsive hơn
        /// <summary>Số điểm look-ahead tối đa (nhìn trước nhiều điểm)</summary>
        [Tooltip("Số điểm look-ahead tối đa (nhìn trước nhiều điểm). Nhiều điểm = rotation thông minh hơn. Range: 1-10")]
        [SerializeField] private int m_maxLookAheadPoints = 3;
        /// <summary>Có dùng banking (nghiêng máy bay khi rẽ) không</summary>
        [Tooltip("Có dùng banking (nghiêng máy bay khi rẽ) không? True = realistic hơn, False = đơn giản hơn")]
        [SerializeField] private bool m_useBanking = true;
        /// <summary>Góc banking tối đa (độ)</summary>
        [Tooltip("Góc banking tối đa (độ). Góc càng lớn, máy bay nghiêng càng nhiều khi rẽ. Range: 0-90")]
        [SerializeField] private float m_maxBankAngle = 90f;
        /// <summary>Tốc độ banking</summary>
        [Tooltip("Tốc độ banking (tốc độ nghiêng máy bay). Cao = nghiêng nhanh, thấp = nghiêng chậm. Range: 0.5-10")]
        [SerializeField] private float m_bankingSpeed = 45f;
        
        [Header("<color=green>Strategic Banking</color>")]
        [Tooltip("Hệ số nhân cho strategic banking khi rẽ exit. 1.0 = 80% maxBank, 1.5 = 120% maxBank. Range: 0.5-2.0")]
        [SerializeField] private float m_exitBankingMultiplier = 1.0f;
        /// <summary>Hệ số nhân cho strategic banking khi vào attack (1.0 = bình thường, 1.5 = mạnh hơn)</summary>
        [Tooltip("Hệ số nhân cho strategic banking khi lượn vào attack. 1.0 = 60% maxBank, 1.5 = 90% maxBank. Range: 0.5-2.0")]
        [SerializeField] private float m_entryBankingMultiplier = 1.0f;
        
        [Header("<color=green>Flight Mode</color>")]
        [Tooltip("Tự động mở rộng đường cong khi góc cua hẹp (<30°) để tạo quỹ đạo bay đẹp và mượt mà hơn")]
        [SerializeField] private bool m_expandNarrowTurns = false; // Tắt để đơn giản hơn
        /// <summary>Hệ số mở rộng đường cong (1.0 = bình thường, 2.0 = mở rộng gấp đôi)</summary>
        [Tooltip("Hệ số mở rộng đường cong (1.0 = bình thường, 2.0 = mở rộng gấp đôi). Cao = cua rộng hơn, thấp = cua hẹp hơn")]
        [SerializeField] private float m_curveExpansionFactor = 1.5f;
        
        
        [Header("<color=green>Attack Settings</color>")]
        [Tooltip("Khoảng cách kích hoạt tấn công khi gần attack point. Range càng lớn, trigger attack từ xa. Range: 1-20")]
        [SerializeField] private float m_attackRange = 3f;
        /// <summary>Thời gian chờ giữa các lần tấn công (giây)</summary>
        [Tooltip("Thời gian chờ giữa các lần tấn công (giây). Cooldown càng nhỏ, attack càng thường xuyên. Range: 0.1-10")]
        [SerializeField] private int m_attackPoint = 0;
        
        [Header("<color=green>Exit Choice</color>")]
        [Tooltip("Cách chọn hướng exit sau attack: Left (luôn trái), Right (luôn phải), Alternate (xen kẽ), Random (ngẫu nhiên)")]
        [SerializeField] private ExitChoice m_exitChoice = ExitChoice.Random;
        
        [Header("<color=red>Dead & Fall Settings</color>")]

        [Tooltip("Tốc độ rơi khi chết (units/giây). Càng âm, rơi càng nhanh. Range: -50 to -5")]
        [SerializeField] private float m_fallSpeed = -12f;

        [Tooltip("Tốc độ xoay loay sòay khi chết (độ/giây). Range: 0-720")]
        [SerializeField] private float m_deathSpinSpeed = 180f;

        [Tooltip("Độ cao tối thiểu trước khi dừng rơi. -100 = rơi sâu dưới đất. Range: -100 to 0")]
        [SerializeField] private float m_minFallHeight = -50f;
        
        [Header("<color=orange>Collision Detection</color>")]

        [Tooltip("Chọn các layers sẽ làm máy bay nổ khi va chạm. Ví dụ: Ground, Terrain, Building, Obstacle layers")]
        [SerializeField] private LayerMask m_explosionTriggerLayers = 1; // Default: Default layer only

        [Tooltip("Bật để máy bay nổ khi chạm vào objects ở layers trên. Tắt để chỉ nổ khi chạm minFallHeight")]
        [SerializeField] private bool m_useCollisionDetection = true;
        
        public enum ExitChoice
        {
            /// <summary>Luôn chọn đường thoát trái</summary>
            Left,
            /// <summary>Luôn chọn đường thoát phải</summary>
            Right, 
            /// <summary>Xen kẽ trái-phải theo thứ tự</summary>
            Alternate,
            /// <summary>Chọn ngẫu nhiên trái hoặc phải</summary>
            Random
        }
        
        /// <summary>
        /// Các giai đoạn bay của máy bay - ĐƠN GIẢN
        /// </summary>
        public enum FlightPhase
        {
            /// <summary>Bay qua waypoints chính lần đầu tiên</summary>
            MainPath,
            /// <summary>Bay qua attack points và trigger tấn công</summary>
            Attack,
            /// <summary>Bay lượn theo đường thoát trái</summary>
            LeftExit,
            /// <summary>Bay lượn theo đường thoát phải</summary>
            RightExit,
            /// <summary>Chết rồi, không hoạt động nữa</summary>
            Dead
        }
        
        /// <summary>
        /// Các loại chết của máy bay
        /// </summary>
        public enum DeathType
        {
            /// <summary>Bị bắn rơi - rơi xuống rồi nổ khi chạm đất</summary>
            ShotDown,
            /// <summary>Hỏng máy - rơi chậm và nổ khi chạm đất</summary>
            EngineFailure,
            /// <summary>Nổ tại chỗ - nổ ngay lập tức</summary>
            Explosion
        }
        
        [Header("<color=green>Debug</color>")]
        [Tooltip("Hiển thị debug gizmos trong Scene view. Bật để thấy toàn bộ visualization (paths, ranges, directions)")]
        [SerializeField] private bool m_showDebugGizmos = true;
        /// <summary>Hiển thị text thông tin trên các điểm</summary>
        [Tooltip("Hiển thị text labels thông tin trên các điểm waypoints. Bật để thấy tên điểm, index, phase info")]
        [SerializeField] private bool m_showDebugLabels = true;
        /// <summary>Hiển thị trajectory đường bay</summary>
        [Tooltip("Hiển thị lịch sử trajectory đường bay máy bay đã đi qua. Màu cam với fade effect, giới hạn 500 điểm")]
        [SerializeField] private bool m_showFlightTrajectory = true;
        /// <summary>Hiển thị toàn bộ flight path (main + attack + exits)</summary>
        [Tooltip("Hiển thị toàn bộ flight paths có thể: Main (cyan), Attack (red), Left Exit (blue), Right Exit (magenta)")]
        [SerializeField] private bool m_showAllFlightPaths = true;
        /// <summary>Màu sắc của debug gizmos</summary>
        [Tooltip("Màu sắc chính của debug gizmos (waypoints, spheres). Không ảnh hưởng path colors")]
        [SerializeField] private Color m_gizmoColor = Color.yellow;
        /// <summary>Kích thước gizmo sphere</summary>
        [Tooltip("Kích thước sphere gizmos của các waypoints. 0.5 = vừa phải, 1.0 = to, 0.2 = nhỏ")]
        [SerializeField] private float m_gizmoSize = 0.5f;
        /// <summary>Hiển thị movement direction để debug hướng bay</summary>
        [Tooltip("Hiển thị movement direction rays để debug hướng bay: Đỏ = hướng mũi thực tế, Xanh = hướng tới target, Cyan = hướng tối ưu")]
        [SerializeField] private bool m_showMovementDirection = true;
        
        #region Runtime Variables & Components
        // ===== COMPONENT ARCHITECTURE =====
        /// <summary>Component quản lý rotation và banking</summary>
        private AircraftRotationController rotationController;
        /// <summary>Component quản lý attack system</summary>
        private AircraftAttackSystem attackSystem;
        /// <summary>Component quản lý path và phase transitions</summary>
        private AircraftPathManager pathManager;
        /// <summary>Handler xử lý logic chết</summary>
        private AircraftDeathHandler _deathHandler; // Thêm dòng này
        
        // ===== TRAJECTORY DEBUG =====
        /// <summary>Lịch sử đường bay để vẽ trajectory</summary>
        private List<Vector3> m_flightTrajectory = new List<Vector3>();
        /// <summary>Thời gian cập nhật trajectory cuối cùng</summary>
        private float m_lastTrajectoryTime = 0f;
        /// <summary>Khoảng cách tối thiểu để thêm điểm trajectory mới</summary>
        private const float TRAJECTORY_MIN_DISTANCE = 0.5f;
        
        // ===== DEATH LOGIC =====
        /// <summary>Đã chết chưa</summary>
        private bool m_isDead = false;
        /// <summary>Thời điểm chết</summary>
        private float m_deathTime = 0f;
        /// <summary>Vận tốc hiện tại của máy bay (vector3)</summary>
        private Vector3 m_currentVelocity = Vector3.zero;
        /// <summary>Loại chết của máy bay</summary>
        private DeathType m_deathType = DeathType.ShotDown;
        #endregion
        
        #region Events
        /// <summary>
        /// Event kích hoạt khi máy bay trigger tấn công tại một attack point
        /// </summary>
        /// <param name="attackPoint">Attack point được kích hoạt</param>
        public System.Action<Transform> OnAttackTriggered;
        
        /// <summary>
        /// Event kích hoạt khi máy bay chuyển đổi giai đoạn bay
        /// </summary>
        /// <param name="newPhase">Giai đoạn bay mới</param>
        public System.Action<FlightPhase> OnPhaseChanged;
        #endregion
        
        #region Public Methods
 
        /// <summary>
        /// Public property để component có thể truy cập arrival distance
        /// </summary>
        public float ArrivalDistance => m_arrivalDistance;
        
        #endregion
        
        #region Unity Lifecycle
        private void Start()
        {
            m_pointGroup = _a10Network.botIdentity.AssignedPath;
            InitializeComponents();
            StartFlightPattern();
        }
        
        private void OnEnable()
        {
            if (_a10Network.isBoss)
            {
                Debug.Log("Boss spawn");
                EventManager.Instance.Publish(new BossSpawnEvent(true));
            }
            // Nếu máy bay được enable lại mà đang ở trạng thái chết, 
            // thì reset nó về trạng thái ban đầu.
            // Điều này hữu ích khi bạn đơn giản gọi SetActive(true) 
            // thay vì dùng ReactivateAircraft.
            if (m_isDead)
            {
                // Lấy vị trí và rotation ban đầu từ PointGroup nếu có
                Vector3 resetPosition = transform.position;
                Quaternion resetRotation = transform.rotation;
                
                if (m_pointGroup != null && m_pointGroup.points != null && m_pointGroup.points.Count > 0)
                {
                    resetPosition = m_pointGroup.points[0].position;
                    // Giả sử rotation ban đầu là identity, bạn có thể tùy chỉnh
                }
                
                // Reset mà không cần truyền tham số cụ thể
                // ReactivateAircraft sẽ đảm bảo mọi thứ được reset
                ReactivateAircraft(resetPosition, resetRotation);
            }

            _a10Network.ACBotDead += botDead =>
            {
                Die(DeathType.ShotDown, "Shot down by player");
            };
        }

        private void OnDisable()
        {
            _a10Network.ACBotDead -= botDead =>
            {
                Die(DeathType.ShotDown, "Shot down by player");
            };
        }

        private bool _isExplosion;
        private void Update()
        {
            // Nếu chết rồi thì chỉ xử lý death logic
            if (m_isDead)
            {
                if (_deathHandler != null && _deathHandler.ProcessDeathFrame() && !_isExplosion)
                {
                    _isExplosion = true;
                    ExplodeAircraft();
                    
                }
                UpdateTrajectory(); // Vẫn cập nhật trajectory để thấy đường rơi
                return;
            }
            AircraftFlying();
        }
        #endregion
        
        #region Component Architecture

        public void AircraftFlying()
        {
            // Logic bay bình thường khi chưa chết
            if (pathManager?.CurrentWaypoints == null || pathManager.CurrentWaypoints.Count == 0) return;
            
            ProcessFlight();     // Xử lý di chuyển
            attackSystem?.CheckAttackTriggers(pathManager.CurrentPhase); // Component attack system
            rotationController?.UpdateStrategicBanking(
                pathManager.CurrentPhase, pathManager.CurrentWaypoints, pathManager.CurrentPointIndex,
                pathManager.LoopCount, m_exitChoice, pathManager.IsAlternateLeft); // Strategic banking
#if UNITY_EDITOR
            UpdateTrajectory();  // Debug trajectory
#endif
        }
        
        /// <summary>
        /// Khởi tạo các components - COMPONENT ARCHITECTURE
        /// </summary>
        private void InitializeComponents()
        {
            //Debug.Log($"[AircraftFlight] Initializing component-based architecture for {name}...");
            
            // Initialize rotation controller
            rotationController = new AircraftRotationController(this, transform);
            ApplyRotationSettings();
            
            // Initialize attack system
            attackSystem = new AircraftAttackSystem(this, transform, m_pointGroup, m_attackPoint);
            ApplyAttackSettings();
            
            // Initialize path manager
            pathManager = new AircraftPathManager(this, m_pointGroup);
            ApplyPathSettings();
            
            // Setup component events
            SetupComponentEvents();
            
            Debug.Log($"[AircraftFlight] Component architecture initialized successfully!");
        }
        
        /// <summary>
        /// Bắt đầu flight pattern sử dụng path manager component
        /// </summary>
        private void StartFlightPattern()
        {
            pathManager?.StartMainPhase();
        }
        
        /// <summary>
        /// Áp dụng cài đặt rotation cho component
        /// </summary>
        private void ApplyRotationSettings()
        {
            if (rotationController == null) return;
            
            rotationController.rotationSpeed = m_rotationSpeed;
            rotationController.lookAheadDistance = m_lookAheadDistance;
            rotationController.rotationSmoothness = m_rotationSmoothness;
            rotationController.useBanking = m_useBanking;
            rotationController.maxBankAngle = m_maxBankAngle;
            rotationController.bankingSpeed = m_bankingSpeed;
            rotationController.exitBankingMultiplier = m_exitBankingMultiplier;
            rotationController.entryBankingMultiplier = m_entryBankingMultiplier;
            rotationController.showMovementDirection = m_showMovementDirection;
        }
        
        /// <summary>
        /// Áp dụng cài đặt attack cho component
        /// </summary>
        private void ApplyAttackSettings()
        {
            if (attackSystem == null) return;
            
            attackSystem.attackRange = m_attackRange;
            attackSystem.AttackPoint = m_attackPoint;
        }
        
        /// <summary>
        /// Áp dụng cài đặt path cho component
        /// </summary>
        private void ApplyPathSettings()
        {
            if (pathManager == null) return;
            
            pathManager.exitChoice = m_exitChoice;
        }
        
        /// <summary>
        /// Thiết lập các events cho components
        /// </summary>
        private void SetupComponentEvents()
        {
            // Attack events
            if (attackSystem != null)
            {
                attackSystem.OnAttackTriggered += (attackPoint) => {
                    OnAttackTriggered?.Invoke(attackPoint);
                };
            }
            
            // Path events
            if (pathManager != null)
            {
                pathManager.OnPhaseChanged += (newPhase) => {
                    OnPhaseChanged?.Invoke(newPhase);
                };
            }
        }
        
        /// <summary>
        /// Xử lý bay sử dụng COMPONENT ARCHITECTURE
        /// 1. Sử dụng rotationController để handle rotation + banking
        /// 2. Di chuyển theo transform.forward (như máy bay thật)
        /// 3. Sử dụng pathManager để check arrival và transitions
        /// </summary>
        private void ProcessFlight()
        {
            Transform targetPoint = pathManager.CurrentTarget;
            if (targetPoint == null)
            {
                pathManager.CheckPathCompletion();
                return;
            }
            
            Vector3 targetPos = targetPoint.position;
            Vector3 currentPos = transform.position;
            float distanceToTarget = Vector3.Distance(currentPos, targetPos);
            
            // 1. Sử dụng rotation controller - COMPONENT ARCHITECTURE
            rotationController?.HandleRotation(targetPos, distanceToTarget, pathManager.CurrentWaypoints, pathManager.CurrentPointIndex);
            
            // 2. Di chuyển theo transform.forward (như máy bay thật)
            Vector3 flightDirection = transform.forward;
            transform.position += flightDirection * m_flySpeed * Time.deltaTime;
            
            // 3. Kiểm tra arrival và advance waypoint - Sử dụng pathManager
            if (distanceToTarget <= m_arrivalDistance)
            {
                pathManager.AdvanceToNextWaypoint();
                pathManager.CheckPathCompletion();
            }
        }
        #endregion
        
        #region Trajectory Debug
        /// <summary>
        /// Cập nhật trajectory - ghi lại đường bay để vẽ debug
        /// </summary>
        private void UpdateTrajectory()
        {
            Vector3 currentPos = transform.position;
            
            // Chỉ thêm điểm mới nếu cách điểm cuối đủ xa
            if (m_flightTrajectory.Count == 0 || 
                Vector3.Distance(currentPos, m_flightTrajectory[m_flightTrajectory.Count - 1]) >= TRAJECTORY_MIN_DISTANCE)
            {
                m_flightTrajectory.Add(currentPos);
                m_lastTrajectoryTime = Time.time;
                
                // Giới hạn số điểm trajectory (giữ 500 điểm cuối)
                if (m_flightTrajectory.Count > 500)
                {
                    m_flightTrajectory.RemoveAt(0);
                }
            }
        }
        #endregion
        
        #region Death Logic
        /// <summary>
        /// Gọi khi máy bay bị bắn chết hoặc gặp sự cố - PUBLIC METHOD
        /// </summary>
        public void Die(DeathType deathType = DeathType.ShotDown, string reason = "Unknown")
        {
            if (m_isDead) return; // Đã chết rồi
            
            m_isDead = true;
            m_deathTime = Time.time;
            m_deathType = deathType;
            
            // Lưu vận tốc hiện tại để tiếp tục bay theo quán tính
            m_currentVelocity = transform.forward * m_flySpeed;
            
            // Khởi tạo Death Handler
            _deathHandler = new AircraftDeathHandler(
                body , deathType,
                m_fallSpeed, m_deathSpinSpeed, m_minFallHeight,
                m_useCollisionDetection, m_explosionTriggerLayers
            );
            _deathHandler.Initialize(m_currentVelocity);

            // Cập nhật phase thành Dead
            if (pathManager != null)
            {
                pathManager.SetPhase(FlightPhase.Dead);
            }
            
            OnPhaseChanged?.Invoke(FlightPhase.Dead);
            
            Debug.Log($"[AircraftFlight] ☠️ {name} DIED! Type: {deathType}, Reason: {reason}");
            
            // Nếu là explosion thì nổ ngay lập tức
            if (deathType == DeathType.Explosion)
            {
                ExplodeAircraft();
            }
        }
        

        
        /// <summary>
        /// Kiểm tra xem máy bay đã chết chưa - PUBLIC PROPERTY
        /// </summary>
        public bool IsDead => m_isDead;
        
        /// <summary>
        /// Thời gian đã chết - PUBLIC PROPERTY
        /// </summary>
        public float TimeSinceDeath => m_isDead ? Time.time - m_deathTime : 0f;
        
        /// <summary>
        /// Nổ máy bay và deactivate
        /// </summary>
        private void ExplodeAircraft()
        {
            Debug.Log($"[AircraftFlight] 💥 {name} EXPLODED! ({m_deathType}) - Deactivating...");
            deadExplosions.Explosion();
            //gameObject.SetActive(false);
        }
        

        
        /// <summary>
        /// Kích hoạt lại máy bay và reset về trạng thái ban đầu - PUBLIC METHOD
        /// </summary>
        public void ReactivateAircraft(Vector3 newPosition, Quaternion newRotation)
        {
            // Reset death state
            m_isDead = false;
            m_deathTime = 0f;
            m_deathType = DeathType.ShotDown;
            m_currentVelocity = Vector3.zero;
            
            // Reset position và rotation
            transform.position = newPosition;
            transform.rotation = newRotation;
            
            // Reset trajectory
            m_flightTrajectory.Clear();
            
            // Reset components
            rotationController?.ResetBanking();
            
            // Reactivate GameObject
            gameObject.SetActive(true);
            
            // Reinitialize và start flight pattern
            if (Application.isPlaying)
            {
                InitializeComponents();
                StartFlightPattern();
            }
            
            Debug.Log($"[AircraftFlight] ✨ {name} REACTIVATED at {newPosition}!");
        }
        #endregion
        
        #region Debug & Visualization
        /// <summary>
        /// Vẽ debug gizmos trong Scene view - COMPONENT DEBUG
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!m_showDebugGizmos) return;
            
            DrawFlightPaths();
            DrawCurrentTarget();
            DrawAttackRanges();
            DrawTrajectory();
            DrawComponentDebugInfo();
        }
        
        /// <summary>
        /// Vẽ tất cả flight paths có thể
        /// </summary>
        private void DrawFlightPaths()
        {
            if (!m_showAllFlightPaths || m_pointGroup == null) return;
            
            m_pointGroup.UpdatePoints();
            
            // Main path - Cyan
            DrawPathLine(m_pointGroup.points, Color.cyan, "MAIN");
            
            // Attack path - Red
            DrawPathLine(m_pointGroup.attackPoints, Color.red, "ATTACK");
            
            // Left exit path - Blue
            DrawPathLine(m_pointGroup.leftPoints, Color.blue, "LEFT EXIT");
            
            // Right exit path - Magenta
            DrawPathLine(m_pointGroup.rightPoints, Color.magenta, "RIGHT EXIT");
        }
        
        /// <summary>
        /// Vẽ đường nối giữa các waypoints
        /// </summary>
        private void DrawPathLine(List<Transform> waypoints, Color color, string pathName)
        {
            if (waypoints == null || waypoints.Count < 2) return;
            
            Gizmos.color = color;
            
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
            
            // Vẽ sphere cho từng waypoint
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, m_gizmoSize);
                    
                    // Debug labels
                    #if UNITY_EDITOR
                    if (m_showDebugLabels)
                    {
                        UnityEditor.Handles.Label(waypoints[i].position + Vector3.up * 2f, 
                            $"{pathName}[{i}]: {waypoints[i].name}");
                    }
                    #endif
                }
            }
        }
        
        /// <summary>
        /// Vẽ target hiện tại và hướng bay
        /// </summary>
        private void DrawCurrentTarget()
        {
            if (pathManager?.CurrentTarget == null) return;
            
            Vector3 targetPos = pathManager.CurrentTarget.position;
            Vector3 currentPos = transform.position;
            
            // Target sphere - Yellow
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(targetPos, m_gizmoSize * 1.5f);
            
            // Line từ aircraft tới target - Green
            Gizmos.color = Color.green;
            Gizmos.DrawLine(currentPos, targetPos);
            
            // Current position - White
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(currentPos, m_gizmoSize * 0.8f);
            
            // Debug label
            #if UNITY_EDITOR
            if (m_showDebugLabels)
            {
                string debugText = $"Target: {pathManager.CurrentTarget.name}\n" +
                                 $"Phase: {pathManager.CurrentPhase}\n" +
                                 $"Point: {pathManager.CurrentPointIndex}/{pathManager.CurrentWaypoints?.Count}\n" +
                                 $"Distance: {Vector3.Distance(currentPos, targetPos):F1}m";
                
                UnityEditor.Handles.Label(currentPos + Vector3.up * 3f, debugText);
            }
            #endif
        }
        
        /// <summary>
        /// Vẽ attack ranges
        /// </summary>
        private void DrawAttackRanges()
        {
            if (m_pointGroup?.attackPoints == null) return;
            
            Gizmos.color = Color.red;
            foreach (Transform attackPoint in m_pointGroup.attackPoints)
            {
                if (attackPoint != null)
                {
                    // Attack range circle
                    Gizmos.DrawWireSphere(attackPoint.position, m_attackRange);
                }
            }
        }
        
        /// <summary>
        /// Vẽ flight trajectory
        /// </summary>
        private void DrawTrajectory()
        {
            if (!m_showFlightTrajectory || m_flightTrajectory.Count < 2) return;
            
            for (int i = 0; i < m_flightTrajectory.Count - 1; i++)
            {
                // Fade effect - càng cũ càng mờ
                float alpha = (float)i / m_flightTrajectory.Count;
                Gizmos.color = new Color(1f, 0.5f, 0f, alpha * 0.8f); // Orange fading
                
                Gizmos.DrawLine(m_flightTrajectory[i], m_flightTrajectory[i + 1]);
            }
        }
        
        /// <summary>
        /// Vẽ component debug info
        /// </summary>
        private void DrawComponentDebugInfo()
        {
            if (!Application.isPlaying || !m_showMovementDirection) return;
            
            Vector3 pos = transform.position;
            
            // Flight direction - Red
            Gizmos.color = Color.red;
            Gizmos.DrawRay(pos, transform.forward * 5f);
            
            // Target direction from rotation controller - Blue
            if (rotationController?.CurrentTargetDirection != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(pos, rotationController.CurrentTargetDirection * 4f);
            }
            
            // Death visualization
            if (m_isDead)
            {
                // Death indicator - Red X
                Gizmos.color = Color.red;
                Vector3 cross1Start = pos + Vector3.up + Vector3.left;
                Vector3 cross1End = pos + Vector3.up + Vector3.right;
                Vector3 cross2Start = pos + Vector3.up + Vector3.forward;
                Vector3 cross2End = pos + Vector3.up + Vector3.back;
                Gizmos.DrawLine(cross1Start, cross1End);
                Gizmos.DrawLine(cross2Start, cross2End);
                
                // Fall velocity indicator
                Gizmos.color = Color.yellow;
                Vector3 fallDir = m_currentVelocity.normalized;
                if (fallDir.magnitude > 0.1f)
                {
                    Gizmos.DrawRay(pos, fallDir * 4f);
                }
                
                #if UNITY_EDITOR
                if (m_showDebugLabels)
                {
                    UnityEditor.Handles.Label(pos + Vector3.up * 4f, 
                        $"DEAD - {TimeSinceDeath:F1}s\n" +
                        $"Fall Speed: {m_fallSpeed}\n" +
                        $"Spin Speed: {m_deathSpinSpeed:F0}\u00b0/s");
                }
                #endif
            }
            // Banking visualization (chỉ khi không chết)
            else if (rotationController != null && Mathf.Abs(rotationController.CurrentBankAngle) > 1f)
            {
                Gizmos.color = Color.cyan;
                Vector3 bankingDir = transform.right * (rotationController.CurrentBankAngle > 0 ? 1 : -1);
                Gizmos.DrawRay(pos + Vector3.up, bankingDir * 3f);
                
                #if UNITY_EDITOR
                if (m_showDebugLabels)
                {
                    UnityEditor.Handles.Label(pos + Vector3.up * 4f, 
                        $"Banking: {rotationController.CurrentBankAngle:F1}\u00b0\n" +
                        $"Strategic: {rotationController.StrategicBankAngle:F1}\u00b0");
                }
                #endif
            }
        }
        
        /// <summary>
        /// Reset tất cả components - gọi từ Context Menu
        /// </summary>
        [ContextMenu("Reset All Components")]
        public void ResetAllComponents()
        {
            Debug.Log("[AircraftFlight] Resetting all components...");
            
            // Reset trajectory
            m_flightTrajectory.Clear();
            
            // Reset rotation controller
            rotationController?.ResetBanking();
            
            // Reinitialize if playing
            if (Application.isPlaying)
            {
                InitializeComponents();
                StartFlightPattern();
                Debug.Log("[AircraftFlight] All components reset and reinitialized!");
            }
        }
        
        /// <summary>
        /// TEST: Kill aircraft - gọi từ Context Menu
        /// </summary>
        [ContextMenu("☠️ Test Death - Shot Down")]
        public void TestDeath_ShotDown()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[AircraftFlight] Test chỉ hoạt động khi game đang chạy!");
                return;
            }
            
            Die(DeathType.ShotDown, "Shot down by player");
        }
        
        /// <summary>
        /// TEST: Kill aircraft with explosion - gọi từ Context Menu
        /// </summary>
        [ContextMenu("💥 Test Death - Explosion")]
        public void TestDeath_Explosion()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[AircraftFlight] Test chỉ hoạt động khi game đang chạy!");
                return;
            }
            
            // Explosion nổ ngay lập tức, không cần rơi
            Die(DeathType.Explosion, "Exploded by missile");
        }
        
        /// <summary>
        /// TEST: Kill aircraft with slow crash - gọi từ Context Menu
        /// </summary>
        [ContextMenu("🚑 Test Death - Engine Failure")]
        public void TestDeath_EngineFailure()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[AircraftFlight] Test chỉ hoạt động khi game đang chạy!");
                return;
            }
            
            Die(DeathType.EngineFailure, "Engine failure");
        }
        
        #endregion
    }
}

