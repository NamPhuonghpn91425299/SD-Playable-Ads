using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WeaponController : MonoBehaviour
{
    [SerializeField]Transform weaponParent;
    public float shootCooldown = 0.5f; // Thời gian giữa các lần bắn (đơn vị giây)
    private float lastShootTime = 0f;  // Thời điểm cuối cùng bắn

    [SerializeField] private List<WeaponDataSO> weapons;
    [SerializeField] Weapon weapon;
    [SerializeField] WeaponDataSO weaponSO;
    private LayerMask hitMask;
    private float missDistance = 50;
    private ObjectPool<GameObject> TrailPool;

    public static WeaponController instance;
    private void Awake()
    {
        instance = this;
        this.SpawnWeapon(weaponParent, weapons[0]);
        TrailPool = new ObjectPool<GameObject>(CreateTrail);
    }
    //void Update()
    //{
    //    // Kiểm tra nếu bấm chuột trái và có súng được chọn
    //    if (Input.GetMouseButton(0) && weapon != null)
    //    {
    //        // Kiểm tra cooldown
    //        if (Time.time >= lastShootTime + shootCooldown)
    //        {
    //            // Bắn và cập nhật thời gian bắn cuối cùng
    //            SpawnBullet();
    //            lastShootTime = Time.time;
    //        }
    //    }
    //}
    public void SpawnWeapon(Transform parent, WeaponDataSO weapondata)
    {
        weapon = weapondata.SpawnWeapon(parent);
        weaponSO = weapondata;
    }

    public void SpawnBullet()
    {
        
        Vector3 shootDirection = weapon.spawmBulletPoint.forward;
        shootDirection.Normalize();
        weapon.PlayEffect();
        //weapon.PlayAnimation("Fire");
        var bullet = TrailPool.Get().GetComponent<BulletFly>();
        bullet.transform.position = weapon.spawmBulletPoint.position;
        bullet.transform.rotation = weapon.spawmBulletPoint.rotation;
        var startponit = weapon.spawmBulletPoint.position;
        var endponit = Physics.Raycast(weapon.spawmBulletPoint.position, shootDirection, out RaycastHit hit, float.MaxValue, hitMask)
            ? hit.point
            : weapon.spawmBulletPoint.position + (shootDirection * missDistance);


        bullet.Init(startponit, endponit, weaponSO.FiringRate);
    }
    private GameObject CreateTrail()
    {
        var bulet = Instantiate(weaponSO.bulletPrefab);
        return bulet;

    }
}
