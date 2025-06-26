using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HelicopterBehaviour : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = .1f;
    [SerializeField] private float _rotationSpeed = 1f;
    [SerializeField] private int _segments = 100;
    [SerializeField] private PathCurve[] paths;
    [SerializeField] private Transform _rootBarrelMachineGun;
    [SerializeField] private Transform _barrelMachineGun;
    [SerializeField] private Transform _posSpawnBulletMachineGun;
    [SerializeField] private GameObject _bulletMachineGun;
    [SerializeField] private GameObject _effectFireMachineGun;
    [SerializeField] private float _timeDelaySpawnBulletMachineGun;
    [SerializeField] private Transform[] _posSpawnRocket;
    [SerializeField] private GameObject _rocket;
    [SerializeField] private float _numberRocketSpawnPerFire;
    [SerializeField] private float _timeDelaySpawnRocketPerFire;
    [SerializeField] private float _timeReloadRocket;
    //[SerializeField] private float _speedRotatePercentOfPath;
    private float _time = 0;
    private int _index = 0;
    private float _totalLength = 0;
    private bool _isToPercentRotate = false;
    private float _angle, dirX, dirY, delaX;
    private float timeDe = 0;
    private float _timeDelayCounter = 0;
    private bool _isFired;
    private bool _isFireMachineGunInfinity = false;
    private bool _isFireRocketInfinity = false;
    private bool _canFireRocket;
    private float _countRocketFired = 0;
    private bool _isRotate = false;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 dir = paths[0].pathBase.startPoint.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(dir);
        transform.rotation = rotation;
        transform.position = paths[0].pathBase.startPoint.position;
        _totalLength = GetTotalLengthOfCurve(paths[0].pathBase, _segments);
        _isFired = false;
        _canFireRocket = true;
    }

    // Update is called once per frame
    void Update()
    {
        MovePathBezierCurve(paths);
    }

    private void MovePathBezierCurve(PathCurve[] path)
    {
        _moveSpeed = path[_index].pathBase.moveSpeed;
        _time += Time.deltaTime;
        float time = _time * _moveSpeed;
        float t = Mathf.Clamp01(time / _totalLength);
        Vector3 pos = GetPointInBezierCurve(t, path[_index].pathBase.startPoint.position, path[_index].pathBase.controlPoint.position, path[_index].pathBase.endPoint.position);
        if (path[_index].pathBase.helicopterMoveType == HelicopterMoveType.MoveForward)
        {
            MoveForwardLogic(t, pos);
        }
        else if (path[_index].pathBase.helicopterMoveType == HelicopterMoveType.MoveandRotatoPlayer)
        {
            //MoveAndRotateToPlayerLogic(path[_index], pos);
            MoveAndRotateToPlayer(t, pos, LocalPlayer.Instance.GetTranformPlayer());
        }
        else if (path[_index].pathBase.helicopterMoveType == HelicopterMoveType.MoveBackwardandRotaForward)
        {
            MoveBackwardandRotaForward(t, pos, _angle);
        }
    }

    private void MoveForwardLogic(float t, Vector3 pos)
    {
        if (Vector3.Distance(transform.position, paths[_index].pathBase.startPoint.position) < .1f)
        {
            if (paths[_index].pathBase.isAttackPath)
            {
                if (paths[_index].pathBase.bulletType.Equals(BulletType.MachineGun))
                {
                    _isFireMachineGunInfinity = true;
                    StartCoroutine(FireInfinityMachineGun());
                }
                if (paths[_index].pathBase.bulletType.Equals(BulletType.Rocket))
                {
                    if (!_isFireRocketInfinity)
                    {
                        _isFireRocketInfinity = true;
                        StartCoroutine(FireRocketInfinityCoroutin());
                    }
                }
            }
        }
        if (Vector3.Distance(transform.position, paths[_index].pathBase.endPoint.position) >= .1f)
        {
            float rotationAngle = 0;
            if (t <= .5f)
            {
                rotationAngle = Mathf.Lerp(0, paths[_index].pathBase.rotateAngle, t * 2);
            }
            else
            {
                rotationAngle = Mathf.Lerp(paths[_index].pathBase.rotateAngle, 0, (t - 0.5f) * 2);
            }
            if (t >= paths[_index].pathBase.percentOfLeghtChangeRotate && paths[_index].pathBase.percentOfLeghtChangeRotate != 0)
            {
                timeDe += 10 * Time.deltaTime;
                _moveSpeed = Mathf.Lerp(_moveSpeed, 0, timeDe);
                if (!_isToPercentRotate)
                {
                    int nextIndex = Mathf.Clamp(_index + 1, 0, paths.Length - 1);
                    if (paths[nextIndex].pathBase.helicopterMoveType == HelicopterMoveType.MoveandRotatoPlayer)
                    {
                        if (!_isRotate)
                        {
                            StartCoroutine(RotateToPlayer());
                        }
                    }
                    else if (paths[nextIndex].pathBase.helicopterMoveType == HelicopterMoveType.MoveBackwardandRotaForward)
                    {
                        StartCoroutine(RotateToDefault());
                    }
                }
                RotateGunDefault();
                if (paths[_index].pathBase.bulletType.Equals(BulletType.MachineGun))
                {
                    _isFireMachineGunInfinity = false;
                }
                if (paths[_index].pathBase.bulletType.Equals(BulletType.Rocket))
                {
                    _isFireRocketInfinity = false;
                }
                if (_effectFireMachineGun.activeInHierarchy)
                {
                    _effectFireMachineGun.SetActive(false);
                }
            }
            else
            {
                Vector3 dir = pos - transform.position;
                Quaternion rotation = Quaternion.LookRotation(dir);
                rotation = Quaternion.Euler(rotation.eulerAngles.x + paths[_index].pathBase.angleChangeX, rotation.eulerAngles.y, rotationAngle);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
                if (paths[_index].pathBase.isAttackPath && paths[_index].pathBase.bulletType == BulletType.MachineGun) RotateGunAndAttack();

            }
            transform.position = pos;
        }
        else
        {
            PrepareNextPath();
        }
    }

    //private void MoveAndRotateToPlayerLogic(PathCurve pathCurve, Vector3 pos)
    //{
    //    Transform playerTransform = LocalPlayer.Instance.GetTranformPlayer();
    //    Vector3 playerDir = playerTransform.position - transform.position;
    //    Quaternion lookAtPlayer = Quaternion.LookRotation(playerDir);

    //    if (!_hasRotatedToPlayer)
    //    {
    //        if (!_isRotatingToPlayer)
    //        {
    //            _isRotatingToPlayer = true;
    //            _rotationElapsed = 0f;
    //        }

    //        _rotationElapsed += Time.deltaTime;
    //        transform.rotation = Quaternion.Slerp(transform.rotation, lookAtPlayer, pathCurve.pathBase.startRotaSpeed * Time.deltaTime);

    //        if (_rotationElapsed >= pathCurve.pathBase.timeRota)
    //        {
    //            _isRotatingToPlayer = false;
    //            _hasRotatedToPlayer = true;
    //            _time = 0; // Reset movement time to start moving after rotation
    //        }
    //    }
    //    else
    //    {
    //        if (Vector3.Distance(transform.position, pathCurve.pathBase.endPoint.position) >= .1f)
    //        {
    //            Vector3 moveDir = pos - transform.position;
    //            Quaternion moveRotation = Quaternion.LookRotation(playerDir);

    //            // Ensure rotation on both X and Y axes
    //            transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, pathCurve.pathBase.moveRotaSpeed * Time.deltaTime);
    //            transform.position = Vector3.MoveTowards(transform.position, pos, _moveSpeed * Time.deltaTime);
    //        }
    //        else
    //        {
    //            _hasRotatedToPlayer = false;
    //            //PrepareNextPath();
    //        }
    //    }
    //}
    private void MoveAndRotateToPlayer(float t, Vector3 pos, Transform target)
    {
        if (Vector3.Distance(transform.position, paths[_index].pathBase.startPoint.position) < .1f)
        {
            if (paths[_index].pathBase.isAttackPath)
            {
                if (paths[_index].pathBase.bulletType.Equals(BulletType.MachineGun))
                {
                    _isFireMachineGunInfinity = true;
                    StartCoroutine(FireInfinityMachineGun());
                }
                if (paths[_index].pathBase.bulletType.Equals(BulletType.Rocket))
                {
                    if (!_isFireRocketInfinity)
                    {
                        _isFireRocketInfinity = true;
                        StartCoroutine(FireRocketInfinityCoroutin());
                    }
                }
            }
        }
        if (Vector3.Distance(transform.position, paths[_index].pathBase.endPoint.position) >= .1f)
        {
            float rotationAngle = 0;
            if (t <= .5f)
            {
                rotationAngle = Mathf.Lerp(0, paths[_index].pathBase.rotateAngle, t * 2);
            }
            else
            {
                rotationAngle = Mathf.Lerp(paths[_index].pathBase.rotateAngle, 0, (t - 0.5f) * 2);
            }
            if (t >= paths[_index].pathBase.percentOfLeghtChangeRotate && paths[_index].pathBase.percentOfLeghtChangeRotate != 0)
            {
                timeDe += 10 * Time.deltaTime;
                _moveSpeed = Mathf.Lerp(_moveSpeed, 0, timeDe);
                if (!_isToPercentRotate)
                {
                    int nextIndex = Mathf.Clamp(_index + 1, 0, paths.Length - 1);
                    if (paths[nextIndex].pathBase.helicopterMoveType == HelicopterMoveType.MoveForward)
                    {
                        StartCoroutine(RotateToNextPath(paths[nextIndex], paths[_index].pathBase.durationRotateToNextPath));
                    }
                    else if (paths[nextIndex].pathBase.helicopterMoveType == HelicopterMoveType.MoveBackwardandRotaForward)
                    {
                        StartCoroutine(RotateToDefault());
                    }
                }
                RotateGunDefault();
                if (paths[_index].pathBase.bulletType.Equals(BulletType.MachineGun))
                {
                    _isFireMachineGunInfinity = false;
                }
                if (paths[_index].pathBase.bulletType.Equals(BulletType.Rocket))
                {
                    _isFireRocketInfinity = false;
                }
                if (_effectFireMachineGun.activeInHierarchy)
                {
                    _effectFireMachineGun.SetActive(false);
                }
            }
            else
            {
                float directionAngleZ = dirX > 0 ? -1 : 1;
                Vector3 dir = target.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(dir);
                rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, directionAngleZ * paths[_index].pathBase.angleChangeZ);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, (_rotationSpeed / 2) * Time.deltaTime);
                if (paths[_index].pathBase.isAttackPath && paths[_index].pathBase.bulletType == BulletType.MachineGun) RotateGunAndAttack();
            }
            transform.position = pos;
        }
        else
        {
            PrepareNextPath();
        }
    }
    private void MoveBackwardandRotaForward(float t, Vector3 pos, float angle)
    {
        if (Vector3.Distance(transform.position, paths[_index].pathBase.endPoint.position) >= .1f)
        {
            if (t >= paths[_index].pathBase.percentOfLeghtChangeRotate && paths[_index].pathBase.percentOfLeghtChangeRotate != 0)
            {
                timeDe += 10 * Time.deltaTime;
                _moveSpeed = Mathf.Lerp(_moveSpeed, 0, timeDe);
                if (!_isToPercentRotate)
                {
                    int nextIndex = Mathf.Clamp(_index + 1, 0, paths.Length - 1);
                    if (paths[nextIndex].pathBase.helicopterMoveType == HelicopterMoveType.MoveForward)
                    {
                        //StartCoroutine(RotateToNextPath(paths[nextIndex], paths[_index].pathBase.durationRotateToNextPath));
                        StartCoroutine(RotateToDefault());
                    }
                    else if (paths[nextIndex].pathBase.helicopterMoveType == HelicopterMoveType.MoveandRotatoPlayer)
                    {
                        if (!_isRotate)
                        {
                            StartCoroutine(RotateToPlayer());
                        }
                    }
                }
                RotateGunDefault();
            }
            else
            {
                float directionAngleX = dirY > 0 ? 1 : -1;
                Quaternion rotation = Quaternion.Euler(angle * directionAngleX, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, (_rotationSpeed / 2) * Time.deltaTime);
            }
            transform.position = pos;
        }
        else
        {
            PrepareNextPath();
        }
    }
    private void SetupThresholdPath(Vector3 currentPos, Vector3 targetPos)
    {
        Vector3 v1 = targetPos - currentPos;
        Vector3 v2 = new Vector3(targetPos.x, currentPos.y, targetPos.z) - currentPos;
        _angle = Vector3.Angle(v1, v2);
        dirX = v1.x > 0 ? 1 : -1;
        dirY = v1.y < 0 ? 1 : -1;
    }
    private void PrepareNextPath()
    {
        //_isToPercentRotate = false;
        _time = 0;
        timeDe = 0;
        int previousIndex = _index;
        if(!_isToPercentRotate)
        {
            _timeDelayCounter += Time.deltaTime;
            if (_timeDelayCounter >= paths[_index].pathBase.timeDelay)
            {
                _index++;
                _timeDelayCounter = 0;
                _isRotate = false;
            }
        }
        if (_index == paths.Length)
        {
            _index = 0;
        }
        _totalLength = GetTotalLengthOfCurve(paths[_index].pathBase, _segments);

        // Logic to rotate to align with the next Path if switching to MoveForward
        //if (paths[_index].pathBase.helicopterMoveType == HelicopterMoveType.MoveForward)
        //{
        //    StartCoroutine(RotateToNextPath(paths[_index]));
        //}
        if (paths[_index].pathBase.helicopterMoveType == HelicopterMoveType.MoveBackwardandRotaForward || paths[_index].pathBase.helicopterMoveType == HelicopterMoveType.MoveandRotatoPlayer)
        {
            SetupThresholdPath(transform.position, paths[_index].pathBase.endPoint.position);
        }
    }
    private IEnumerator RotateToPlayer()
    {
        _isRotate = true;
        _isToPercentRotate = true;
        float timeElapsed = 0;
        Quaternion startRotation = transform.rotation;
        Transform playerTransform = LocalPlayer.Instance.GetTranformPlayer();
        Vector3 dir = playerTransform.position - transform.position;
        Quaternion lookAtPlayer = Quaternion.LookRotation(dir);
        float startAngle = transform.eulerAngles.y;
        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float dirAngle = (paths[_index].pathBase.endPoint.position - paths[_index].pathBase.startPoint.position).x > 0 ? 1 : -1;
        if(dirAngle > 0 && targetAngle < startAngle)
        {
            targetAngle += 360;
        }else if(dirAngle < 0 && targetAngle > startAngle)
        {
            targetAngle -= 360;
        }
        float duration = paths[_index].pathBase.durationRotateToPlayer / 2;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;
            transform.rotation = Quaternion.Euler(Mathf.Lerp(startRotation.eulerAngles.x, lookAtPlayer.eulerAngles.x,t), Mathf.Lerp(startAngle, targetAngle + (50 * -dirAngle), t), Mathf.Lerp(startRotation.eulerAngles.z, lookAtPlayer.eulerAngles.z, t));
            yield return null;
        }
        timeElapsed = 0;
        startRotation = transform.rotation;
        dir = playerTransform.position - paths[_index].pathBase.endPoint.position;
        lookAtPlayer = Quaternion.LookRotation(dir);
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;
            transform.rotation = Quaternion.Slerp(startRotation, lookAtPlayer, t);
            yield return null;
        }
        _isToPercentRotate = false;
    }
    private IEnumerator RotateToDefault()
    {
        _isToPercentRotate = true;
        float timeElapsed = 0;
        Quaternion startRotation = transform.rotation;
        while (timeElapsed < paths[_index].pathBase.durationRotateBackToDefault)
        {
            timeElapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, Quaternion.Euler(0, 0, 0), timeElapsed / paths[_index].pathBase.durationRotateBackToDefault);
            yield return null;
        }
        _isToPercentRotate = false;
    }
    private IEnumerator RotateToNextPath(PathCurve nextPath, float duration)
    {
        _isToPercentRotate = true;
        Vector3 nextDir = nextPath.pathBase.startPoint.position - transform.position;
        Quaternion nextRotation = Quaternion.LookRotation(nextDir);
        float rotationElapsed = 0f;
        Quaternion startRotation = transform.rotation;
        while (rotationElapsed < duration)
        {
            rotationElapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, nextRotation, rotationElapsed / duration);
            yield return null;
        }
        _isToPercentRotate = false;
        //StartCoroutine(RotateToPlayerByDuration());
    }
    private void RotateGunAndAttack()
    {
        Vector3 dir = LocalPlayer.Instance.GetLocalPlayer() - transform.position;
        Quaternion rotation = Quaternion.LookRotation(dir);
        _rootBarrelMachineGun.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        _barrelMachineGun.transform.rotation = Quaternion.Euler(rotation.eulerAngles.x, 0, 0);

    }
    private void RotateGunDefault()
    {
        _rootBarrelMachineGun.transform.localRotation = Quaternion.RotateTowards(_rootBarrelMachineGun.transform.localRotation, Quaternion.Euler(0, 0, 0), _rotationSpeed * Time.deltaTime);
        _barrelMachineGun.transform.localRotation = Quaternion.RotateTowards(_barrelMachineGun.transform.localRotation, Quaternion.Euler(0, 0, 0), _rotationSpeed * Time.deltaTime);
    }
    public void FireRocket()
    {
        for (int j = 0; j < _posSpawnRocket.Length; j++)
        {
            var rocketTank = ObjectPool.Instance.PopFromPool(_rocket, instantiateIfNone: true);
            rocketTank.transform.SetPositionAndRotation(_posSpawnRocket[j].transform.position, _posSpawnRocket[j].transform.rotation);
            RocketOnBot bullet = rocketTank.GetComponent<RocketOnBot>();
            Vector3 directionToTarget = LocalPlayer.Instance.GetTranformPlayer().position - _posSpawnRocket[j].transform.position;
            bullet.transform.rotation = Quaternion.LookRotation(directionToTarget);
            bullet.Initialize(bullet.Damage, directionToTarget);
        }
    }
    IEnumerator DelayForNextFire()
    {
        FireRocket();
        float count = 1;
        while (count < _numberRocketSpawnPerFire)
        {
            yield return new WaitForSeconds(_timeDelaySpawnRocketPerFire);
            count++;
            FireRocket();
        }
    }
    IEnumerator FireRocketInfinityCoroutin()
    {
        StartCoroutine(DelayForNextFire());
        float time = 0;
        while (_isFireRocketInfinity)
        {
            time += Time.deltaTime;
            if (time >= _timeReloadRocket)
            {
                time = 0;
                StartCoroutine(DelayForNextFire());
            }
            yield return null;
        }
    }
    public void FireMachineGun()
    {
        if (!_effectFireMachineGun.activeInHierarchy)
        {
            _effectFireMachineGun.SetActive(true);
        }
        Vector3 dir = LocalPlayer.Instance.GetTranformPlayer().position - _posSpawnBulletMachineGun.position;
        Quaternion rotation = Quaternion.LookRotation(dir);
        var bullet = ObjectPool.Instance.PopFromPool(_bulletMachineGun, instantiateIfNone: true);
        bullet.transform.SetPositionAndRotation(_posSpawnBulletMachineGun.position, rotation);
        BulletTrail bulletTrail = bullet.GetComponent<BulletTrail>();
        bulletTrail.Speed = 2.5f;
        bulletTrail.Init(dir);
    }
    IEnumerator FireInfinityMachineGun()
    {
        float timeElapse = 0f;
        while (_isFireMachineGunInfinity)
        {
            timeElapse += Time.deltaTime;
            if (timeElapse >= _timeDelaySpawnBulletMachineGun)
            {
                timeElapse = 0f;
                FireMachineGun();
            }
            yield return null;
        }
        //_machineGun.gameObject.SetActive(false);
    }
    private float GetTotalLengthOfCurve(PathBase path, int segments)
    {
        float totalLength = 0;
        for (int i = 0; i < segments; i++)
        {
            float t1 = (float)i / segments;
            float t2 = (float)(i + 1) / segments;
            Vector3 p1 = GetPointInBezierCurve(t1, path.startPoint.position, path.controlPoint.position, path.endPoint.position);
            Vector3 p2 = GetPointInBezierCurve(t2, path.startPoint.position, path.controlPoint.position, path.endPoint.position);
            float distance = Vector3.Distance(p1, p2);
            totalLength += distance;
        }
        return totalLength;
    }
    private Vector3 GetPointInBezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 point = uu * p0;
        point += 2 * u * t * p1;
        point += tt * p2;
        return point;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        int number = 50;
        for (int i = 0; i < paths.Length; i++)
        {
            for (int j = 0; j < number; j++)
            {
                float t1 = (float)j / number;
                float t2 = (float)(j + 1) / number;
                Vector3 point1 = GetPointInBezierCurve(t1, paths[i].pathBase.startPoint.position, paths[i].pathBase.controlPoint.position, paths[i].pathBase.endPoint.position);
                Vector3 point2 = GetPointInBezierCurve(t2, paths[i].pathBase.startPoint.position, paths[i].pathBase.controlPoint.position, paths[i].pathBase.endPoint.position);
                Gizmos.DrawLine(point1, point2);
            }
        }
    }
#endif
}
