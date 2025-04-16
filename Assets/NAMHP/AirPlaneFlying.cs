using System;
using UnityEngine;

public class AirPlaneFlying : MonoBehaviour
{
    public AudioSource audioSource;
    public Transform center; // Tâm mà đối tượng quay quanh
    public float radius = 35f; // Bán kính quỹ đạo
    public float height = 8f; // Độ cao của quỹ đạo so với tâm
    public float speed = 3f; // Tốc độ quay (độ trên giây)
    public bool isOrbiting; // Trạng thái quay (true = quay, false = dừng)
    public float initialAngle = 0f; // Góc ban đầu (độ)
    private float angle = 0f; // Góc hiện tại của đối tượng trên quỹ đạo
    [SerializeField] private float segments;
    [SerializeField] private bool isEndGame = false;
#if UNITY_EDITOR
    private void OnValidate()
    {
        SetStartLocation();
    }
#endif
    private void OnEnable()
    {
        audioSource.Play();
        SetStartLocation();
    }

    void Update()
    {
        if (UIEndGame.Instance.IsShowEndGame && !isEndGame)
        {
            audioSource.Stop();
            isEndGame = true;
        }
        // Dừng hoặc tiếp tục quay khi nhấn phím Space
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isOrbiting = !isOrbiting;
        }
    }

    private void FixedUpdate()
    {
        //SetStartLocation();
        if (isOrbiting)
        {
            AirPlaneFly();
        }
    }

    private void AirPlaneFly()
    {
        // Cập nhật góc quay và giữ nó trong khoảng 0-360 độ
        angle = (angle - speed * Time.deltaTime) % 360;

        // Tính toán vị trí mới
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
        float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
        float y = height;

        // Cập nhật vị trí và hướng của đối tượng
        transform.position = center.position + new Vector3(x, y, z);
        transform.LookAt(center.position);
    }
    private void SetStartLocation()
    {
        if (center == null) return;
        // Đặt góc ban đầu
        angle = initialAngle;

        // Tính toán vị trí ban đầu dựa trên góc ban đầu
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
        float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
        float y = height;

        // Đặt vị trí ban đầu của đối tượng
        transform.position = center.position + new Vector3(x, y, z);
        // Hướng đối tượng về tâm
        transform.LookAt(center.position);
    }
    
    void OnDrawGizmos()
    {
        if (center == null) return;

        // Vẽ quỹ đạo tròn
        Gizmos.color = Color.green; // Màu xanh lá cho quỹ đạo
        //int segments = 50; // Số đoạn để vẽ vòng tròn mượt mà
        float angleStep = 360f / segments;
        Vector3 prevPoint = center.position + new Vector3(radius, height, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            Vector3 currentPoint = center.position + new Vector3(x, height, z);
            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }

        // Vẽ đường kính từ tâm đến đối tượng
        Gizmos.color = Color.magenta; // Màu đỏ cho đường kính
        Gizmos.DrawLine(center.position, transform.position);
    }
}