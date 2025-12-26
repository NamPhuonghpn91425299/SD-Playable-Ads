using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static GameConstants;

public class ThuyenPR1124_Attack : StateBase
{
    [SerializeField] private Transform turret;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Transform gunTurret;
    [SerializeField] protected ProjectileEnemy _bulletType;  // Loại đạn sẽ được bắn
    bool isFiring = false;
    float timer;
    [SerializeField] private float timerReload = 5f;

    [SerializeField] private int maxBullet = 3;
    [SerializeField] private int currentBullet = 0;

    public override void EnterState()
    {
        // Quay turret theo trục Y (horizontal) về phía người chơi
        Vector3 horizontalDirection = PlayerInstant.Instance.transform.position - turret.position;
        horizontalDirection.y = 0; // Loại bỏ thành phần Y để chỉ quay ngang

        turret.DORotateQuaternion(Quaternion.LookRotation(horizontalDirection), 1f).OnComplete(() =>
        {
            // Sau khi turret quay xong, gunTurret ngẩng lên để nhắm vào người chơi
            gunTurret.DOLookAt(PlayerInstant.Instance.transform.position, rotateSpeed).OnComplete(() =>
            {
                // Bắt đầu bắn sau khi đã nhắm xong
                isFiring = true;
            });
        });
    }

    public override void ExitState()
    {

    }

    public override void UpdateState()
    {

        if (isFiring && currentBullet < maxBullet)
        {
            timer += Time.deltaTime;
            if (timer >= fireRate)
            {
                currentBullet++;
                muzzleFlash.Play();
                timer = 0f;
                Rocket bullet = SimplePool<ProjectileEnemy>.Spawn<Rocket>(_bulletType, muzzleFlash.gameObject.transform.position, muzzleFlash.gameObject.transform.rotation);
                bullet.Init(botContext.botNetwork.Damage);
            }
        }
        else if (currentBullet >= maxBullet)
        {
            isFiring = false;
            timer += Time.deltaTime;
            if (timer >= timerReload)
            {
                currentBullet = 0;
                timer = 0f;
                isFiring = true;
            }
        }

    }


}
