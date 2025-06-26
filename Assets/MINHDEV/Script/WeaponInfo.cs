using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "ScriptableObjects/WeaponInfo", order = 1)]
public class WeaponInfo : ScriptableObject
{
    public int damage = 10;
    public float FireRate = 0.1f;

    public AudioClip audioClip;
    public AudioClip AudioStartBarrel;
    public AudioClip AudioEndBarrel;
    public AudioClip AudioReloadIn;
    public AudioClip AudioReloadOut;
    public float inaccuracy = 0.01f;
    public float recoilAmount = 0.1f;
    public int bulletCount = 30; // Số lượng đạn trong băng
    public float reloadTime = 2f; // Thời gian nạp đạn
    public bool infiniteBullet = false; // Chế độ đạn vô hạn
    public AnimationClip Fire;
    public AnimationClip Idle;
    public AnimationClip _reloadAnimIn;
    public AnimationClip _reloadAnimOn;
    public AnimationClip _reloadAnimOut;

    // Các biến cho súng 6 nòng
    public float WaitToShoot = 0; // Đợi 1 khoảng thời gian xoay nòng rồi mới bắn
    public float MaxSpeedRotaBarrel = 0; // Tốc độ quay tối đa
    public float TimeMinSpeed = 0; // Thời gian giảm tốc độ xoay nòng xuống 0
    public float MinSpeedRotaBarrel = 0; // Tốc độ khi không quay nữa
    public bool isGatlingGun = false; // Trạng thái súng 6 nòng
}
