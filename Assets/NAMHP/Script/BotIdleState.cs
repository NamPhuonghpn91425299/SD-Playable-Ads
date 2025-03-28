using UnityEngine;

public class BotIdleState : MonoBehaviour, IBotState
{
    [SerializeField] private BotAI bot;
    [SerializeField] private float idleTime = 2f; // Time to idle at patrol point
    [SerializeField] private float idleTimer;

    public BotIdleState(BotAI bot)
    {
        this.bot = bot;
    }

    public void UpdateState()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            bot.SetState(bot.PatrolState);
        }
    }

    public void EnterState()
    {
        idleTimer = 0f;
    }
}