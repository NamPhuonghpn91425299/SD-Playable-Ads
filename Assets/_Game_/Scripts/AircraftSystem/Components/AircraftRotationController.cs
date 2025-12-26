using UnityEngine;
using System.Collections.Generic;

namespace AircraftSystem.Components
{
    /// <summary>
    /// Aircraft Rotation Controller - tách logic rotation và banking từ main controller
    /// Giữ nguyên toàn bộ logic rotation như cũ, chỉ tách ra để dễ đọc và maintain
    /// </summary>
    public class AircraftRotationController
    {
        #region Settings (từ main controller)
        public float rotationSpeed = 90f;
        public float lookAheadDistance = 20f;
        public float rotationSmoothness = 0.6f;
        
        // Banking settings
        public bool useBanking = true;
        public float maxBankAngle = 90f;
        public float bankingSpeed = 45f;
        public float exitBankingMultiplier = 1f;
        public float entryBankingMultiplier = 1f;
        
        // Debug
        public bool showMovementDirection = true;
        #endregion
        
        #region Runtime Data
        private Vector3 currentTargetDirection = Vector3.forward;
        private float currentBankAngle = 0f;
        private float strategicBankAngle = 0f;
        #endregion
        
        #region Main Controller Reference
        private Transform aircraftTransform;
        private AircraftFlightController mainController;
        #endregion
        
        /// <summary>
        /// Constructor - nhận reference từ main controller
        /// </summary>
        public AircraftRotationController(AircraftFlightController controller, Transform transform)
        {
            mainController = controller;
            aircraftTransform = transform;
        }
        
        /// <summary>
        /// Handle rotation logic - 
        /// </summary>
        public void HandleRotation(Vector3 currentTarget, float distanceToCurrentTarget, List<Transform> currentWaypoints, int currentPointIndex)
        {
            // Tính toán hướng look-ahead đơn giản
            Vector3 optimalLookDirection = CalculateSimpleLookAheadDirection(currentTarget, distanceToCurrentTarget, currentWaypoints, currentPointIndex);
            
            // Áp dụng rotation với banking
            ApplySimpleRotationWithBanking(optimalLookDirection);
            
            // Lưu hướng cho debug
            currentTargetDirection = optimalLookDirection;
        }
        
        /// <summary>
        /// Calculate simple look-ahead direction - LOGIC CŨ
        /// </summary>
        private Vector3 CalculateSimpleLookAheadDirection(Vector3 currentTarget, float distanceToCurrentTarget, 
            List<Transform> currentWaypoints, int currentPointIndex)
        {
            Vector3 aircraftPos = aircraftTransform.position;
            
            // Nếu không có điểm tiếp theo hoặc còn xa điểm hiện tại, chỉ nhìn điểm hiện tại
            if (currentPointIndex + 1 >= currentWaypoints.Count || distanceToCurrentTarget > lookAheadDistance)
            {
                return (currentTarget - aircraftPos).normalized;
            }
            
            Transform nextPoint = currentWaypoints[currentPointIndex + 1];
            if (nextPoint == null)
            {
                return (currentTarget - aircraftPos).normalized;
            }
            
            // Look-ahead đơn giản: khi gần điểm hiện tại thì bắt đầu nhìn về điểm tiếp theo
            float lookAheadBlend = Mathf.Clamp01((lookAheadDistance - distanceToCurrentTarget) / lookAheadDistance);
            lookAheadBlend *= 0.5f; // Giảm mức độ look-ahead để ổn định hơn
            
            Vector3 currentDirection = (currentTarget - aircraftPos).normalized;
            Vector3 nextDirection = (nextPoint.position - aircraftPos).normalized;
            
            // Blend đơn giản giữa 2 hướng
            Vector3 blendedDirection = Vector3.Slerp(currentDirection, nextDirection, lookAheadBlend);
            
            return blendedDirection.normalized;
        }
        
