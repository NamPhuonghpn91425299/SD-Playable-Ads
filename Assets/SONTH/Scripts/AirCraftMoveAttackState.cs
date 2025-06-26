using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static AirCraftStateMachine;


public class AirCraftMoveAttackState : BaseState<AirCraftState>
{
    [SerializeField] private BotNetwork _botNetwork;
    [SerializeField] private float _moveSpeed = .1f;
    [SerializeField] private float _rotationSpeed = 1f;
    [SerializeField] private int _segments = 100;
    [SerializeField] private PathCurve[] paths;
    [SerializeField] private GameObject _rocket;
    [SerializeField] private Transform _posSpawnRocket;
    [SerializeField] private GameObject _machineGun;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private Transform _posSpawnBullet;
    [SerializeField] private Transform _posSpawnBullet1;
    [SerializeField] private float _timeAttackMachineGun = 2;
    [SerializeField] private float _timeDelaySpawnBulletMachineGun = .01f;
    [SerializeField] private float _timeReloadRocket = 1;
    [SerializeField] private GameObject _explosionRocket;

    [SerializeField] private GameObject _projectile_Aicraft;
    [SerializeField] private GameObject _projectile_Aicraft1;
    private float _time = 0;
    private int _index = 0;
    private float _totalLenght = 0;
    private bool _isFired;
    private bool _isFireMachineGunInfinity = false;
    private bool _isFireRocketInfinity = false;

    public override void EnterState()
    {
        Vector3 dir = paths[0].pathBase.startPoint.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(dir);
        transform.rotation = rotation;
        transform.position = paths[0].pathBase.startPoint.position;
        _totalLenght = GetTotalLengthOfCurve(paths[0].pathBase, _segments);
    }
    public override void ExitState()
    {
        StopAllCoroutines();
        _machineGun.SetActive(false);
    }

    public override AirCraftState GetNextState()
    {
        if (_botNetwork.IsDead)
        {
            return AirCraftState.Dead;
        }
        return StateKey;
    }

