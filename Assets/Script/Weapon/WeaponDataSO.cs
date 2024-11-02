using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Guns/WeaponData", order = 0)]
public class WeaponDataSO : ScriptableObject
{
    public string Name;
    public GameObject ModePrefab;
    public GameObject bulletPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;
    public Vector3 spread = new Vector3(0.1f, 0.1f, 0.1f);
    public List<AnimationData> animationDatas;
    public int Damage = 10;
    public float FiringRate = 2f;
    [Header("Weapon Info")]
    public Sprite weaponIcon;
    [Header("Shooting Settings")]
    [Tooltip("Thời gian chờ trước khi bắt đầu bắn (giây)")]
    public float initialDelay = 1f;
    [Tooltip("Thời gian giữa các phát bắn lúc bắt đầu (giây) --- Tối đa bắn 5 viên/giây")]
    public float startFireRate = 0.2f;
    [Tooltip("Thời gian giữa các phát bắn lúc tối đa (giây) --- Tối đa bắn 10 viên/giây")]
    public float maxFireRate = 0.1f;
    [Tooltip("Thời gian để đạt tốc độ bắn tối đa (giây)")]
    public float timeToMaxFireRate = 1f;
    public Weapon SpawnWeapon(Transform parent)
    {
        var weapon = Instantiate(ModePrefab).GetComponent<Weapon>();
        weapon.transform.parent = parent;
        weapon.transform.localPosition = SpawnPoint;
        weapon.transform.localRotation = Quaternion.Euler(SpawnRotation);

        weapon.SetUp(animationDatas);
        return weapon;
    }
}
