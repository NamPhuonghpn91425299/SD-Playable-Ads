using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static GameConstants;

public class RocketPanzerwerfer : GameUnit<ProjectileEnemy>
{
    [SerializeField] private float height;
    [SerializeField] private float speedFlyUp;
    [SerializeField] private float speedFlyToPlayer;
    [SerializeField] private float distanceToDestroy;
    [SerializeField] private ProjectileEnemy _explosionType;
    [SerializeField] private int damage;

    void OnEnable()
    {
        StartCoroutine(IEStartFlyUp());
    }

    public void Init(int damage)
    {
        this.damage = damage;
    }

    private void Update()
    {
        if (Vector3.Distance(TF.position, PlayerInstant.Instance.transform.position) < distanceToDestroy)
        {
            OnDespawn();
        }
    }

    IEnumerator IEStartFlyUp()
    {
        // Xoay về hướng lên trời (mặt rocket hướng lên)
        TF.transform.DORotate(new Vector3(-90, 0, 0), 0.5f).SetEase(Ease.Linear);

        // Bay lên cao từ vị trí hiện tại
        float currentY = TF.position.y;
        TF.DOMoveY(currentY + height, speedFlyUp).SetEase(Ease.Linear);

        yield return new WaitForSeconds(speedFlyUp);
        StartCoroutine(IEFlyToThePlayer());
    }

    private IEnumerator IEFlyToThePlayer()
    {
        Vector3 startPos = TF.position;
        Vector3 targetPos = PlayerInstant.Instance.transform.position;

        Vector3 dir = (targetPos - startPos).normalized;
        float yRotation = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        TF.rotation = Quaternion.Euler(TF.rotation.eulerAngles.x, yRotation, TF.rotation.eulerAngles.z);

        float xRotation = Mathf.Atan2(-dir.y, new Vector2(dir.x, dir.z).magnitude) * Mathf.Rad2Deg;
        Vector3 finalRotation = new Vector3(xRotation, yRotation, TF.rotation.eulerAngles.z);

        yield return TF.DORotate(finalRotation, 0.25f).SetEase(Ease.Linear).WaitForCompletion();

        Vector3 midPoint = (startPos + targetPos) / 2f;

        midPoint.y = Mathf.Max(targetPos.y, midPoint.y - height);

        if (midPoint.y < targetPos.y)
            midPoint.y = targetPos.y;

        Vector3[] path = new Vector3[] { startPos, midPoint, targetPos };

        TF.DOPath(path, speedFlyToPlayer, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .SetLookAt(0.01f);
    }


    public void OnDespawn()
    {
        ExplosionPanzerwerfer bullet = SimplePool<GameConstants.ProjectileEnemy>.Spawn<ExplosionPanzerwerfer>(_explosionType, this.transform.position, Quaternion.identity);
        EventManager.Instance?.Publish(new PlayerHealthChangedEvent(damage: damage, state: "OnlyDamage"));
        EventManager.Instance?.Publish(new CamShakeEvent(new CamShakeData{duration = .3f,strength = .025f,vibrato = 15,randomness = 45}));
        SimplePool<ProjectileEnemy>.Despawn(this);
    }
}
