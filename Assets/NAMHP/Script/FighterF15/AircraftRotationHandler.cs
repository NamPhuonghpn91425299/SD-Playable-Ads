using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lớp xử lý xoay của máy bay
public class AircraftRotationHandler
{
    private readonly AircraftMovementSettings settings;
    public Quaternion targetRotation;      // Góc xoay mục tiêu
    public Vector3 smoothedDirection;       // Hướng di chuyển đã làm mượt

    public AircraftRotationHandler(AircraftMovementSettings settings)
    {
        this.settings = settings;
        smoothedDirection = -Vector3.forward;

    }
    
public void UpdateRotation(Transform aircraft, Vector3 currentPosition, Vector3 futureDirection)
{
    // Trường hợp đặc biệt: Time.deltaTime <= 0 hoặc hướng dự đoán không hợp lệ
    if (Time.deltaTime <= 0f || futureDirection == Vector3.zero)
    {
        //Debug.LogWarning("Invalid futureDirection or zero deltaTime");
        return;
    }

    // Chuẩn hóa futureDirection (giá trị mặc định nếu zero)
    futureDirection = futureDirection.normalized;
    if (futureDirection == Vector3.zero)
    {
        //Debug.LogWarning("Future direction is zero. Using fallback direction.");
        futureDirection = Vector3.forward; // Sử dụng hướng mặc định
    }

    // Làm mượt hướng di chuyển
    smoothedDirection = Vector3.Slerp(
        smoothedDirection,
        futureDirection,
        Mathf.Min(Time.deltaTime, 0.033f) * settings.smoothRotationSpeed
    );

    // Kiểm tra smoothedDirection
    if (smoothedDirection != Vector3.zero)
    {
        // Các tính toán xoay
        Vector3 right = Vector3.Cross(Vector3.up, smoothedDirection).normalized;
        float turnRate = Vector3.Dot(right, futureDirection);
        float bankAngle = -turnRate * settings.bankingStrength * settings.maxBankAngle;
        bankAngle = Mathf.Clamp(bankAngle, -settings.maxBankAngle, settings.maxBankAngle);

        float pitchAngle = Mathf.Asin(smoothedDirection.y) * Mathf.Rad2Deg;
        pitchAngle = Mathf.Clamp(pitchAngle * settings.pitchStrength,
            -settings.maxPitchAngle, settings.maxPitchAngle);

        Quaternion directionRotation = Quaternion.LookRotation(smoothedDirection);
        Quaternion bankRotation = Quaternion.Euler(pitchAngle, 0, bankAngle);
        targetRotation = directionRotation * bankRotation;

        float deltaTime = Mathf.Min(Time.deltaTime, 0.033f);
        aircraft.rotation = Quaternion.Slerp(
            aircraft.rotation,
            targetRotation,
            deltaTime * settings.rotationSpeed
        );
    }
    else
    {
        Debug.LogWarning("Smoothed direction is still zero after Slerp.");
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
