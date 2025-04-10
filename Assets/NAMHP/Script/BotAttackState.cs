using UnityEngine;

public class BotAttackState : MonoBehaviour, IBotState
{
    [SerializeField] private BotAI bot;
    [SerializeField] private float bodyRotationSpeed = 5f; // Xoay ngang
    [SerializeField] private float upperBodyRotationSpeed = 5f; // Xoay lên/xuống
    [SerializeField] private Transform upperBody;

    public void EnterState()
    {
        Debug.Log("Bot bắt đầu tấn công!");
    }

    public void UpdateState()
    {
        if (bot == null || bot.player == null) return;

        RotateBody();
        RotateUpperBody();
    }

    /// <summary>
    /// Xoay toàn thân về phía player (chỉ xoay ngang - Y)
    /// </summary>
    void RotateBody()
    {
        Vector3 direction = bot.player.position - bot.transform.position;
        direction.y = 0; // Giữ nguyên chiều cao

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            bot.transform.rotation = Quaternion.Slerp(bot.transform.rotation, targetRotation, Time.deltaTime * bodyRotationSpeed);
        }
    }

    /// <summary>
    /// Xoay thân trên để nhắm lên/xuống đúng hướng
    /// </summary>
    void RotateUpperBody()
    {
        if (upperBody == null) return;

        Vector3 targetDirection = bot.player.position - upperBody.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        // Chỉ xoay theo trục X (nghiêng lên/xuống)
        Vector3 eulerAngles = targetRotation.eulerAngles;
        eulerAngles.y = upperBody.eulerAngles.y; // Giữ nguyên xoay ngang
        upperBody.rotation = Quaternion.Slerp(upperBody.rotation, Quaternion.Euler(eulerAngles), Time.deltaTime * upperBodyRotationSpeed);
    }

    /// <summary>
    /// Khi bot rời trạng thái tấn công, đưa góc X của upperBody về 0
    /// </summary>
    public void ExitState()
    {
        if (upperBody != null)
        {
            Vector3 eulerAngles = upperBody.localEulerAngles;
            eulerAngles.x = Mathf.LerpAngle(eulerAngles.x, 0, Time.deltaTime * upperBodyRotationSpeed);
            upperBody.localRotation = Quaternion.Euler(eulerAngles);
        }
    }
}
