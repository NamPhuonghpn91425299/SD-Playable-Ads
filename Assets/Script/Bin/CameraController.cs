using UnityEngine;

public class CameraController : MonoBehaviour 
{
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxVerticalAngle = 10f;  // Góc nhìn lên xuống tối đa
    [SerializeField] private float minVerticalAngle = -50f; // Góc nhìn xuống tối đa
    [SerializeField] private float maxHorizontalAngle = 60f; // Góc xoay ngang tối đa mỗi bên

    [SerializeField] private float verticalRotation = 0f;
    [SerializeField] private float horizontalRotation = 0f;
    
    void Start()
    {
        // Khóa chuột vào giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Lấy input chuột
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Xử lý xoay camera lên xuống (quanh trục X)
        verticalRotation -= mouseY; // Đảo ngược để chuột lên = camera lên
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        // Xử lý xoay camera trái phải (quanh trục Y)
        horizontalRotation += mouseX;
 
        horizontalRotation = Mathf.Clamp(horizontalRotation, -maxHorizontalAngle, maxHorizontalAngle);

        // Áp dụng góc xoay
        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }

}