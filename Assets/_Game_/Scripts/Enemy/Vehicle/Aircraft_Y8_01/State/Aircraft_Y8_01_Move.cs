using System.Collections;
using static GameConstants;
using UnityEngine;

public class Aircraft_Y8_01_Move : StateBase
{
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của bot.")] 
    [SerializeField] private float moveSpeed = 5.0f;

    [Tooltip("Tốc độ xoay của bot khi đổi hướng.")]
    [SerializeField] private float rotationSpeed = 10.0f;

    [Header("Pathing Info (Read-Only)")] 
    [Tooltip("Tuyến đường mà bot này đang đi theo.")] [SerializeField] private PointGroup assignedPath; // Để debug trong Inspector

    public BotIdentity botIdentity; // Tham chiếu đến BotIdentity để lấy thông tin về đường đi
    [SerializeField] private int currentPointIndex = 0; // Điểm tiếp theo cần đến

    [Header("Drop Troops")] 
    public Transform[] spawnPoints;
    public BotDefinition botDefinitionDropped;
    [Tooltip("Là số trên đường raycat di chuyển của nó")]public int indexPointsDropped;
    public int countDropped;
    public float cooldownDropped;
    private Coroutine droptroopCoroutine;
    private void OnEnable()
    {
        //assignedPath = botIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
        currentPointIndex = 0;
    }

    public override void EnterState()
    {
        //assignedPath = botIdentity.AssignedPath;
        Invoke(nameof(Init), .1f);
    }

    void Init()
    {
        if (botContext.botNetwork.IsDeadExplosion||botContext.botNetwork.IsDead)
            return;
        assignedPath = botIdentity.AssignedPath; // Lấy đường đi từ BotIdentity
        
        // Smooth initial rotation to first point
        if (assignedPath != null && assignedPath.points.Count > 0)
        {
            Vector3 direction = (assignedPath.points[currentPointIndex].position - TF.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                TF.rotation = targetRotation; // Initial rotation without animation
            }
        }
        botContext.ChangeAnimAndType(HashMove, 0);
    }

    public override void UpdateState()
    {
        // Nếu không ở trạng thái di chuyển, hoặc không có đường đi, thì không làm gì cả.
        if ( assignedPath == null || assignedPath.points.Count == 0||currentPointIndex >= assignedPath.points.Count)
        {
//            Debug.LogError($"Bot '{gameObject.name}' không có tuyến đường để di chuyển.");
            return;
        }

        // Move towards target point
        TF.position = Vector3.MoveTowards(TF.position, assignedPath.points[currentPointIndex].position,
            moveSpeed * Time.deltaTime);

        // Smooth rotation towards current target
        Vector3 targetDirection = assignedPath.points[currentPointIndex].position - TF.position;
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            TF.rotation = Quaternion.Slerp(TF.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Check if reached current point
        if (Vector3.Distance(TF.position, assignedPath.points[currentPointIndex].position) < .1f)
        {
            currentPointIndex++;
            if (currentPointIndex == indexPointsDropped)
                droptroopCoroutine = StartCoroutine(IEDropTroop());
            if (currentPointIndex >= assignedPath.points.Count)
                botContext.botNetwork.OnDespawn(0f);
            // No need for immediate LookAt here since smooth rotation handles it
        }
    }

    private IEnumerator IEDropTroop()
    {
        if (botIdentity.AssignedPath.PointChindCanMove.Count <= 0)
        {
            Debug.Log("Null PointChindCanMove");
            yield break;
        }
        for (int i = 0; i < countDropped; i++)
        {
            EnemyBase enemyBase = BotSpawnManager.Instance.ExecuteSpawnOrder(botDefinitionDropped, spawnPoints[Random.Range(0,spawnPoints.Length)], botIdentity.AssignedPath.PointChindCanMove[i],false);
            enemyBase.TF.parent = null;
            enemyBase.OnInit();
            enemyBase.GetComponent<PlayitaNhaydu>()?.SetupPointSpawnInfantry(botIdentity.AssignedPath.PointChindCanMove[i]);
            yield return HelperCoroutine.GetWait(cooldownDropped);
        }

        droptroopCoroutine = null;
    }

    public override void ExitState()
    {
        if (droptroopCoroutine != null)
            StopCoroutine(droptroopCoroutine);
    }
}