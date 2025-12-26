using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

public class Panzerwerfer_Attack : StateBase
{
    [SerializeField] private Transform _turretBody;
    [SerializeField] private Transform _turretHead;
    [SerializeField] private Transform[] _turrentSpawnPoints;
    [SerializeField] protected GameConstants.ProjectileEnemy _bulletType;  // Loại đạn sẽ được bắn
    [SerializeField] private float _timerAim;
    [SerializeField] private float _fireRate;
    [SerializeField] private float _turnSpeed = 45f;
    [SerializeField] private float _launchForce = 10f;
    [SerializeField] private int _maxBulletSpawned = 5;
    [SerializeField] private float _headPitchDegrees = 50f; // Góc ngẩng nòng lên trời

    Quaternion _initialBodyRotation;
    Quaternion _initialHeadRotation;

    private void Start()
    {
        _initialBodyRotation = _turretBody.localRotation;
        _initialHeadRotation = _turretHead.localRotation;
    }

    public override void EnterState()
    {
        StartCoroutine(RotateToPlayer());
    }

    public override void ExitState()
    {
        StopAllCoroutines();

    }

    public override void UpdateState()
    {
        if(botContext.botNetwork.IsDead)
        {
            StopAllCoroutines();
        }

    }

    IEnumerator RotateToPlayer()
    {
        // Thời gian để ngắm trước khi bắn
        float aimTime = 0f;
        bool hasAimed = false;

        while (!hasAimed)
        {
            // Yaw thân tháp pháo về phía player chỉ theo trục Y (bỏ qua chênh lệch độ cao)
            Vector3 toPlayer = PlayerInstant.Instance.transform.position - _turretBody.position;
            toPlayer.y = 0f; // loại chiều cao
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                Quaternion targetBodyRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
                _turretBody.rotation = Quaternion.RotateTowards(_turretBody.rotation, targetBodyRot, _turnSpeed * Time.deltaTime);
            }

            // Pitch đầu nòng xuống để bắn (_headPitchDegrees âm để hướng xuống)
            Quaternion targetHeadLocal = Quaternion.Euler(-_headPitchDegrees, 0f, 0f) * _initialHeadRotation;
            _turretHead.localRotation = Quaternion.Slerp(_turretHead.localRotation, targetHeadLocal, Time.deltaTime * _turnSpeed);

            // Đếm thời gian ngắm
            aimTime += Time.deltaTime;
            if (aimTime >= _timerAim)
            {
                hasAimed = true;
            }

            yield return null;
        }

        // Sau khi ngắm xong, bắn rocket
        StartCoroutine(IESpawnRocket());
    }

    private IEnumerator IEReRotateToInitial()
    {
        yield return HelperCoroutine.GetWait(_timerAim);
        while (_turretBody.localRotation != _initialBodyRotation || _turretHead.localRotation != _initialHeadRotation)
        {
            _turretBody.localRotation = Quaternion.RotateTowards(_turretBody.localRotation, _initialBodyRotation, _turnSpeed * Time.deltaTime);
            _turretHead.localRotation = Quaternion.RotateTowards(_turretHead.localRotation, _initialHeadRotation, _turnSpeed * Time.deltaTime);
            yield return null;
        }

        // Chuyển về trạng thái Move sau khi hoàn thành attack
        StartCoroutine(IEChangeMoveState());
    }


    private IEnumerator IESpawnRocket()
    {
        for (int i = 0; i < _maxBulletSpawned; i++)
        {
            // Sử dụng modulo để tránh lỗi index out of range
            int spawnPointIndex = i % _turrentSpawnPoints.Length;
            SpawnRocket(_turrentSpawnPoints[spawnPointIndex].position);
            yield return HelperCoroutine.GetWait(_fireRate);
        }
        StartCoroutine(IEReRotateToInitial());
    }

    IEnumerator IEChangeMoveState()
    {
        yield return HelperCoroutine.GetWait(2f);
        botContext.stateController.ChangeState(EnemyState.Move);
    }

    public void SpawnRocket(Vector3 positionSpawn)
    {
        try
        {
            RocketPanzerwerfer bullet = SimplePool<GameConstants.ProjectileEnemy>.Spawn<RocketPanzerwerfer>(_bulletType, positionSpawn, Quaternion.LookRotation(Vector3.up));
            bullet.Init(botContext.botNetwork.Damage);
        }
        catch (Exception e)
        {
            Debug.LogError("Spawn error: " + e);
        }


    }
}
