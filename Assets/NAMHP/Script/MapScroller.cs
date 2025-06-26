//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class MapScroller : MonoBehaviour
//{

//    [SerializeField] private float _speed = 200;
//    [Tooltip("Khi renderer đi quá giới hạn này, nó sẽ được dịch chuyển lùi lại")]
//    [SerializeField] private float _zThreshold = 8000;

//    [Tooltip("Khoảng cách dịch chuyển khi bound của renderer vượt qua giới hạn")]
//    [SerializeField]
//    private float _zShift = 12000;

//    [SerializeField] private List<Renderer> _renderers;
//    private Transform[] _transforms;

//    private void Start()
//    {
//        _transforms = _renderers.Select(x => x.transform).ToArray();
//    }

//    private void Update()
//    {
//        var movement = new Vector3(0, 0, _speed * Time.deltaTime);
//        for (var i = 0; i < _renderers.Count; i++)
//        {
//            if (_transforms[i] == null || _renderers[i] == null)
//            {
//                Debug.LogError($"Renderer or Transform at index {i} is null!");
//                continue;
//            }

//            _transforms[i].Translate(movement, Space.World);

//            var rend = _renderers[i];
//            if (rend.bounds.min.z > _zThreshold)
//            {
//                rend.transform.Translate(0, 0, -_zShift, Space.World);
//            }
//        }
//    }

//}
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
