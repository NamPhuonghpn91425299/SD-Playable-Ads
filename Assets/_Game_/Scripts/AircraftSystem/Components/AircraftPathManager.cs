using UnityEngine;
using System.Collections.Generic;

namespace AircraftSystem.Components
{
    /// <summary>
    /// Aircraft Path Manager - tách logic path và phase management từ main controller
    /// Giữ nguyên toàn bộ logic path như cũ, chỉ tách ra để dễ đọc và maintain
    /// </summary>
    public class AircraftPathManager
    {
        #region Settings (từ main controller)
        public AircraftFlightController.ExitChoice exitChoice = AircraftFlightController.ExitChoice.Random;
        #endregion
        
        #region Runtime Data
        private AircraftFlightController.FlightPhase currentPhase = AircraftFlightController.FlightPhase.MainPath;
        private int currentPointIndex = 0;
        private List<Transform> currentWaypoints;
        private int loopCount = 0;
        private bool hasCompletedMainPath = false;
        private bool isAlternateLeft = true;
        #endregion
        
        #region Main Controller Reference
        private AircraftFlightController mainController;
        private PointGroup pointGroup;
        #endregion
        
        #region Events
        /// <summary>Event kích hoạt khi máy bay chuyển đổi giai đoạn bay</summary>
        public System.Action<AircraftFlightController.FlightPhase> OnPhaseChanged;
        #endregion
        
        /// <summary>
        /// Constructor - nhận reference từ main controller
        /// </summary>
        public AircraftPathManager(AircraftFlightController controller, PointGroup points)
        {
            mainController = controller;
            pointGroup = points;
            currentWaypoints = new List<Transform>();
        }
        
        /// <summary>
        /// Start main phase - LOGIC CŨ
        /// </summary>
        public void StartMainPhase()
        {
            if (pointGroup == null)
            {
                Debug.LogError("[AircraftPath] PointGroup is not assigned!");
                return;
            }
            
            pointGroup.UpdatePoints();
            currentWaypoints.Clear();
            currentWaypoints.AddRange(pointGroup.points);
            currentPhase = AircraftFlightController.FlightPhase.MainPath;
            currentPointIndex = 0;
            
            OnPhaseChanged?.Invoke(currentPhase);
            
            //Debug.Log($"[AircraftPath] Started MainPath with {currentWaypoints.Count} waypoints");
        }
        
        /// <summary>
        /// Check if reached path end và handle transitions - LOGIC CŨ
        /// </summary>
        public void CheckPathCompletion()
        {
            if (currentPointIndex >= currentWaypoints.Count)
            {
                OnReachPathEnd();
            }
        }
        
        /// <summary>
        /// On reach path end - LOGIC CŨ
        /// </summary>
        private void OnReachPathEnd()
        {
            switch (currentPhase)
            {
                case AircraftFlightController.FlightPhase.MainPath:
                    // Lần đầu: waypoints -> attack
                    hasCompletedMainPath = true;
                    StartAttackPhase();
                    break;
                    
                case AircraftFlightController.FlightPhase.Attack:
                    // Từ attack -> chọn left/right
                    ChooseExitPath();
                    break;
                    
                case AircraftFlightController.FlightPhase.LeftExit:
                case AircraftFlightController.FlightPhase.RightExit:
                    // Từ left/right -> quay lại attack (đơn giản)
                    loopCount++;
                    StartAttackPhase(); // Trực tiếp về Attack phase
                    break;
                    
                case AircraftFlightController.FlightPhase.Dead:
                default:
                    Debug.Log($"[AircraftPath] Bot died or completed!");
                    break;
            }
        }
        