        /// <summary>
        /// Apply rotation với banking - LOGIC CŨ
        /// </summary>
        private void ApplySimpleRotationWithBanking(Vector3 targetDirection)
        {
            if (targetDirection == Vector3.zero || targetDirection.magnitude < 0.1f)
            {
                Debug.LogWarning("[AircraftRotation] Target direction không hợp lệ!");
                return;
            }
            
            // Đảm bảo targetDirection được normalize
            targetDirection = targetDirection.normalized;
            
            // Tính góc giữa hướng hiện tại và target
            float angleToTarget = Vector3.Angle(aircraftTransform.forward, targetDirection);
            
            // Nếu góc quá nhỏ, không cần xoay (tránh jitter)
            if (angleToTarget < 0.5f)
            {
                return;
            }
            
            // Tính target rotation
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            
            // Debug rays
            if (showMovementDirection)
            {
                Debug.DrawRay(aircraftTransform.position, targetDirection * 4f, Color.blue, 0.1f);
            }
            
            // Áp dụng banking nếu bật
            if (useBanking)
            {
                // Tính banking dựa trên góc rẽ
                Vector3 cross = Vector3.Cross(aircraftTransform.forward, targetDirection);
                float turnDirection = Vector3.Dot(cross, Vector3.up);
                
                // Giới hạn góc banking dựa trên góc rẽ
                float maxAllowedBank = Mathf.Min(maxBankAngle, angleToTarget * 0.8f);
                float targetBankAngle = Mathf.Clamp(turnDirection * angleToTarget * 0.3f, -maxAllowedBank, maxAllowedBank);
                
                // GIẢM tactical banking khi có strategic banking mạnh để tránh double banking
                float strategicBankingStrength = Mathf.Abs(strategicBankAngle) / maxBankAngle; // 0-1
                float tacticalBankingReduction = 1f - (strategicBankingStrength * 0.7f); // Giảm tối đa 70%
                targetBankAngle *= tacticalBankingReduction;
                
                // Smooth banking transition
                currentBankAngle = Mathf.Lerp(currentBankAngle, targetBankAngle, bankingSpeed * Time.deltaTime);
                
                // Áp dụng banking vào rotation (kết hợp tactical + strategic)
                float totalBank = currentBankAngle + strategicBankAngle;
                targetRotation *= Quaternion.AngleAxis(totalBank, Vector3.forward);
            }
            
            // Áp dụng rotation mượt - tối ưu hóa
            float rotationSpeedDelta = rotationSpeed * Time.deltaTime;
            
            if (rotationSmoothness > 0.1f)
            {
                // Slerp cho rotation mượt
                float smoothFactor = (1f - rotationSmoothness * 0.8f); // Giảm ảnh hưởng smoothness
                float slerpSpeed = rotationSpeedDelta / 90f * smoothFactor;
                slerpSpeed = Mathf.Clamp(slerpSpeed, 0.01f, 1f); // Giới hạn tốc độ slerp
                
                aircraftTransform.rotation = Quaternion.Slerp(aircraftTransform.rotation, targetRotation, slerpSpeed);
            }
            else
            {
                // RotateTowards cho responsive
                aircraftTransform.rotation = Quaternion.RotateTowards(aircraftTransform.rotation, targetRotation, rotationSpeedDelta);
            }
        }
        
        /// <summary>
        /// Update strategic banking - LOGIC CŨ
        /// </summary>
        public void UpdateStrategicBanking(AircraftFlightController.FlightPhase currentPhase, List<Transform> currentWaypoints, 
            int currentPointIndex, int loopCount, AircraftFlightController.ExitChoice exitChoice, bool isAlternateLeft)
        {
            if (!useBanking || currentWaypoints == null || currentWaypoints.Count == 0)
            {
                strategicBankAngle = 0f;
                return;
            }
            
            float targetStrategicBank = 0f;
            
            // Kiểm tra nếu đang ở điểm cuối của phase hiện tại
            bool isNearPhaseEnd = (currentPointIndex >= currentWaypoints.Count - 1) || 
                                  (currentPointIndex == currentWaypoints.Count - 2);
            
            if (isNearPhaseEnd)
            {
                // Xác định banking dựa trên phase hiện tại và phase tiếp theo
                switch (currentPhase)
                {
                    case AircraftFlightController.FlightPhase.Attack:
                        // Khi ở attack -> giữ banking theo hướng sắp rẽ
                        targetStrategicBank = GetConsistentBankingAngle(exitChoice, isAlternateLeft);
                        break;
                        
                    case AircraftFlightController.FlightPhase.LeftExit:
                        // Sau left exit -> banking phải để lượn vào attack (ngược lại)
                        targetStrategicBank = maxBankAngle * 0.6f * entryBankingMultiplier;
                        break;
                        
                    case AircraftFlightController.FlightPhase.RightExit:
                        // Sau right exit -> banking trái để lượn vào attack (ngược lại)
                        targetStrategicBank = -maxBankAngle * 0.6f * entryBankingMultiplier;
                        break;
                        
                    case AircraftFlightController.FlightPhase.MainPath:
                        // MainPath -> Attack, không cần banking đặc biệt
                        targetStrategicBank = 0f;
                        break;
                }
            }
            else
            {
                // Logic banking cho tất cả phase (không chỉ cuối phase)
                switch (currentPhase)
                {
                    case AircraftFlightController.FlightPhase.LeftExit:
                        // Trong left exit -> luôn banking phải để lượn vào attack
                        targetStrategicBank = maxBankAngle * 0.6f * entryBankingMultiplier;
                        break;
                        
                    case AircraftFlightController.FlightPhase.RightExit:
                        // Trong right exit -> luôn banking trái để lượn vào attack
                        targetStrategicBank = -maxBankAngle * 0.6f * entryBankingMultiplier;
                        break;
                        
                    case AircraftFlightController.FlightPhase.Attack:
                        // Trong attack -> banking hết khi đến attack point đầu tiên
                        targetStrategicBank = CalculateAttackPhaseBanking(currentWaypoints, currentPointIndex, exitChoice, isAlternateLeft, loopCount);
                        break;
                        
                    default:
                        targetStrategicBank = 0f;
                        break;
                }
            }
            
            // Smooth transition cho strategic banking
            float bankingTransitionSpeed = bankingSpeed * 0.5f; // Chậm hơn để mượt
            strategicBankAngle = Mathf.Lerp(strategicBankAngle, targetStrategicBank, bankingTransitionSpeed * Time.deltaTime);
        }
        
