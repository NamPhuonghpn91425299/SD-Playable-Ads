using System;
using System.Linq;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct LayerMaskAndEffect
{
    public LayerMask layerMask;
    public GameConstants.EffectType effectType;
}

public class WeaponBase : GameUnit<GameConstants.Weapon>, Assets._Develop_.ThanhNT.Scripts.Observer.IObserver<ChangeProjectileGunEvent>
{
    [Header("Data Weapon")]
    [SerializeField] public WeaponInfo weaponInfo;

    [Header("Layer Target And Effect")]
    [SerializeField] protected LayerMaskAndEffect[] layerMasksAndEffects;

    [Header("Effect")]
    [SerializeField] private ParticleSystem[] _fireEffect;

    [Header("Audio")]
    [SerializeField] protected AudioSource _audioSource;

    [Header("DrawGizmod Caculator Point Shoot")]
    [SerializeField] protected Transform _cameraTransform;
    [SerializeField] LayerMask _layerTarget;

    public bool _isHoldScreen;
    bool _onChangeWeapon;


    #region BaseUnity

    protected virtual void Update()
    {
#if UNITY_EDITOR
        GizmodCaculatorPointShoot();
#endif
        if (GameController.Instance.CurrentGameState != GameConstants.GameState.InGame)
        {
            _isHoldScreen = false;
            return;
        }

        // Xử lý multi-touch: chỉ detect ngón đầu tiên để bắn

#if UNITY_EDITOR
        _isHoldScreen = !_onChangeWeapon && Input.GetMouseButton(0);
        return;
#endif
        _isHoldScreen = !_onChangeWeapon && Input.touchCount > 0;
    }

    #endregion


    public virtual void OnInit()
    {
        _onChangeWeapon = true;
        EventManager.Instance?.Subscribe<ChangeProjectileGunEvent>(this);
        GameController.Instance.CurrentWeapon = this;
        AddAnimationClips();
        _cameraTransform = GameController.Instance.CameraMainTF;
        //TODO: logic súng đi lên
        TF.localPosition = Vector3.down;
        TF.DOLocalMove(Vector3.zero, .5f).OnComplete(() => _onChangeWeapon = false);
    }

    public void OnDisable()
    {
        EventManager.Instance?.Unsubscribe<ChangeProjectileGunEvent>(this);
    }

    /// <summary>
    /// Add animation clips to the weapon
    /// </summary>
    protected virtual void AddAnimationClips()
    {

    }

    #region Check Layer target
    protected bool IsInLayerIndex(GameObject _obj, int _layerIndex) => ((1 << _obj.layer) & layerMasksAndEffects[_layerIndex].layerMask.value) != 0;
    protected int CombinedLayerMask() => layerMasksAndEffects.Aggregate(0, (mask, item) => mask | item.layerMask.value);

    #endregion

    // Thêm phương thức dừng âm thanh bắn
    protected void StopShootingSound()
    {
        if (_audioSource.isPlaying && _audioSource.clip == weaponInfo.audioClip)
            _audioSource.Stop();
    }

    public Vector3 GizmodCaculatorPointShoot()
    {
        // Check if _cameraTransform is assigned to prevent UnassignedReferenceException
        if (_cameraTransform == null)
        {
            // Return a default position if camera transform is not assigned yet
            return Vector3.zero;
        }

        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
        RaycastHit hit;

        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 200f, Color.red);

        // Bắn raycast
        if (Physics.Raycast(ray, out hit, 200f, _layerTarget))
        {
            // Debug.Log("Va chạm tại vị trí: " + hit.point);
            // Debug.Log("Khoảng cách đến box: " + hit.distance);

            Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.1f, Color.green);
            return hit.point;
        }

        return ray.origin + ray.direction * 200f;
    }

    #region Play - Stop Gun Effect
    protected void PlayGunEffect()
    {
        foreach (ParticleSystem fireEffect in _fireEffect)
            if (fireEffect != null && !fireEffect.isPlaying)
                fireEffect.Play();
    }

    public void StopGunEffect()
    {
        _isHoldScreen = false;

        foreach (ParticleSystem fireEffect in _fireEffect)
            if (fireEffect != null && fireEffect.isPlaying)
                fireEffect.Stop();
    }
    #endregion

    public virtual void OnDespawn()
    {
        _onChangeWeapon = true;
        //TODO: logic súng đi ra ngoài
        TF.DOLocalMove(new Vector3(0, -1, -2), 1f)
            .OnComplete(() =>
            {
                SimplePool<GameConstants.Weapon>.Despawn(this);
            });
    }


    public virtual void OnNotify(ChangeProjectileGunEvent data)
    {
    }

    #region Critical Damage Logic
    /// <summary>
    /// Phương thức phụ trợ để lấy WeakPoint component nếu active: 
    /// - Không có component WeakPoint → Mặc định là TRUE (coi như luôn active). 
    /// - Có component WeakPoint || isActive = true → TRUE. 
    /// - Có component WeakPoint || isActive = false → FALSE. 
    /// </summary>
    private WeakPoint GetWeakPointIfActive(RaycastHit hit)
    {
        if (!weaponInfo.isCritEnabled)
        {
            return null;  // Nếu crit tắt, bỏ qua
        }

        bool isOnWeakPointLayer = ((1 << hit.collider.gameObject.layer) & weaponInfo.WeakPointLayerMask.value) != 0;

        // Check layer weak point (chính)
        if (isOnWeakPointLayer)
        {
            WeakPoint weakPointComp = hit.collider.GetComponent<WeakPoint>();
            bool isActive = weakPointComp == null || weakPointComp.isActive;
            return isActive ? weakPointComp : null;  // Trả về component nếu active, null nếu không
        }

        return null;
    }

    /// <summary>
    /// Kiểm tra hit có phải weak point không (dùng LayerMask, không tag)
    /// </summary>
    protected bool IsWeakPoint(RaycastHit hit)
    {
        return GetWeakPointIfActive(hit) != null;
    }

    /// <summary>
    /// Tính damage crit nếu trúng weak point
    /// </summary>
    protected virtual int CalculateDamage(int baseDamage, RaycastHit hit)
    {
        WeakPoint weakPointComponent = GetWeakPointIfActive(hit);

        if (weakPointComponent != null)
        {
            int critDamage = (int)(baseDamage * weaponInfo.critMultiplier);
#if UNITY_EDITOR
            //Debug.Log($"Weapon {name}: CRIT HIT! Damage: {baseDamage} -> {critDamage} (x{weaponInfo.critMultiplier})");
#endif

            // Trigger event sử dụng cached component - chỉ gọi GetComponent 1 lần
            weakPointComponent.OnWeakPointDamage?.Invoke(critDamage);

            return critDamage;
        }

        return baseDamage;
    }
    #endregion
}