        /// <summary>
        /// Start attack phase - LOGIC CŨ + RESET ATTACK TRIGGERS
        /// </summary>
        private void StartAttackPhase()
        {
            if (pointGroup == null || pointGroup.attackPoints.Count == 0)
            {
                Debug.LogWarning("[AircraftPath] No attack points found!");
                currentPhase = AircraftFlightController.FlightPhase.Dead;
                return;
            }
            
            pointGroup.UpdatePoints();
            
            // BAY QUA TẤT CẢ ATTACK POINTS - ĐƠN GIẢN
            currentWaypoints.Clear();
            currentWaypoints.AddRange(pointGroup.attackPoints);
            currentPhase = AircraftFlightController.FlightPhase.Attack; // Luôn là Attack
            currentPointIndex = 0;
            
            OnPhaseChanged?.Invoke(currentPhase);
            
            //var loopText = loopCount > 0 ? $" (loop {loopCount})" : "";
            //Debug.Log($"[AircraftPath] Started Attack phase with {currentWaypoints.Count} attack points{loopText}");
        }
        
        
        /// <summary>
        /// Choose exit path - LOGIC CŨ
        /// </summary>
        private void ChooseExitPath()
        {
            if (pointGroup == null)
            {
                Debug.LogWarning("[AircraftPath] PointGroup is null!");
                return;
            }
            
            pointGroup.UpdatePoints();
            
            bool useLeft = false;
            
            switch (exitChoice)
            {
                case AircraftFlightController.ExitChoice.Left:
                    useLeft = true;
                    break;
                case AircraftFlightController.ExitChoice.Right:
                    useLeft = false;
                    break;
                case AircraftFlightController.ExitChoice.Alternate:
                    useLeft = isAlternateLeft;
                    isAlternateLeft = !isAlternateLeft; // Đổi cho lần sau
                    break;
                case AircraftFlightController.ExitChoice.Random:
                    useLeft = Random.Range(0, 2) == 0;
                    break;
            }
            
            if (useLeft && pointGroup.leftPoints.Count > 0)
            {
                StartLeftExitPhase();
            }
            else if (!useLeft && pointGroup.rightPoints.Count > 0)
            {
                StartRightExitPhase();
            }
            else
            {
                // Fallback nếu không có exit points
                Debug.LogWarning("[AircraftPath] No suitable exit points found! Using available option.");
                if (pointGroup.leftPoints.Count > 0)
                {
                    StartLeftExitPhase();
                }
                else if (pointGroup.rightPoints.Count > 0)
                {
                    StartRightExitPhase();
                }
                else
                {
                    Debug.LogError("[AircraftPath] No exit points available!");
                    currentPhase = AircraftFlightController.FlightPhase.Dead;
                }
            }
        }
        
        /// <summary>
        /// Start left exit phase - LOGIC CŨ
        /// </summary>
        private void StartLeftExitPhase()
        {
            currentWaypoints.Clear();
            currentWaypoints.AddRange(pointGroup.leftPoints);
            currentPhase = AircraftFlightController.FlightPhase.LeftExit;
            currentPointIndex = 0;
            
            OnPhaseChanged?.Invoke(currentPhase);
            
            //Debug.Log($"[AircraftPath] Started LeftExit phase with {currentWaypoints.Count} points");
        }
        
        /// <summary>
        /// Start right exit phase - LOGIC CŨ
        /// </summary>
        private void StartRightExitPhase()
        {
            currentWaypoints.Clear();
            currentWaypoints.AddRange(pointGroup.rightPoints);
            currentPhase = AircraftFlightController.FlightPhase.RightExit;
            currentPointIndex = 0;
            
            OnPhaseChanged?.Invoke(currentPhase);
            
            //Debug.Log($"[AircraftPath] Started RightExit phase with {currentWaypoints.Count} points");
        }
        
        /// <summary>
        /// Advance to next waypoint - LOGIC CŨ
        /// </summary>
        public void AdvanceToNextWaypoint()
        {
            currentPointIndex++;
            //Debug.Log($"[AircraftPath] Reached point {currentPointIndex - 1}");
        }
        
        #region Public Properties
        public AircraftFlightController.FlightPhase CurrentPhase => currentPhase;
        public int CurrentPointIndex => currentPointIndex;
        public List<Transform> CurrentWaypoints => currentWaypoints;
        public Transform CurrentTarget => (currentWaypoints != null && currentPointIndex < currentWaypoints.Count) 
            ? currentWaypoints[currentPointIndex] : null;
        public int LoopCount => loopCount;
        public bool HasCompletedMainPath => hasCompletedMainPath;
        public bool IsAlternateLeft => isAlternateLeft;
        
        /// <summary>
        /// Set phase trực tiếp - dùng cho death logic
        /// </summary>
        public void SetPhase(AircraftFlightController.FlightPhase newPhase)
        {
            currentPhase = newPhase;
            Debug.Log($"[AircraftPath] Phase forced to: {newPhase}");
        }
        #endregion
    }
}