        /// <summary>
        /// Get consistent banking angle - LOGIC CŨ
        /// </summary>
        private float GetConsistentBankingAngle(AircraftFlightController.ExitChoice exitChoice, bool isAlternateLeft)
        {
            // Dự đoán hướng exit tiếp theo
            bool willUseLeft = false;
            
            switch (exitChoice)
            {
                case AircraftFlightController.ExitChoice.Left:
                    willUseLeft = true;
                    break;
                case AircraftFlightController.ExitChoice.Right:
                    willUseLeft = false;
                    break;
                case AircraftFlightController.ExitChoice.Alternate:
                    willUseLeft = isAlternateLeft;
                    break;
                case AircraftFlightController.ExitChoice.Random:
                    // Random thì không thể dự đoán, banking nhẹ thôi
                    return Random.Range(-maxBankAngle * 0.3f, maxBankAngle * 0.3f);
            }
            
            // Trả về góc banking chuẩn bị rẽ exit
            if (willUseLeft)
            {
                return -maxBankAngle * 0.8f * exitBankingMultiplier; // Rẽ trái = banking trái mạnh
            }
            else
            {
                return maxBankAngle * 0.8f * exitBankingMultiplier;  // Rẽ phải = banking phải mạnh
            }
        }
        
        /// <summary>
        /// Calculate attack phase banking - LOGIC CŨ
        /// </summary>
        private float CalculateAttackPhaseBanking(List<Transform> currentWaypoints, int currentPointIndex,
            AircraftFlightController.ExitChoice exitChoice, bool isAlternateLeft, int loopCount)
        {
            if (currentWaypoints == null || currentWaypoints.Count == 0) return 0f;
            
            // Kiểm tra khoảng cách đến attack point đầu tiên
            Transform firstAttackPoint = currentWaypoints[0];
            if (firstAttackPoint == null) return 0f;
            
            float distanceToFirstAttack = Vector3.Distance(aircraftTransform.position, firstAttackPoint.position);
            
            // Xác định banking ban đầu dựa trên exit phase trước đó
            float initialBanking = GetInitialAttackBanking(loopCount, exitChoice, isAlternateLeft);
            
            // Kiểm tra nếu đã đến attack point đầu tiên
            if (currentPointIndex > 0 || distanceToFirstAttack <= mainController.ArrivalDistance * 2f)
            {
                // Đã đến/qua attack point đầu tiên -> banking = 0
                return 0f;
            }
            
            // Vẫn chưa đến attack point đầu tiên -> giữ banking từ exit phase
            return initialBanking;
        }
        
        /// <summary>
        /// Get initial attack banking - LOGIC CŨ
        /// </summary>
        private float GetInitialAttackBanking(int loopCount, AircraftFlightController.ExitChoice exitChoice, bool isAlternateLeft)
        {
            // Dựa trên loop count để biết đã đi qua exit phase nào
            if (loopCount == 0) 
            {
                // Lần đầu tiên chưa có exit
                return 0f;
            }
            
            // Dựa trên exit choice để đoán exit phase trước đó
            bool wasFromLeftExit = false;
            
            switch (exitChoice)
            {
                case AircraftFlightController.ExitChoice.Left:
                    wasFromLeftExit = true;
                    break;
                case AircraftFlightController.ExitChoice.Right:
                    wasFromLeftExit = false;
                    break;
                case AircraftFlightController.ExitChoice.Alternate:
                    // Alternate thì ngược lại với lần tiếp theo
                    wasFromLeftExit = !isAlternateLeft;
                    break;
                case AircraftFlightController.ExitChoice.Random:
                    // Random thì không biết, trả về 0
                    return 0f;
            }
            
            // Trả về banking tương ứng với exit phase trước đó
            if (wasFromLeftExit)
            {
                // Từ LeftExit -> banking phải
                return maxBankAngle * 0.6f * entryBankingMultiplier;
            }
            else
            {
                // Từ RightExit -> banking trái
                return -maxBankAngle * 0.6f * entryBankingMultiplier;
            }
        }
        
        /// <summary>
        /// Reset banking - LOGIC CŨ
        /// </summary>
        public void ResetBanking()
        {
            currentBankAngle = 0f;
            strategicBankAngle = 0f;
        }
        
        /// <summary>
        /// Get current target direction for debug
        /// </summary>
        public Vector3 CurrentTargetDirection => currentTargetDirection;
        
        /// <summary>
        /// Get current bank angle for debug
        /// </summary>
        public float CurrentBankAngle => currentBankAngle;
        
        /// <summary>
        /// Get strategic bank angle for debug
        /// </summary>
        public float StrategicBankAngle => strategicBankAngle;
    }
}
