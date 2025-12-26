using static GameConstants;
using UnityEngine;

public class Default_DeadExplosion : StateBase
{
    public override void EnterState()
    {
        botContext.ChangeAnimAndType(HashDeadExplosion,GetHitDirectionAnim(botContext.botNetwork.posExplosion));
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
        botContext.botNetwork.OnDespawn(3f);
    }

    /// <summary>
    /// Xác định hướng vụ nổ để chọn animation bay tương ứng (0: Trước, 1: Phải, 2: Trái, 3: Sau)
    /// </summary>
    public int GetHitDirectionAnim(Vector3 explosionPos)
    {
        // Vector từ vị trí vụ nổ đến nhân vật (hướng vụ nổ "tác động" tới nhân vật)
        Vector3 toSelf = (TF.position - explosionPos).normalized;

        // Các hướng local của nhân vật
        Vector3 forward = TF.forward;
        Vector3 right = TF.right;

        // Tính dot để xác định hướng gần nhất
        float dotForward = Vector3.Dot(toSelf, forward); // 1 = đối mặt, -1 = lưng
        float dotRight = Vector3.Dot(toSelf, right);     // 1 = phải, -1 = trái

        // So sánh độ lớn dot để xác định hướng ưu tiên (trục nào chiếm ưu thế)
        if (Mathf.Abs(dotForward) >= Mathf.Abs(dotRight))
        {
            // Vụ nổ ở trục Z là chính (trước/sau)
            return dotForward >= 0 ? 0 : 3; // 0 = Trước, 3 = Sau
        }
        else
        {
            // Vụ nổ ở trục X là chính (trái/phải)
            return dotRight >= 0 ? 1 : 2; // 1 = Phải, 2 = Trái
        }
    }
}