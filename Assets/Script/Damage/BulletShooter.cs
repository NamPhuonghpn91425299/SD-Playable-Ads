using UnityEngine;

public class BulletShooter : MonoBehaviour
{

    public DamageSender DamageSender;
    //public DamageSettings damageSettings;  // Tham chiếu tới cài đặt sát thương
    public float maxRange = 100f;          // Tầm xa tối đa của đạn
    public LayerMask hitLayers;            // Lớp đối tượng mà đạn có thể va chạm

    private void Awake()
    {

            DamageSender = GetComponent<DamageSender>();
    }
    public void Shoot()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRange, hitLayers))
        {
            // Kiểm tra nếu đối tượng va chạm có DamageReceiver
            var target = hit.collider.GetComponent<IDamageHit>();
            if (target != null)
            {
                DealDamage(target);
            }

            // Tùy chọn: Hiển thị hiệu ứng va chạm tại vị trí trúng đạn
            ShowHitEffect(hit.point);
        }
    }

    private void DealDamage(IDamageHit target)
    {
        if (target == null) return;

        float finalDamage = DamageSender.damageAmount;

        // Tính sát thương chí mạng
        if (Random.value < DamageSender.criticalChance)
        {
            finalDamage *= DamageSender.criticalMultiplier;
        }

        target.OnHit((int)finalDamage);
    }

    private void ShowHitEffect(Vector3 position)
    {
        // Tạo hiệu ứng va chạm (nếu có) tại vị trí `position`
        // Bạn có thể tạo hiệu ứng particle hoặc bất kỳ hiệu ứng nào bạn muốn tại đây
    }
}
