using System.Collections.Generic;
using UnityEngine;

public class MapScroller : MonoBehaviour
{
    [SerializeField] private float _speed = 200f;
    [Tooltip("Khi đối tượng vượt qua giới hạn Z này, nó sẽ được dịch chuyển lại")]
    [SerializeField] private float _zThreshold = 8000f;
    [Tooltip("Khoảng cách đối tượng được dịch chuyển lại phía sau")]
    [SerializeField] private float _zShift = 12000f;
    [SerializeField] private List<Transform> _objectsToScroll;

    private void Update()
    {
        // Tính toán khoảng di chuyển dựa trên tốc độ
        Vector3 movement = new Vector3(0, 0, _speed * Time.timeScale);

        // Duyệt qua các đối tượng cần scroll
        foreach (var obj in _objectsToScroll)
        {
            if (obj == null) continue;

            // Dịch chuyển đối tượng
            obj.Translate(movement, Space.World);

            // Kiểm tra vị trí Z của đối tượng
            if (obj.position.z > _zThreshold)
            {
                obj.Translate(0, 0, -_zShift, Space.World);
            }
        }

    }
}