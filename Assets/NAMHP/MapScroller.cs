using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapScroller : MonoBehaviour
{

    [SerializeField] private float _speed = 200;
    [Tooltip("Khi renderer đi quá giới hạn này, nó sẽ được dịch chuyển lùi lại")]
    [SerializeField] private float _zThreshold = 8000;

    [Tooltip("Khoảng cách dịch chuyển khi bound của renderer vượt qua giới hạn")]
    [SerializeField]
    private float _zShift = 12000;

    [SerializeField] private List<Renderer> _renderers;
    private Transform[] _transforms;

    private void Start()
    {
        _transforms = _renderers.Select(x => x.transform).ToArray();
    }

    private void Update()
    {
        var movement = new Vector3(0, 0, _speed * Time.unscaledDeltaTime);
        for (var i = 0; i < _renderers.Count; i++)
        {
            _transforms[i].Translate(movement, Space.World);

            var rend = _renderers[i];
            if (rend.bounds.min.z > _zThreshold)
            {
                rend.transform.Translate(0, 0, -_zShift, Space.World);
            }
        }
    }
}
