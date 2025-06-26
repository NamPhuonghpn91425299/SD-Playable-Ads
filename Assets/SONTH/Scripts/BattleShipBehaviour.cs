using System.Collections;
using UnityEngine;

public class BattleShipBehaviour : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1f;
    [SerializeField] private int _segments = 100;
    [SerializeField] private PathCurve[] _paths;
    [SerializeField] private GameObject _turretRocket;
    [SerializeField] private float _speedRotateTurret;
    [SerializeField] private float _timeReloadRocket = 1f;
    [SerializeField] private GameObject _rocket;
    [SerializeField] private Transform _posSpawnRocket;
    [SerializeField] private GameObject _explosionRocket;

    private float _time = 0;
    private int _index = 0;
    private float _totalLength = 0;
    private float _currentSpeed = 0;
    private float _targetSpeed = 0;
    private float _speedChangeTimer = 0;
    private bool _isChangingSpeed = false;
    private bool _isOnMoveAttack = false;
    private Coroutine _currentAttackCoroutine;
    private Quaternion _defaultTurretRocketRotation;

    void Start()
    {
        _defaultTurretRocketRotation = _turretRocket.transform.rotation;
        Vector3 dir = _paths[0].pathBase.startPoint.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(dir);
        transform.rotation = rotation;
        transform.position = _paths[0].pathBase.startPoint.position;
        _currentSpeed = _paths[0].pathBase.moveSpeed;
        _targetSpeed = _currentSpeed;
        _totalLength = GetTotalLengthOfCurve(_paths[0].pathBase, _segments);
        Debug.Log($"Initial Speed: {_currentSpeed}");
    }

    void Update()
    {
        MovePathBezierCurve(_paths);
    }

    void MovePathBezierCurve(PathCurve[] path)
    {
        // Thay đổi tốc độ nếu cần
        if (_isChangingSpeed)
        {
            float deltaSpeed = (_targetSpeed - _currentSpeed) / _speedChangeTimer * Time.deltaTime;
            _currentSpeed += deltaSpeed;
            _speedChangeTimer -= Time.deltaTime;

            Debug.Log($"Changing Speed: {_currentSpeed}");

            if (_speedChangeTimer <= 0)
            {
                _currentSpeed = _targetSpeed;
                _isChangingSpeed = false;
                Debug.Log($"Speed Stabilized at: {_currentSpeed}");
            }
        }

        // Tính toán vị trí mới trên đường cong
        _time += Time.deltaTime;
        float time = _time * _currentSpeed;
        float t = Mathf.Clamp01(time / _totalLength);
        Vector3 pos = GetPoinInBezierCurve(t,
                                           path[_index].pathBase.startPoint.position,
                                           path[_index].pathBase.controlPoint.position,
                                           path[_index].pathBase.endPoint.position);

        Vector3 dir = pos - transform.position;

        // Xoay nghiêng tàu theo góc (rotateAngle)
        float rotationAngle = 0;
        if (t <= 0.5f)
        {
            rotationAngle = Mathf.Lerp(0, path[_index].pathBase.rotateAngle, t * 2);
        }
        else
        {
            rotationAngle = Mathf.Lerp(path[_index].pathBase.rotateAngle, 0, (t - 0.5f) * 2);
        }

        // Di chuyển và xoay tàu
        if (Vector3.Distance(transform.position, path[_index].pathBase.endPoint.position) >= .1f)
        {
            Quaternion rotation = Quaternion.LookRotation(dir);
            rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, rotationAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
            transform.position = pos;
        }
        else
        {
            _time = 0;
            _index++;

            if (_index == path.Length)
            {
                _index = 0;
            }

            _totalLength = GetTotalLengthOfCurve(path[_index].pathBase, _segments);

            // Xử lý thay đổi tốc độ
            _targetSpeed = path[_index].pathBase.moveSpeed;
            if (path[_index].pathBase.isChangeSpeed)
            {
                _speedChangeTimer = path[_index].pathBase.timeChangeSpeed;
                _isChangingSpeed = true;
                Debug.Log($"Starting Speed Change: Current Speed {_currentSpeed}, Target Speed {_targetSpeed}, Duration {_speedChangeTimer}s");
            }
            else
            {
                _currentSpeed = _targetSpeed;
                Debug.Log($"Speed Set Immediately to: {_currentSpeed}");
            }

            // Xử lý logic tấn công
            if (path[_index].pathBase.isAttackPath)
            {
                if (!_isOnMoveAttack)
                {
                    _isOnMoveAttack = true; // Đặt lại trạng thái tấn công
                    if (_currentAttackCoroutine != null)
                    {
                        StopCoroutine(_currentAttackCoroutine); // Đảm bảo coroutine cũ dừng lại
                    }
                    _currentAttackCoroutine = StartCoroutine(RotateTurretToAttack(path, _turretRocket));
                }
            }
            else
            {
                if (_isOnMoveAttack)
                {
                    _isOnMoveAttack = false;
                    if (_currentAttackCoroutine != null)
                    {
                        StopCoroutine(_currentAttackCoroutine); // Dừng coroutine tấn công
                        _currentAttackCoroutine = null;
                    }
                    StartCoroutine(RotateTurretToDefault(_turretRocket)); // Xoay về trạng thái mặc định
                }
            }
            if (path[_index].pathBase.isAttackPath)
            {
                if (!_isOnMoveAttack)
                {
                    _currentAttackCoroutine = StartCoroutine(RotateTurretToAttack(path, _turretRocket));
                }
            }
            else
            {
                if (_isOnMoveAttack)
                {
                    _isOnMoveAttack = false;
                    if (_currentAttackCoroutine != null)
                    {
                        StopCoroutine(_currentAttackCoroutine);
                        _currentAttackCoroutine = null;
                    }
                    StartCoroutine(RotateTurretToDefault(_turretRocket));
                }
            }
        }
    }

    IEnumerator RotateTurretToAttack(PathCurve[] path, GameObject objToRotate)
    {
        float time = 0;
        _isOnMoveAttack = true;

        while (_isOnMoveAttack)
        {
            time += Time.deltaTime;
            if (time >= _timeReloadRocket)
            {
                time = 0;
                FireRocket();
            }

            Vector3 direction = LocalPlayer.Instance.GetTranformPlayer().position - objToRotate.transform.position;
            Quaternion ro = Quaternion.LookRotation(direction);

            objToRotate.transform.rotation = Quaternion.RotateTowards(
                objToRotate.transform.rotation,
                Quaternion.Euler(objToRotate.transform.rotation.eulerAngles.x, ro.eulerAngles.y, objToRotate.transform.rotation.eulerAngles.z),
                _speedRotateTurret * Time.deltaTime
            );

            yield return null;
        }
    }

    IEnumerator RotateTurretToDefault(GameObject objToRotate)
    {
        while (objToRotate.transform.localRotation != Quaternion.identity)
        {
            objToRotate.transform.localRotation = Quaternion.RotateTowards(objToRotate.transform.localRotation, Quaternion.identity, _speedRotateTurret * Time.deltaTime);
            yield return null;
        }
    }

    public void FireRocket()
    {
        var explosionRocket = ObjectPool.Instance.PopFromPool(_explosionRocket, instantiateIfNone: true);
        explosionRocket.transform.SetPositionAndRotation(_posSpawnRocket.transform.position, _posSpawnRocket.transform.rotation);
        explosionRocket.transform.SetParent(_posSpawnRocket.transform);
        ExplosionRocket explosion = explosionRocket.GetComponent<ExplosionRocket>();
        explosion.Explosion();

        var rocketTank = ObjectPool.Instance.PopFromPool(_rocket, instantiateIfNone: true);
        rocketTank.transform.SetPositionAndRotation(_posSpawnRocket.transform.position, _posSpawnRocket.transform.rotation);
        RocketOnBot bullet = rocketTank.GetComponent<RocketOnBot>();
        Vector3 directionToTarget = (LocalPlayer.Instance.GetTranformPlayer().position - _posSpawnRocket.transform.position).normalized;
        bullet.transform.rotation = Quaternion.LookRotation(directionToTarget);
        bullet.Initialize(10, directionToTarget);
    }

    float GetTotalLengthOfCurve(PathBase path, int segments)
    {
        float totalLength = 0;
        for (int i = 0; i < segments; i++)
        {
            float t1 = (float)i / segments;
            float t2 = (float)(i + 1) / segments;
            Vector3 p1 = GetPoinInBezierCurve(t1, path.startPoint.position, path.controlPoint.position, path.endPoint.position);
            Vector3 p2 = GetPoinInBezierCurve(t2, path.startPoint.position, path.controlPoint.position, path.endPoint.position);
            float distance = Vector3.Distance(p1, p2);
            totalLength += distance;
        }
        return totalLength;
    }

    public Vector3 GetPoinInBezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 point = uu * p0;
        point += 2 * u * t * p1;
        point += tt * p2;
        return point;
    }
}
