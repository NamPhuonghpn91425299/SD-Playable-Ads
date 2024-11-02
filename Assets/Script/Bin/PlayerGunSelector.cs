using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour
{
    [SerializeField] private string gunName;
    [SerializeField] private Transform gunParent;
    [SerializeField] private List<GunSO> guns;
    [Space]
    public GunSO activeGun;

    private void Start()
    {
        GunSO gun = guns.Find(gun => gun.Name == gunName);
        if (gun == null)
        {
            Debug.LogError($"No weapon: {gun}");
            return;

        }
        activeGun = gun;
        gun.Spawn(gunParent, this);
    }

}
