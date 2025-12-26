using static GameConstants;
using UnityEngine;

public class Default_DeadExplosionHelicoter : StateBase
{
    private float timer = 0f;
    [SerializeField] private float forwardDuration = 1f;
    [SerializeField] private float fallDuration = 3f;
    [SerializeField] private LayerMask groundLayer; // Replace "Ground" with the desired layer name
    private float forwardDistance = 5f;
    private float gravity = 9f;

    private Vector3 startPos;
    private Vector3 forwardTarget;
    private bool canFall;

    public override void EnterState()
    {
        forwardDistance = Random.Range(3f, 7f);
        gravity = Random.Range(7f, 12f);
        botContext.stateController.canDead = false;
        botContext.botNetwork.BotDead();
        botContext.ChangeAnimAndType(HashDeadExplosion, 4);

        startPos = TF.position;
        forwardTarget = startPos + TF.forward * forwardDistance;
        timer = 0f;
        canFall = true;
    }

   public override void UpdateState()
    {
        timer += Time.deltaTime;
    
        // Di chuyển về phía trước trong thời gian đầu
        float forwardProgress = Mathf.Clamp01(timer / forwardDuration);
        Vector3 forwardMovement = Vector3.Lerp(startPos, forwardTarget, forwardProgress);
    
        // Luôn tính toán rơi tự do theo thời gian tổng (liên tục)
        Vector3 fallDisplacement = Vector3.down * 0.5f * gravity * timer * timer;
    
        // Kết hợp di chuyển ngang và rơi
        Vector3 newPosition = forwardMovement + fallDisplacement;
    
        // Kiểm tra nếu chạm đất với layer được truyền vào
        
        if(!canFall)
            return;
        
        if (Physics.Raycast(newPosition, Vector3.down, out RaycastHit hit, 0.2f, groundLayer))
        {
            // Nếu chạm đất, dừng rơi và đặt vị trí tại điểm chạm
            TF.position = hit.point;
            canFall = false;
            botContext.ChangeAnimAndType(HashDead,2);
            botContext.botNetwork.OnDespawn(3f);
            return;
        }
        
        // Cập nhật vị trí nếu chưa chạm đất
        TF.position = newPosition;
    
        // Khi hết thời gian rơi thì despawn
        if (timer >= (forwardDuration + fallDuration))
        {
            botContext.botNetwork.OnDespawn(0f);
        }
    }

    public override void ExitState()
    {
        // Có thể reset biến nếu cần
    }
}