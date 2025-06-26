using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [System.Serializable]
public class AircraftMovementSettings
{
        [Header("Settings Speed")]
        public float movementSpeed = 500f;     // Tốc độ di chuyển
        public float endSlowdownDistance = 100f; // Khoảng cách bắt đầu giảm tốc
        public float minSpeedPercent = 0.01f;    // Phần trăm tốc độ tối thiểu

        [Header("Settings Rotation")]
        public float rotationSpeed = 10f;       // Tốc độ xoay
        public float bankingStrength = 15f;     // Độ mạnh của góc nghiêng
        public float pitchStrength = 1.1f;        // Độ mạnh của góc pitch
        public float smoothRotationSpeed = 6f;  // Tốc độ làm mượt xoay
        public float maxBankAngle = 90f;        // Góc nghiêng tối đa
        public float maxPitchAngle = 30f;       // Góc pitch tối đa

        [Header("Settings Direction Forward")]
        public float lookAheadDistance = 50f;   // Khoảng cách nhìn trước
        public int predictionSteps = 100;        // Số bước dự đoán

        [Header("Settings Bezeir")]
        [SerializeField] public float controlPointOffset = -0.4f;
}


