using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using UnityEngine.UI;

public class AimUIVfx : VFXBase
{
    [SerializeField] private Sprite _sprite_no_aim;
    [SerializeField] private Sprite _sprite_aim;
    [SerializeField] private Slider _slider_aim;
    [SerializeField] private Image _image_aim;
    [SerializeField] private Camera _camera;
    [SerializeField] private Camera _cameraWeapon;
    [SerializeField] private Text _text_aim;

    private bool _isAiming = false; // Track current state
    private float _targetFOV = 50f; // Mục tiêu FOV
    private float _smoothSpeed = 5f; // Tốc độ mượt
    
    private float _targetFOV_WP = 55f; // Mục tiêu FOV

    public override void Play<T>(T parameter)
    {
        // Toggle the aiming state
        _isAiming = !_isAiming;

        // Update UI based on new state
        _image_aim.sprite = _isAiming ? _sprite_aim : _sprite_no_aim;
        _slider_aim.gameObject.SetActive(_isAiming);
        Debug.Log($"Aim State Changed: {_isAiming}");
    }

    void Update()
    {
        if (_isAiming)
        {
            // Tính toán target FOV khi đang aim
            _targetFOV = Mathf.Lerp(50f, 20f, (_slider_aim.value + 25f) / 25f);
            _text_aim.text = $"{((_slider_aim.value + 25f) / 10f):F1}X";
        }
        else
        {
            _targetFOV = 50f; // Reset về FOV mặc định
        }

        // Chuyển FOV hiện tại về targetFOV một cách mượt
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFOV, Time.deltaTime * _smoothSpeed);
        
        float targetWeaponFOV = _isAiming ? 45f : 55f;
        _cameraWeapon.fieldOfView = Mathf.Lerp(_cameraWeapon.fieldOfView, targetWeaponFOV, Time.deltaTime * _smoothSpeed);
    }
}
