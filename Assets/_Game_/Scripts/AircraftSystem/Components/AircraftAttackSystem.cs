using UnityEngine;
using System.Collections.Generic;

namespace AircraftSystem.Components
{
    /// <summary>
    /// Aircraft Attack System - tách logic attack và triggers từ main controller
    /// Giữ nguyên toàn bộ logic attack như cũ, chỉ tách ra để dễ đọc và maintain
    /// </summary>
    public class AircraftAttackSystem
    {
        #region Settings (từ main controller)
        public float attackRange = 3f;
        public int AttackPoint = 0;
        #endregion
        
        #region Runtime Data
        private HashSet<Transform> triggeredAttacks = new HashSet<Transform>();
        private float lastAttackTime = 0f;
        private AircraftFlightController.FlightPhase lastPhase = AircraftFlightController.FlightPhase.MainPath;
        #endregion
        
        #region Main Controller Reference
        private Transform aircraftTransform;
        private AircraftFlightController mainController;
        private PointGroup pointGroup;
        #endregion
        
        #region Events
        /// <summary>Event kích hoạt khi máy bay trigger tấn công tại một attack point</summary>
        public System.Action<Transform> OnAttackTriggered;
        #endregion
        
        /// <summary>
        /// Constructor - nhận reference từ main controller
        /// </summary>
        public AircraftAttackSystem(AircraftFlightController controller, Transform transform, PointGroup points, int attackPoint)
        {
            mainController = controller;
            aircraftTransform = transform;
            pointGroup = points;
            AttackPoint = attackPoint;
        }
        
        /// <summary>
        /// Check attack triggers - TỰ ĐỘNG RESET KHI PHASE THAY ĐỔI
        /// </summary>
        public void CheckAttackTriggers(AircraftFlightController.FlightPhase currentPhase)
        {
            // Tự động reset triggered attacks khi chuyển sang attack phase mới
            if (currentPhase != lastPhase && currentPhase == AircraftFlightController.FlightPhase.Attack)
            {
                ResetTriggeredAttacks();
                lastPhase = currentPhase;
            }
            
            // Chỉ trigger attack khi đang bay qua attack points
            if (currentPhase != AircraftFlightController.FlightPhase.Attack) 
            {
                lastPhase = currentPhase; // Cập nhật phase nhưng không attack
                return;
            }

            if (pointGroup == null) return;
            
            pointGroup.UpdatePoints();
            
            // ✅ FIX: Chỉ trigger 1 lần tại attack point được chọn
            if (AttackPoint >= 0 && AttackPoint < pointGroup.attackPoints.Count)
            {
                var selectedAttackPoint = pointGroup.attackPoints[AttackPoint];
                
                if (selectedAttackPoint != null && !triggeredAttacks.Contains(selectedAttackPoint))
                {
                    var distance = Vector3.Distance(aircraftTransform.position, selectedAttackPoint.position);
                    if (distance <= attackRange)
                    {
                        TriggerAttack(selectedAttackPoint); // Chỉ gọi 1 lần
                    }
                }
            }
            // foreach (Transform attackPoint in pointGroup.attackPoints)
            // {
            //     if (attackPoint == null || triggeredAttacks.Contains(attackPoint)) continue;
            //     
            //     float distance = Vector3.Distance(aircraftTransform.position, attackPoint.position);
            //     if (distance <= attackRange)
            //     {
            //         TriggerAttack(attackPoint);
            //         break; // Chỉ trigger 1 attack mỗi lần
            //     }
            // }
        }
        
        /// <summary>
        /// Trigger attack - TẤN CÔNG PLAYER!
        /// </summary>
        private void TriggerAttack(Transform attackPoint)
        {
            triggeredAttacks.Add(attackPoint);
            lastAttackTime = Time.time;
            
            OnAttackTriggered?.Invoke(attackPoint);
            
            Debug.Log($"[AircraftAttack] ⚔️ ATTACK TRIGGERED at {attackPoint.name}! Player hit! Total triggered: {triggeredAttacks.Count}");
        }
        
        /// <summary>
        /// Reset triggered attacks - CHO PHÉP TẤN CÔNG LẠI
        /// </summary>
        public void ResetTriggeredAttacks()
        {
            triggeredAttacks.Clear();
            Debug.Log($"[AircraftAttack] Ready to attack again!");
        }
        
        /// <summary>
        /// Get triggered attacks count for debugging
        /// </summary>
        public int TriggeredAttacksCount => triggeredAttacks.Count;
        
        /// <summary>
        /// Get time since last attack
        /// </summary>
        public float TimeSinceLastAttack => Time.time - lastAttackTime;
        
    }
}
