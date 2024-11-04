using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
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
    [SerializeField]
    private LayerMask hitMask;
    [SerializeField]
    private float missDistance = 50;
    [SerializeField]
    private ObjectPool<GameObject> TrailPool;
    [SerializeField]
    private PoolType bulletPool;

    public static WeaponController instance;
    private void Awake()
    {
        instance = this;
        this.SpawnWeapon(weaponParent, weapons[0]);
        //TrailPool = new ObjectPool<GameObject>(CreateTrail);
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
        
        var bullet = ObjectPool.Instance.GetPooledObject(bulletPool, weaponSO.bulletPrefab);
        Vector3 shootDirection = weapon.spawmBulletPoint.forward;
        shootDirection.Normalize();
        weapon.PlayEffect();
        BulletFly bulletFly = bullet.GetComponent<BulletFly>();
        bulletFly.transform.position = weapon.spawmBulletPoint.position;
        bulletFly.transform.rotation = weapon.spawmBulletPoint.rotation;
        var startponit = weapon.spawmBulletPoint.position;
        var damagedealt = bullet.GetComponent<IDamageDealt>();

        Vector3 endpoint;
        if (Physics.Raycast(weapon.spawmBulletPoint.position, shootDirection, out RaycastHit hit, float.MaxValue, hitMask))
        {
            endpoint = hit.point;



            bulletFly.Init(startponit, endpoint, weaponSO.FiringRate);
            damagedealt.Init(weaponSO.Damage, hit.collider.GetComponentInParent<IDamageHit>());
        }
        else
        {
            endpoint = weapon.spawmBulletPoint.position + (shootDirection * missDistance);

            damagedealt.Init(weaponSO.Damage, null);
        }


        bulletFly.Init(startponit, endpoint, weaponSO.FiringRate);
    }
    private GameObject CreateTrail()
    {
        var bulet = ObjectPool.Instance.GetPooledObject(bulletPool,weaponSO.bulletPrefab);
        return bulet;

    }

}
