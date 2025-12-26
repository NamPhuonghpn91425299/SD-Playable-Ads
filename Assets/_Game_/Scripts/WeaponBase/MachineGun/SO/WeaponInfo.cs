using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "ScriptableObjects/WeaponInfo", order = 1)]
public class WeaponInfo : ScriptableObject
{
    public int damage = 10;
    public float FireRate = 0.1f;
    [Header("Critical Damage")]
    public bool isCritEnabled = true;  // Bật/tắt crit toàn cục cho weapon
    [Range(1f, 5f)] public float critMultiplier = 1.5f;  // Nhân damage lên bao nhiêu (ví dụ: 2x)
    [SerializeField] private LayerMask _weakPointLayerMask;
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
    public AnimationClip Fire_Right;
    public AnimationClip Idle;
    public AnimationClip _reloadAnimIn;
    public AnimationClip _reloadAnimOn;
    public AnimationClip _reloadAnimOut;

    [Header("Các biến chỉ dùng cho súng nòng quay")]
    // Các biến cho súng 6 nòng
    public float WaitToShoot = 0; // Đợi 1 khoảng thời gian xoay nòng rồi mới bắn
    public float MaxSpeedRotaBarrel = 0; // Tốc độ quay tối đa
    public float TimeMinSpeed = 0; // Thời gian giảm tốc độ xoay nòng xuống 0
    // Public property để access (tự động init nếu chưa set)
    public LayerMask WeakPointLayerMask
    {
        get
        {
            // Nếu chưa init (value == 0, default cho LayerMask serialized), init ở đây
            // Nhưng vì OnEnable chạy trước, thường đã set rồi
            return _weakPointLayerMask;
        }
    }

    private void OnEnable()
    {
        if (_weakPointLayerMask.value == 0)
        {
            int layerIndex = LayerMask.NameToLayer("WeakPoint");
            if (layerIndex != -1) // Kiểm tra layer tồn tại
            {
                _weakPointLayerMask = 1 << layerIndex;
            }
        }
    }
}