    public override void UpdateState()
    {
        if (!_botNetwork.IsDead)
        {
            MovePathBezierCurve(paths);
        }
    }
    bool MoveToTargetPoint(Vector3 targetPoint)
    {
        float distance = Vector3.Distance(transform.position, targetPoint);
        if (distance <= .1f)
        {
            return true;
        }
        else
        {
            Vector3 dir = targetPoint - transform.position;
            Quaternion rotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, _moveSpeed * Time.deltaTime);
        }
        return false;
    }
    void MovePathBezierCurve(PathCurve[] path)
    {
        _moveSpeed = path[_index].pathBase.moveSpeed;
        _time += Time.deltaTime;
        float time = _time * _moveSpeed;
        float t = Mathf.Clamp01(time / _totalLenght);
        Vector3 pos = GetPoinInBezierCurve(t, path[_index].pathBase.startPoint.position, path[_index].pathBase.controlPoint.position, path[_index].pathBase.endPoint.position);
        Vector3 dir = pos - transform.position;
        if (Vector3.Distance(transform.position, path[_index].pathBase.startPoint.position) < .1f)
        {
            if (path[_index].pathBase.isAttackPath)
            {
                if (path[_index].pathBase.attackType.Equals(AttackType.OnPoint))
                {
                    if (path[_index].pathBase.typePoint.Equals(TypePoint.StartPoint))
                    {
                        if (path[_index].pathBase.bulletType.Equals(BulletType.Rocket) && !_isFired)
                        {
                            StartCoroutine(FireRocketCoroutin());
                        }
                        else if (path[_index].pathBase.bulletType.Equals(BulletType.MachineGun) && !_isFired)
                        {
                            StartCoroutine(FireMachineGunCoroutin());
                        }
                    }
                }
                else if (path[_index].pathBase.attackType.Equals(AttackType.OnMove))
                {
                    if (path[_index].pathBase.bulletType.Equals(BulletType.MachineGun))
                    {
                        _isFireMachineGunInfinity = true;
                        StartCoroutine(FireInfinityMachineGun());
                    }
                    if (path[_index].pathBase.bulletType.Equals(BulletType.Rocket))
                    {
                        if (!_isFireRocketInfinity)
                        {
                            _isFireRocketInfinity = true;
                            StartCoroutine(FireRocketInfinityCoroutin());
                        }
                    }
                }
            }
        }
        if (Vector3.Distance(transform.position, path[_index].pathBase.endPoint.position) >= .1f)
        {
            float rotationAngle = 0;
            if (t <= .5f)
            {
                rotationAngle = Mathf.Lerp(0, path[_index].pathBase.rotateAngle, t * 2);
            }
            else
            {
                rotationAngle = Mathf.Lerp(path[_index].pathBase.rotateAngle, 0, (t - 0.5f) * 2);
            }
            Quaternion rotation = Quaternion.LookRotation(dir);
            rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, rotationAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
            transform.position = pos;
        }
        else
        {
            if (path[_index].pathBase.isAttackPath)
            {
                if (path[_index].pathBase.attackType.Equals(AttackType.OnPoint))
                {
                    if (path[_index].pathBase.typePoint.Equals(TypePoint.EndPoint))
                    {
                        if (path[_index].pathBase.bulletType.Equals(BulletType.Rocket) && !_isFired)
                        {
                            StartCoroutine(FireRocketCoroutin());
                        }
                        else if (path[_index].pathBase.bulletType.Equals(BulletType.MachineGun) && !_isFired)
                        {
                            StartCoroutine(FireMachineGunCoroutin());
                        }
                    }
                }
                else if (path[_index].pathBase.attackType.Equals(AttackType.OnMove))
                {
                    if (path[_index].pathBase.bulletType.Equals(BulletType.MachineGun))
                    {
                        _isFireMachineGunInfinity = false;
                    }
                    if (path[_index].pathBase.bulletType.Equals(BulletType.Rocket))
                    {
                        _isFireRocketInfinity = false;
                    }
                }
            }
            _time = 0;
            _index++;
            //_index = Math.Clamp(_index, 0, path.Length - 1);
            if (_index == path.Length)
            {
                _index = 0;
            }
            _totalLenght = GetTotalLengthOfCurve(path[_index].pathBase, _segments);
        }
    }
    public void FireRocket()
    {
        var rocketTank = ObjectPool.Instance.PopFromPool(_rocket, instantiateIfNone: true);
        rocketTank.transform.SetPositionAndRotation(_posSpawnRocket.transform.position, _posSpawnRocket.transform.rotation);
        RocketOnBot bullet = rocketTank.GetComponent<RocketOnBot>();
        Vector3 directionToTarget = (LocalPlayer.Instance.GetTranformPlayer().position - _posSpawnRocket.transform.position).normalized;
        bullet.transform.rotation = Quaternion.LookRotation(directionToTarget);
        bullet.Initialize(bullet.Damage, directionToTarget);
    }
    IEnumerator FireRocketCoroutin()
    {
        FireRocket();
        _isFired = true;
        yield return new WaitForSeconds(2);
        _isFired = false;
    }
    IEnumerator FireRocketInfinityCoroutin()
    {
        FireRocket();
        float time = 0;
        while (_isFireRocketInfinity)
        {
            time += Time.deltaTime;
            if (time >= _timeReloadRocket)
            {
                time = 0;
                FireRocket();
            }
            yield return null;
        }
    }
    public void FireMachineGun()
    {
        if (!_machineGun.gameObject.activeInHierarchy)
        {
            _machineGun.gameObject.SetActive(true);
        }
        Vector3 dir = (LocalPlayer.Instance.GetTranformPlayer().position - _machineGun.transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(dir);
        _machineGun.transform.rotation = rotation;

        Vector3 dir1 = (LocalPlayer.Instance.GetTranformPlayer().position - _projectile_Aicraft.transform.position).normalized;
        var bullet = ObjectPool.Instance.PopFromPool(_bullet, instantiateIfNone: true);
        bullet.transform.SetPositionAndRotation(_posSpawnBullet.position, Quaternion.LookRotation(dir1));
        BulletTrail bulletTrail = bullet.GetComponent<BulletTrail>();
        //bulletTrail.Speed = 2.5f;
        bulletTrail.Init(dir);

        Vector3 dir2 = (LocalPlayer.Instance.GetTranformPlayer().position - _projectile_Aicraft1.transform.position).normalized;
        bullet = ObjectPool.Instance.PopFromPool(_bullet, instantiateIfNone: true);
        bullet.transform.SetPositionAndRotation(_posSpawnBullet1.position, Quaternion.LookRotation(dir2));
        bulletTrail = bullet.GetComponent<BulletTrail>();
        //bulletTrail.Speed = 2.5f;
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
        _machineGun.gameObject.SetActive(false);
    }
    IEnumerator FireMachineGunCoroutin()
    {
        _isFired = true;
        float time = 0;
        float timeElapse = 0f;
        while (time < _timeAttackMachineGun)
        {
            time += Time.deltaTime;
            timeElapse += Time.deltaTime;
            if (timeElapse >= _timeDelaySpawnBulletMachineGun)
            {
                timeElapse = 0f;
                FireMachineGun();
            }
            yield return null;
        }
        _isFired = false;
        _machineGun.gameObject.SetActive(false);
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
        // Bezier Bậc 2 : B(t) = (1-t)^2 * p0 + 2 * (1-t) * t * p1 + t^2 * p2
        // t [0,1]
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 point = uu * p0;
        point += 2 * u * t * p1;
        point += tt * p2;
        return point;
    }
}
