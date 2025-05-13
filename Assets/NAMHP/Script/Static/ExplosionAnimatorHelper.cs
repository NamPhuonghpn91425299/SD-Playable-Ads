using UnityEngine;

public enum ExplodeDirection { Front, Back, Left, Right } // Giữ lại nếu bạn vẫn dùng cho cách discrete

// Hashes cho discrete triggers (nếu vẫn dùng)
public static class ExplosionAnimHashes
{
    public static readonly int Explode_Front = Animator.StringToHash("Explode_Front");
    public static readonly int Explode_Back  = Animator.StringToHash("Explode_Back");
    public static readonly int Explode_Left  = Animator.StringToHash("Explode_Left");
    public static readonly int Explode_Right = Animator.StringToHash("Explode_Right");

    // Parameters cho Blend Tree (Đảm bảo tên khớp với Animator Controller của Bot)
    public static readonly int HitDirX = Animator.StringToHash("HitDirX");
    public static readonly int HitDirZ = Animator.StringToHash("HitDirZ");
    public static readonly int IsDeadBool = Animator.StringToHash("isDead"); // Nếu dùng bool
    public static readonly int DieTrigger = Animator.StringToHash("DieTrigger"); // Nếu dùng trigger
}

public static class ExplosionAnimatorHelper
{
    /// <summary>
    /// Sets Animator parameters on the target's Animator for a Blend Tree death animation.
    /// Calculates the impact direction in the target's local space.
    /// </summary>
    /// <param name="targetAnimator">The Animator component of the target bot.</param>
    /// <param name="explosionOrigin">The Transform of the explosion's source (e.g., StaticController).</param>
    /// <param name="targetTransform">The Transform of the target bot.</param>
    public static void PlayExplosionBlendForTarget(Animator targetAnimator, Transform explosionOrigin, Transform targetTransform)
    {
        // Vector từ vụ nổ (origin) đến mục tiêu (target) trong world space
        Vector3 directionFromExplosionToTarget_World = (targetTransform.position - explosionOrigin.position).normalized;

        // Chuyển vector này sang không gian local của MỤC TIÊU (target)
        // Điều này cho chúng ta biết vụ nổ đến từ hướng nào SO VỚI MẶT CỦA MỤC TIÊU
        Vector3 impactDirection_TargetLocal = targetTransform.InverseTransformDirection(directionFromExplosionToTarget_World);

        // (Tùy chọn) "Snap" các giá trị nếu bạn muốn hướng rõ ràng hơn là pha trộn hoàn toàn
        float hitX = 0f;
        float hitZ = 0f;
        float absLocalX = Mathf.Abs(impactDirection_TargetLocal.x);
        float absLocalZ = Mathf.Abs(impactDirection_TargetLocal.z);

        // Ưu tiên hướng có thành phần local lớn hơn
        if (absLocalX > absLocalZ)
        {
            hitX = Mathf.Sign(impactDirection_TargetLocal.x); // -1 (trái của target) hoặc 1 (phải của target)
        }
        else if (absLocalZ > absLocalX)
        {
            hitZ = Mathf.Sign(impactDirection_TargetLocal.z); // -1 (sau lưng target) hoặc 1 (trước mặt target)
        }
        else if (absLocalX > 0.05f) // Nếu gần bằng nhau và không phải zero (hướng chéo)
        {
            hitX = Mathf.Sign(impactDirection_TargetLocal.x);
            hitZ = Mathf.Sign(impactDirection_TargetLocal.z);
        }
        // Nếu cả x và z đều gần zero (ví dụ nổ ngay tâm trên/dưới), bạn có thể mặc định
        // ví dụ: hitZ = -1 (ngã ngửa) hoặc dựa vào impactDirection_TargetLocal.y

        // Đặt parameters cho blend tree trên Animator của mục tiêu
        targetAnimator.SetFloat(ExplosionAnimHashes.HitDirX, hitX); // Hoặc impactDirection_TargetLocal.x để pha trộn tự do
        targetAnimator.SetFloat(ExplosionAnimHashes.HitDirZ, hitZ); // Hoặc impactDirection_TargetLocal.z để pha trộn tự do

        // Ghi chú: Việc kích hoạt bool "isDead" hoặc trigger "DieTrigger"
        // nên được thực hiện trong StaticController.ExplodeTargets() ngay trước khi gọi hàm này,
        // sau khi đã xác định bot thực sự chết.
    }
}