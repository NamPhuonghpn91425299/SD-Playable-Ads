using UnityEngine;

public class DamageSender : MonoBehaviour
{
    //[SerializeField] private DamageSettings damageSettings; // Tham chiếu đến ScriptableObject chứa thông tin sát thương
    public float damageAmount = 1f;         // Lượng sát thương
    public float criticalMultiplier = 2f;  // Hệ số nhân khi gây sát thương chí mạng
    public float criticalChance = 0.1f;
    // Gọi phương thức này để gây sát thương cho đối tượng
    public void DealDamage(IDamageHit target)
    {
        if (target == null) return;

        float finalDamage = damageAmount;

        // Tính toán sát thương chí mạng
        if (Random.value < criticalChance)
        {
            finalDamage *= criticalMultiplier;
        }
        target.OnHit((int)finalDamage);         // Trừ máu của đối tượng nhận sát thương
    }
}
