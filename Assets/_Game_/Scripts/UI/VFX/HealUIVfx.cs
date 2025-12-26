using System.Collections;
using System.Collections.Generic;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using UnityEngine.UI;

// hổi máu vfx
public class HealUIVfx : VFXBase
{
    private Animator _animator;
    [SerializeField] private float _coundDownTime = 30f; // Thời gian sau khi hồi máu để reload lại repair kit
    [SerializeField] private Text _text;
    private bool _isActive = true;
    [SerializeField] private GameObject _lockedIcon; // Icon hiển thị khi không thể hồi máu
    [SerializeField] private int _maxHealAmount = 50; // Số máu tối đa có thể hồi
    [SerializeField] private float _healPerSecond = 2f; // Số máu hồi mỗi giây
    [SerializeField] private float _healInterval = 0.5f; // Khoảng thời gian giữa mỗi lần hồi (giây)
    float timer;
    private float _healTimer; // Timer cho việc hồi máu
    private float _totalHealedAmount; // Tổng số máu đã hồi

    private void Start()
    {
        this._animator = GetComponent<Animator>();
        // Only set to true if it's not already set to false by previous Play() call
        if (_isActive == true) // Keep current state if it was already set to false
        {
            _isActive = true; // Force set to true only if it's not false
        }
        Debug.Log($"Start - _isActive maintained as: {_isActive}");
    }

    void Update()
    {
        if (!_isActive)
        {
            if (timer >= _coundDownTime)
            {
                _isActive = true;
                timer = 0f;
                _healTimer = 0f;
                _totalHealedAmount = 0f;
                _lockedIcon.SetActive(false); // Hide the locked icon when ready to heal
                _text.text = "30s"; // Reset text to show countdown
                Debug.Log("Update - _isActive reset to true after countdown");
            }
            else
            {
                timer += Time.deltaTime;
                _text.text = (30 - timer).ToString("F1") + "s";
                
                // Hồi máu theo thời gian thực
                _healTimer += Time.deltaTime;
                if (_healTimer >= _healInterval && _totalHealedAmount < _maxHealAmount)
                {
                    float healAmount = _healPerSecond * _healInterval;
                    
                    // Đảm bảo không vượt quá maxHealAmount
                    if (_totalHealedAmount + healAmount > _maxHealAmount)
                    {
                        healAmount = _maxHealAmount - _totalHealedAmount;
                    }
                    
                    _totalHealedAmount += healAmount;
                    _healTimer = 0f;
                    
                    // Publish event để hồi máu
                    EventManager.Instance?.Publish(new PlayerHealthChangedEvent(
                        state: "Heal", 
                        damage: Mathf.RoundToInt(healAmount)
                    ));
                    
                    Debug.Log($"Healing: +{healAmount:F1} HP (Total: {_totalHealedAmount:F1}/{_maxHealAmount})");
                }
            }
        }
    }

    public override void Play<T>(T parameter)
    {
        Debug.Log($"HealUIVfx Play called - _isActive before: {_isActive}");
        if (_isActive)
        {
            _animator.Play(parameter.ToString(), 0, 0f); // layer 0, normalized time 0
            _isActive = false;
            _healTimer = 0f; // Reset heal timer
            _totalHealedAmount = 0f; // Reset total healed amount
            _lockedIcon.SetActive(true); // Show the locked icon when healing is not available
            Debug.Log($"HealUIVfx Play executed - _isActive after: {_isActive}");
            Debug.Log($"Starting heal process: {_healPerSecond} HP/sec for {_coundDownTime} seconds (Max: {_maxHealAmount} HP)");
        }
        else
        {
            Debug.Log("HealUIVfx Play skipped - _isActive is false");
        }
    }
}
