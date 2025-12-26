using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    [Header("Weak Point Config")]
    public bool isActive = true;  // Bật/tắt weak point này (có thể thay đổi runtime qua script enemy)

    [Header("Debug")]
    public bool showDebugLogs = true;  // Enable debug logs for testing

    public System.Action<int> OnWeakPointDamage;  // Để enemy xử lý extra logic (ví dụ: stagger)

    void OnEnable()
    {
        OnWeakPointDamage += HandleWeakPointDamage;
    }

    void OnDisable()
    {
        OnWeakPointDamage -= HandleWeakPointDamage;
    }

    private void HandleWeakPointDamage(int damage)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Weak point {gameObject.name} received damage: {damage}", gameObject);
        }
    }

}