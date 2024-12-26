using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lớp chứa các thông số cấu hình cho chuyển động
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

// Lớp xử lý xoay của máy bay
public class AircraftRotationHandler
{
    private readonly AircraftMovementSettings settings;
    private Quaternion targetRotation;      // Góc xoay mục tiêu
    private Vector3 smoothedDirection;       // Hướng di chuyển đã làm mượt

    public AircraftRotationHandler(AircraftMovementSettings settings)
    {
        this.settings = settings;
        smoothedDirection = Vector3.zero;
    }

    // Cập nhật góc xoay của máy bay
    public void UpdateRotation(Transform aircraft, Vector3 currentPosition, Vector3 futureDirection)
    {
        // Làm mượt hướng di chuyển
        smoothedDirection = Vector3.Slerp(smoothedDirection, futureDirection, 
            Time.deltaTime * settings.smoothRotationSpeed);

        if (smoothedDirection != Vector3.zero)
        {
            // Tính góc nghiêng (banking)
            Vector3 right = Vector3.Cross(Vector3.up, smoothedDirection).normalized;
            float turnRate = Vector3.Dot(right, futureDirection);
            float bankAngle = -turnRate * settings.bankingStrength * settings.maxBankAngle;
            bankAngle = Mathf.Clamp(bankAngle, -settings.maxBankAngle, settings.maxBankAngle);

            // Tính góc pitch (ngẩng lên/cúi xuống)
            float pitchAngle = Mathf.Asin(smoothedDirection.y) * Mathf.Rad2Deg;
            pitchAngle = Mathf.Clamp(pitchAngle * settings.pitchStrength, 
                -settings.maxPitchAngle, settings.maxPitchAngle);

            // Tạo và áp dụng góc xoay
            Quaternion directionRotation = Quaternion.LookRotation(smoothedDirection);
            Quaternion bankRotation = Quaternion.Euler(pitchAngle, 0, bankAngle);
            targetRotation = directionRotation * bankRotation;

            // Áp dụng xoay với làm mượt
            aircraft.rotation = Quaternion.Slerp(aircraft.rotation, targetRotation, 
                Time.deltaTime * settings.rotationSpeed);
        }
    }

    // Xử lý nhìn vào mục tiêu
    public void LookAtTarget(Transform aircraft, Vector3 targetPosition, float rotationSpeed)
    {
        Vector3 directionToTarget = (targetPosition - aircraft.position).normalized;
        // Tính góc nghiêng khi nhìn vào mục tiêu
        float bankAngle = Vector3.Dot(aircraft.right, directionToTarget) * 
            settings.maxBankAngle * 0.5f;
        
        Quaternion targetLookRotation = Quaternion.LookRotation(directionToTarget) * 
            Quaternion.Euler(0, 0, bankAngle);
        
        // Áp dụng xoay với làm mượt
        aircraft.rotation = Quaternion.Slerp(
            aircraft.rotation,
            targetLookRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}
