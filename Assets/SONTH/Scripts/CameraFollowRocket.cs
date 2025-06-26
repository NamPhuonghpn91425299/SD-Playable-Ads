using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraFollowRocket : MonoBehaviour
{
    [Header("Thời gian di chuyển camera tới target")]
    [SerializeField] private float _durationFollow = 2f;
    [SerializeField] private Transform _defaultParent;
    [Header("Foward")]
    [SerializeField] private Vector3 _customOffsetFoward;
    [SerializeField] private Vector3 _customRotationForward;
    [Header("Follow")]
    [SerializeField] private Vector3 _customOffsetFollow;
    [SerializeField] private Vector3 _customRotationFollow;
    private Vector3 _defaultPos;
    private Quaternion _defaultRot;
    public static CameraFollowRocket Instance;
    [System.NonSerialized]
    public bool isFollow;
    [System.NonSerialized]
    public bool isMoving;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        _defaultPos = transform.localPosition;
        _defaultRot = transform.localRotation;
        isFollow = false;
        isMoving = false;
    }

    public void Follow(Transform target)
    {
        StopAllCoroutines();
        StartCoroutine(FollowCroutin(target));
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && isMoving)
        {
            StopAllCoroutines();
            BackToDefault();
        }
    }


    IEnumerator FollowCroutin(Transform target)
    {
        EventManager.Invoke(EventName.OnCameraFollowRocket, true);
        isMoving = true;
        float timeElapse = 0;
        Vector3 targetOffset = isFollow ? _customOffsetFollow : _customOffsetFoward;
        Vector3 targetRotate = isFollow ? _customRotationFollow : _customRotationForward;
        while (timeElapse < _durationFollow)
        {
            timeElapse += Time.deltaTime;
            float t = timeElapse / _durationFollow;
            Vector3 newPos = target.position + targetOffset;
            Vector3 dir = newPos - transform.position;
            Quaternion rotation = Quaternion.LookRotation(dir);
            rotation = Quaternion.Euler(rotation.eulerAngles.x + targetRotate.x, rotation.eulerAngles.y + targetRotate.y, rotation.eulerAngles.z +targetRotate.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, t);
            transform.position = Vector3.Lerp(transform.position, newPos, t);
            yield return null;
        }
        StartCoroutine(WaitToBackToDefault());
    }
    IEnumerator WaitToBackToDefault()
    {
        EventManager.Invoke(EventName.OnCameraFollowRocket, false);
        float timeElapse = 0;
        while (timeElapse < _durationFollow)
        {
            timeElapse += Time.deltaTime;
            float t = timeElapse / _durationFollow;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, _defaultRot, t);
            transform.localPosition = Vector3.Lerp(transform.localPosition, _defaultPos, t);
            yield return null;
        }
        isMoving = false;
    }
    public void BackToDefault()
    {
        StopAllCoroutines();
        StartCoroutine(WaitToBackToDefault());
    }
}
