using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeAnyThings : MonoBehaviour
{
    [SerializeField] Transform rotaBody;
    [SerializeField] float offsetY = 0f;
    [Header("Cường độ rung")]
    [SerializeField] float shakeIntensity = 0.6f;
    [Header("Tốc độ rung")]
    [SerializeField] float shakeSpeed = 4f;
    [Header("Hệ số xoay")]
    [SerializeField] float mutiRotaZ = 4f;
    [SerializeField] float mutiRotaX = 2f;
    [Header("Hệ số độ rung")]
    [SerializeField] float bonusPower = 1f;

    Transform mytrans;
    float time;
    bool isShaking = true;

    private void OnEnable()
    {
        mytrans = transform;
        offsetY = mytrans.localPosition.y;
        time = 0f;
        isShaking = true;
    }

    void Update()
    {
        if (!isShaking) return;

        time += Time.deltaTime * shakeSpeed;
        
        // Tạo hiệu ứng rung bằng hàm sin với tần số khác nhau cho mỗi trục
        float shakeX = Mathf.Sin(time) * shakeIntensity * bonusPower;
        float shakeY = Mathf.Sin(time * 1.3f) * shakeIntensity * 0.5f * bonusPower;
        float shakeZ = Mathf.Sin(time * 0.7f) * shakeIntensity * bonusPower;

        // Áp dụng vị trí rung
        mytrans.localPosition = new Vector3(shakeX, offsetY + shakeY, shakeZ);
        
        // Áp dụng xoay dựa trên vị trí rung
        rotaBody.localRotation = Quaternion.Euler(mutiRotaX * 0.5f * shakeX, 0, mutiRotaZ * shakeZ);
    }

    public void StopShaking()
    {
        isShaking = false;
        
        // Reset về vị trí ban đầu
        mytrans.localPosition = new Vector3(0, offsetY, 0);
        rotaBody.localRotation = Quaternion.Euler(0, 0, 0);
        
        this.enabled = false;
    }
}
